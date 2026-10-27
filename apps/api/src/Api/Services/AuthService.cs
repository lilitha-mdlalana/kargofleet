using Api.Data;
using Api.Data.Entities;
using Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    private readonly AppDbContext _db;
    private readonly IOtpGenerator _otpGenerator;

    public AuthService(AppDbContext db, IOtpGenerator otpGenerator)
    {
        _db = db;
        _otpGenerator = otpGenerator;
    }

    public async Task<User> StartSignupAsync(string name, string phone)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        if (existing is not null)
        {
            throw new AuthException(400, "Phone already registered");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone,
            Driver = new Driver { Id = Guid.NewGuid() },
        };

        IssueOtp(user);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<User> StartLoginAsync(string phone)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        if (user is null)
        {
            throw new AuthException(404, "No account with that phone number");
        }

        IssueOtp(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<User> VerifyOtpAsync(string phone, string code)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        if (user is null)
        {
            throw new AuthException(404, "No account with that phone number");
        }

        if (user.OtpExpiresAt is null || user.OtpExpiresAt < DateTime.UtcNow || user.OtpCode != code)
        {
            throw new AuthException(401, "Invalid or expired code");
        }

        user.OtpCode = null;
        user.OtpExpiresAt = null;
        await _db.SaveChangesAsync();

        return user;
    }

    private void IssueOtp(User user)
    {
        var code = _otpGenerator.Generate();
        user.OtpCode = code;
        user.OtpExpiresAt = DateTime.UtcNow.Add(OtpTtl);
        Console.WriteLine($"OTP for {user.Phone}: {code}");
    }
}
