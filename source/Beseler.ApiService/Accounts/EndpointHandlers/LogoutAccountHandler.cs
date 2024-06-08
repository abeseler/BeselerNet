using Beseler.ApiService.Application.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Beseler.ApiService.Accounts.EndpointHandlers;

internal static class LogoutAccountHandler
{
    [Authorize]
    public static async Task<IResult> HandleAsync(TokenService tokenService, TokenRepository tokenRepository, CookieService cookieService, CancellationToken stoppingToken)
    {
        if (cookieService.TryGetValue(CookieKeys.RefreshToken, out var refreshToken))
        {
            var principal = await tokenService.ValidateAsync(refreshToken!);
            if (Guid.TryParse(principal?.FindFirstValue(JwtRegisteredClaimNames.Jti), out var tokenId))
            {
                await tokenRepository.RevokeChainAsync(tokenId, stoppingToken);
            }
        }

        cookieService.Remove(CookieKeys.RefreshToken);

        return TypedResults.NoContent();
    }
}
