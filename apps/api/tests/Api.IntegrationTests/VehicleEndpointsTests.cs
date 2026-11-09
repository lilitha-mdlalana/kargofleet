using System.Net;
using System.Net.Http.Json;
using Api.Data;
using Api.DTOs.Auth;
using Api.DTOs.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.IntegrationTests;

public class VehicleEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public VehicleEndpointsTests(ApiFactory factory)
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

    private async Task<string> SignupAndVerifyAsync(string phone, string name = "Jane Doe")
    {
        await _client.PostAsJsonAsync("/auth/signup", new SignupRequest(name, phone));
        var code = await GetLoggedOtpAsync(phone);
        var response = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(phone, code));
        var body = await response.Content.ReadFromJsonAsync<VerifyResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task Vehicle_MissingToken_Returns401()
    {
        var response = await _client.GetAsync("/vehicle");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Vehicle_FreshSignupNoVehicle_Returns404()
    {
        var token = await SignupAndVerifyAsync(NewPhone());

        using var request = new HttpRequestMessage(HttpMethod.Get, "/vehicle");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Vehicle_SeededDemoDriver_Returns200WithVehicle()
    {
        const string seededPhone = "+27821234567";
        await _client.PostAsJsonAsync("/auth/login", new LoginRequest(seededPhone));
        var code = await GetLoggedOtpAsync(seededPhone);
        var verifyResponse = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(seededPhone, code));
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<VerifyResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/vehicle");
        request.Headers.Add("Authorization", $"Bearer {verifyBody!.Token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardVehicle>();
        Assert.Equal("Toyota Quantum", body!.Name);
        Assert.Equal("CA 123-456", body.Registration);
    }
}
