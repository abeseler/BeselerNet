using Beseler.ApiService.Application.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Beseler.ApiService.Accounts.EndpointHandlers;

internal static class RefreshTokenHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
        RefreshTokenRequest? request,
        TokenService tokenService,
        CookieService cookieService,
        AccountRepository accountRepository,
        TokenRepository tokenRepository,
        CancellationToken stoppingToken)
    {
        var token = cookieService.TryGetValue(CookieKeys.RefreshToken, out var value) ? value : request?.RefreshToken;
        if (token is null) { return TypedResults.Unauthorized(); }

        var principal = await tokenService.ValidateAsync(token!);
        if (principal?.Identity is not { IsAuthenticated: true, Name: not null } || int.TryParse(principal?.Identity?.Name, out var accountId) is false)
        {
            cookieService.Remove(CookieKeys.RefreshToken);
            return TypedResults.Unauthorized();
        }

        if (Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Jti), out var tokenId) is false)
        {
            return TypedResults.Unauthorized();
        }

        var tokenLog = await tokenRepository.GetByIdAsync(tokenId, stoppingToken);
        if (tokenLog is null) { return TypedResults.Unauthorized(); }

        if (tokenLog.IsRevoked || tokenLog.HasBeenReplaced)
        {
            await tokenRepository.RevokeChainAsync(tokenLog.TokenId, stoppingToken);
            cookieService.Remove(CookieKeys.RefreshToken);
            return TypedResults.Unauthorized();
        }

        var account = await accountRepository.GetByIdAsync(accountId, stoppingToken);
        if (account is not { IsLocked: false })
        {
            cookieService.Remove(CookieKeys.RefreshToken);
            return TypedResults.Forbid();
        }

        var (_, accessToken, expiresOn) = tokenService.GenerateAccessToken(account);
        var (refreshTokenId, refreshToken, refreshExpiresOn) = tokenService.GenerateRefreshToken(account);

        tokenLog.ReplacedBy(refreshTokenId);
        await tokenRepository.SaveAsync(tokenLog, stoppingToken);
        await tokenRepository.SaveAsync(TokenLog.Create(refreshTokenId, refreshExpiresOn, account), stoppingToken);

        cookieService.Set(CookieKeys.RefreshToken, refreshToken, refreshExpiresOn);
        return TypedResults.Ok(new AccessTokenResponse(accessToken, expiresOn));
    }
}
