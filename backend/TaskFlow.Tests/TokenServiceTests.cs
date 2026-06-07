using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;
using Xunit;

namespace TaskFlow.Tests;

public class TokenServiceTests
{
    private static TokenService MakeService() => new(Options.Create(new JwtOptions
    {
        Issuer = "TaskFlow",
        Audience = "TaskFlowClient",
        Key = "unit-test-signing-key-at-least-32-characters-long!!",
        ExpiryMinutes = 60,
    }));

    [Fact]
    public void CreateToken_embeds_user_id_and_email()
    {
        var user = new User { Email = "a@b.com", DisplayName = "Alice" };
        var jwt = MakeService().CreateToken(user);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        Assert.Equal(user.Id.ToString(), parsed.Subject);
        Assert.Contains(parsed.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "a@b.com");
        Assert.Equal("TaskFlow", parsed.Issuer);
    }

    [Fact]
    public void CreateToken_sets_future_expiry()
    {
        var jwt = MakeService().CreateToken(new User { Email = "x@y.com" });
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        Assert.True(parsed.ValidTo > DateTime.UtcNow);
    }
}
