using System.IdentityModel.Tokens.Jwt;
using Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Api.UnitTests;

public class JwtServiceTests
{
    private static JwtService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Secret"] = "test-secret-at-least-32-bytes-long!" })
            .Build();
        return new JwtService(config);
    }

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
}
