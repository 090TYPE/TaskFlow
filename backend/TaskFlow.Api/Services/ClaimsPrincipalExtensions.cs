using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskFlow.Api.Services;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Returns the authenticated user's id from the "sub" claim, or null if absent/invalid.</summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
