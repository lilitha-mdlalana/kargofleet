using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public class JwtService : IJwtService
{
    private readonly byte[] _keyBytes;

    public JwtService(IConfiguration config)
    {
        _keyBytes = DeriveKeyBytes(ResolveSecret(config));
    }

    public static string ResolveSecret(IConfiguration config) =>
        Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? config["Jwt:Secret"]
            ?? "dev-secret-change-me";

    // HS256 requires a key of at least 256 bits; hash the configured secret
    // down to a fixed 32-byte key so any secret length works.
    public static byte[] DeriveKeyBytes(string secret) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(secret));

    public string Sign(Guid userId)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(_keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) },
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return handler.WriteToken(token);
    }
}
