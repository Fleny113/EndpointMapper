using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace EndpointMapper.TestApplication;

public class AuthenticationEndpoint : IEndpoint
{
    [HttpMapGet("/auth"), Authorize]
    public Ok<string> Handle(ClaimsPrincipal user)
    {
        return TypedResults.Ok($"Hello {user.Identity?.Name}");
    }

    [HttpMapGet("/auth/2"), Authorize(AuthenticationSchemes = "AnotherJWT")]
    public Ok<string> HandleStrunz(ClaimsPrincipal user)
    {
        return TypedResults.Ok($"Hello {user.Identity?.Name}");
    }
}