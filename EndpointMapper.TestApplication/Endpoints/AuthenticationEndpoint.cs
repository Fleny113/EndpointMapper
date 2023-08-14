using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace EndpointMapper.TestApplication.Endpoints;

public class AuthenticationEndpoint : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/auth"), Authorize]
    public static Ok<string> Handle(ClaimsPrincipal user) => TypedResults.Ok($"Hello {user.Identity?.Name}");

    [HttpMap(HttpMapMethod.Get, "/auth/2"), Authorize(AuthenticationSchemes = "AnotherJWT")]
    public static Ok<string> HandleWithAnotherAuthScheme(ClaimsPrincipal user) => TypedResults.Ok($"Hello {user.Identity?.Name}");
}