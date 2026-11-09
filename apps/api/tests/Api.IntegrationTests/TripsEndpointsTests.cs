using System.Net;
using System.Net.Http.Json;
using Api.Data;
using Api.Data.Entities;
using Api.DTOs.Auth;
using Api.DTOs.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.IntegrationTests;

public class TripsEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public TripsEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string NewPhone() => $"+2782{Random.Shared.Next(1000000, 9999999)}";

    private async Task<string?> GetLoggedOtpAsync(string phone)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        return user?.OtpCode;
    }

    private async Task<(string Token, Guid UserId)> SignupAndVerifyAsync(string phone, string name = "Jane Doe")
    {
        await _client.PostAsJsonAsync("/auth/signup", new SignupRequest(name, phone));
        var code = await GetLoggedOtpAsync(phone);
        var response = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(phone, code));
        var body = await response.Content.ReadFromJsonAsync<VerifyResponse>();
        return (body!.Token, body.User.Id);
    }

    private async Task<Guid> SeedTripAsync(Guid userId, string status)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var driver = await db.Drivers.FirstAsync(d => d.UserId == userId);
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Test Van",
            Registration = $"TV-{Guid.NewGuid():N}"[..10],
        };
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DriverId = driver.Id,
            VehicleId = vehicle.Id,
            Origin = "A",
            Destination = "B",
            Status = status,
        };
        db.Vehicles.Add(vehicle);
        db.Trips.Add(trip);
        await db.SaveChangesAsync();
        return trip.Id;
    }

    [Fact]
    public async Task Start_ScheduledTrip_Returns200AndActivatesWithStartedAt()
    {
        var (token, userId) = await SignupAndVerifyAsync(NewPhone());
        var tripId = await SeedTripAsync(userId, "scheduled");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{tripId}/start");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardTrip>();
        Assert.Equal("active", body!.Status);
        Assert.NotNull(body.StartedAt);
    }

    [Fact]
    public async Task Start_ActiveTrip_Returns400()
    {
        var (token, userId) = await SignupAndVerifyAsync(NewPhone());
        var tripId = await SeedTripAsync(userId, "active");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{tripId}/start");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Start_CompletedTrip_Returns400()
    {
        var (token, userId) = await SignupAndVerifyAsync(NewPhone());
        var tripId = await SeedTripAsync(userId, "completed");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{tripId}/start");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Start_NonexistentTrip_Returns404()
    {
        var (token, _) = await SignupAndVerifyAsync(NewPhone());

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{Guid.NewGuid()}/start");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Start_AnotherDriversTrip_Returns404()
    {
        var (_, userIdA) = await SignupAndVerifyAsync(NewPhone());
        var (tokenB, _) = await SignupAndVerifyAsync(NewPhone());
        var tripId = await SeedTripAsync(userIdA, "scheduled");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{tripId}/start");
        request.Headers.Add("Authorization", $"Bearer {tokenB}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Start_MissingToken_Returns401()
    {
        var response = await _client.PostAsync($"/trips/{Guid.NewGuid()}/start", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task End_ActiveTrip_Returns200AndCompletesWithEndedAt()
    {
        var (token, userId) = await SignupAndVerifyAsync(NewPhone());
        var tripId = await SeedTripAsync(userId, "active");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{tripId}/end");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardTrip>();
        Assert.Equal("completed", body!.Status);
        Assert.NotNull(body.EndedAt);
    }

    [Fact]
    public async Task End_ScheduledTrip_Returns400()
    {
        var (token, userId) = await SignupAndVerifyAsync(NewPhone());
        var tripId = await SeedTripAsync(userId, "scheduled");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{tripId}/end");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task End_NonexistentTrip_Returns404()
    {
        var (token, _) = await SignupAndVerifyAsync(NewPhone());

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/trips/{Guid.NewGuid()}/end");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task End_MissingToken_Returns401()
    {
        var response = await _client.PostAsync($"/trips/{Guid.NewGuid()}/end", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
