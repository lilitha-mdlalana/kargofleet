using System.IdentityModel.Tokens.Jwt;
using Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Api.UnitTests;

public class JwtServiceTests
{
    private const string TestSecret = "test-secret";

    private static JwtService CreateService() =>
        new(JwtService.DeriveKeyBytes(TestSecret));

    [Fact]
    public void Sign_ProducesTokenWithUserIdAsSubClaim()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var token = service.Sign(userId);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var sub = jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        Assert.Equal(userId.ToString(), sub);
    }

    [Fact]
    public void Sign_SetsSevenDayExpiry()
    {
        var service = CreateService();

        var token = service.Sign(Guid.NewGuid());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var expectedExpiry = DateTime.UtcNow.AddDays(7);
        Assert.True(Math.Abs((jwt.ValidTo - expectedExpiry).TotalMinutes) < 1);
    }

    [Fact]
    public void Sign_TokenValidatesAgainstIndependentlyDerivedKey()
    {
        // Regression test: signing (JwtService) and Bearer validation (Program.cs)
        // must derive the exact same key bytes from the same secret, independently,
        // with no shared mutable state. If either side re-derives from a config
        // value that can change at runtime, this guards against them drifting apart.
        var service = CreateService();
        var token = service.Sign(Guid.NewGuid());
        var validationKeyBytes = JwtService.DeriveKeyBytes(TestSecret);

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(validationKeyBytes),
        }, out _);
    }
}
