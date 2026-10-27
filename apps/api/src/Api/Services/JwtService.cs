using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public class JwtService : IJwtService
{
    private readonly string _secret;

    public JwtService(IConfiguration config)
    {
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? config["Jwt:Secret"]
            ?? "dev-secret-change-me";
    }

    public string Sign(Guid userId)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) },
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return handler.WriteToken(token);
    }
}
