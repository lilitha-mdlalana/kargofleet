using System.Net;
using System.Net.Http.Json;
using Api.Data;
using Api.DTOs.Auth;
using Api.DTOs.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Api.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(ApiFactory factory)
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
    public async Task Signup_WithNewPhone_Returns202()
    {
        var phone = NewPhone();

        var response = await _client.PostAsJsonAsync("/auth/signup", new SignupRequest("Jane Doe", phone));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MessageResponse>();
        Assert.Equal("OTP sent", body!.Message);
    }

    [Fact]
    public async Task Signup_MissingName_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/signup", new SignupRequest(null, NewPhone()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Signup_DuplicatePhone_Returns400()
    {
        var phone = NewPhone();
        await _client.PostAsJsonAsync("/auth/signup", new SignupRequest("Jane Doe", phone));

        var response = await _client.PostAsJsonAsync("/auth/signup", new SignupRequest("Someone Else", phone));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownPhone_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(NewPhone()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Login_KnownPhone_Returns202()
    {
        var phone = NewPhone();
        await _client.PostAsJsonAsync("/auth/signup", new SignupRequest("Jane Doe", phone));

        var response = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(phone));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Verify_CorrectCode_Returns200WithTokenAndUser()
    {
        var phone = NewPhone();
        await _client.PostAsJsonAsync("/auth/signup", new SignupRequest("Jane Doe", phone));
        var code = await GetLoggedOtpAsync(phone);

        var response = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(phone, code));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<VerifyResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(phone, body.User.Phone);
        Assert.Equal("Jane Doe", body.User.Name);
    }

    [Fact]
    public async Task Verify_WrongCode_Returns401()
    {
        var phone = NewPhone();
        await _client.PostAsJsonAsync("/auth/signup", new SignupRequest("Jane Doe", phone));

        var response = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(phone, "000000"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Verify_UnknownPhone_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(NewPhone(), "123456"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Profile_MissingToken_Returns401()
    {
        var response = await _client.GetAsync("/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Message));
    }

    [Fact]
    public async Task Profile_MalformedToken_Returns401()
    {
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer not-a-real-token");

        var response = await _client.GetAsync("/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_CrossOriginRequest_AllowsAnyOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://example.com");

        var response = await _client.SendAsync(request);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Profile_ValidToken_Returns501()
    {
        var token = await SignupAndVerifyAsync(NewPhone());

        using var request = new HttpRequestMessage(HttpMethod.Get, "/profile");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_MissingToken_Returns401()
    {
        var response = await _client.GetAsync("/driver/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_FreshSignup_Returns200WithDriverInfoAndNullVehicleAndTrip()
    {
        var phone = NewPhone();
        var token = await SignupAndVerifyAsync(phone, "Fresh Driver");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/driver/dashboard");
        request.Headers.Add("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardResponse>();
        Assert.Equal("Fresh Driver", body!.Driver.Name);
        Assert.Equal(phone, body.Driver.Phone);
        Assert.Null(body.Vehicle);
        Assert.Null(body.Trip);
    }

    [Fact]
    public async Task Dashboard_SeededDemoDriver_Returns200WithVehicleAndTrip()
    {
        const string seededPhone = "+27821234567";
        await _client.PostAsJsonAsync("/auth/login", new LoginRequest(seededPhone));
        var code = await GetLoggedOtpAsync(seededPhone);
        var verifyResponse = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(seededPhone, code));
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<VerifyResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/driver/dashboard");
        request.Headers.Add("Authorization", $"Bearer {verifyBody!.Token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<DashboardResponse>();
        Assert.NotNull(body!.Vehicle);
        Assert.Equal("Toyota Quantum", body.Vehicle!.Name);
        Assert.Equal("CA 123-456", body.Vehicle.Registration);
        Assert.NotNull(body.Trip);
        Assert.Equal("Cape Town", body.Trip!.Origin);
        Assert.Equal("Stellenbosch", body.Trip.Destination);
        Assert.Equal("scheduled", body.Trip.Status);
    }
}
