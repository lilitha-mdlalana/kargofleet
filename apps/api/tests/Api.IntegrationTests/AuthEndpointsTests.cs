using System.Net;
using System.Net.Http.Json;
using Api.Data;
using Api.DTOs.Auth;
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
    public async Task Profile_ValidToken_Returns501()
    {
        var phone = NewPhone();
        await _client.PostAsJsonAsync("/auth/signup", new SignupRequest("Jane Doe", phone));
        var code = await GetLoggedOtpAsync(phone);
        var verifyResponse = await _client.PostAsJsonAsync("/auth/verify", new VerifyRequest(phone, code));
        var verifyBody = await verifyResponse.Content.ReadFromJsonAsync<VerifyResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/profile");
        request.Headers.Add("Authorization", $"Bearer {verifyBody!.Token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
