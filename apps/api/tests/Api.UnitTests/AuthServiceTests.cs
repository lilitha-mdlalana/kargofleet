using Api.Data;
using Api.Exceptions;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.UnitTests;

public class AuthServiceTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private class FakeOtpGenerator : IOtpGenerator
    {
        public string Code { get; set; } = "123456";
        public string Generate() => Code;
    }

    [Fact]
    public async Task StartSignupAsync_CreatesUserWithDriverAndOtp()
    {
        using var db = CreateDb();
        var service = new AuthService(db, new FakeOtpGenerator { Code = "654321" });

        var user = await service.StartSignupAsync("Thando Mokoena", "+27821112222");

        Assert.Equal("654321", user.OtpCode);
        Assert.NotNull(user.OtpExpiresAt);
        Assert.NotNull(user.Driver);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task StartSignupAsync_DuplicatePhone_Throws400()
    {
        using var db = CreateDb();
        var service = new AuthService(db, new FakeOtpGenerator());
        await service.StartSignupAsync("Existing", "+27820000000");

        var ex = await Assert.ThrowsAsync<AuthException>(
            () => service.StartSignupAsync("Someone Else", "+27820000000"));

        Assert.Equal(400, ex.Status);
    }

    [Fact]
    public async Task StartLoginAsync_UnknownPhone_Throws404()
    {
        using var db = CreateDb();
        var service = new AuthService(db, new FakeOtpGenerator());

        var ex = await Assert.ThrowsAsync<AuthException>(
            () => service.StartLoginAsync("+27829999999"));

        Assert.Equal(404, ex.Status);
    }

    [Fact]
    public async Task StartLoginAsync_KnownPhone_RegeneratesOtp()
    {
        using var db = CreateDb();
        var otp = new FakeOtpGenerator { Code = "111111" };
        var service = new AuthService(db, otp);
        var user = await service.StartSignupAsync("Thando", "+27821234567");

        otp.Code = "222222";
        var updated = await service.StartLoginAsync("+27821234567");

        Assert.Equal("222222", updated.OtpCode);
        Assert.Equal(user.Id, updated.Id);
    }

    [Fact]
    public async Task VerifyOtpAsync_CorrectCode_ClearsOtpAndReturnsUser()
    {
        using var db = CreateDb();
        var service = new AuthService(db, new FakeOtpGenerator { Code = "123456" });
        await service.StartSignupAsync("Thando", "+27821234567");

        var user = await service.VerifyOtpAsync("+27821234567", "123456");

        Assert.Null(user.OtpCode);
        Assert.Null(user.OtpExpiresAt);
    }

    [Fact]
    public async Task VerifyOtpAsync_WrongCode_Throws401()
    {
        using var db = CreateDb();
        var service = new AuthService(db, new FakeOtpGenerator { Code = "123456" });
        await service.StartSignupAsync("Thando", "+27821234567");

        var ex = await Assert.ThrowsAsync<AuthException>(
            () => service.VerifyOtpAsync("+27821234567", "000000"));

        Assert.Equal(401, ex.Status);
    }

    [Fact]
    public async Task VerifyOtpAsync_ExpiredCode_Throws401()
    {
        using var db = CreateDb();
        var service = new AuthService(db, new FakeOtpGenerator { Code = "123456" });
        var user = await service.StartSignupAsync("Thando", "+27821234567");
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AuthException>(
            () => service.VerifyOtpAsync("+27821234567", "123456"));

        Assert.Equal(401, ex.Status);
    }

    [Fact]
    public async Task VerifyOtpAsync_UnknownPhone_Throws404()
    {
        using var db = CreateDb();
        var service = new AuthService(db, new FakeOtpGenerator());

        var ex = await Assert.ThrowsAsync<AuthException>(
            () => service.VerifyOtpAsync("+27820000000", "123456"));

        Assert.Equal(404, ex.Status);
    }
}
