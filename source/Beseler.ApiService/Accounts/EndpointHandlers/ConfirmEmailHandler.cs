using Beseler.ApiService.Application.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Transactions;

namespace Beseler.ApiService.Accounts.EndpointHandlers;

internal static class ConfirmEmailHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
        ConfirmEmailRequest request,
        AccountRepository repository,
        TokenService tokenService,
        TokenRepository tokenRepository,
        CookieService cookieService,
        CancellationToken stoppingToken)
    {
        var principal = await tokenService.ValidateAsync(request.ConfirmationCode);

        if (int.TryParse(principal?.Identity?.Name, out var accountId) is false)
            return TypedResults.Unauthorized();

        if (principal.HasClaim(AppClaims.ConfirmEmail(tokenService.Audience)) is false)
            return TypedResults.Unauthorized();

        if (await repository.GetByIdAsync(accountId, stoppingToken) is not { } account)
            return TypedResults.Unauthorized();

        if (account.IsVerified)
            return TypedResults.BadRequest($"{account.Email} has already been verified.");

        account.Verify();

        var (_, accessToken, expiresOn) = tokenService.GenerateAccessToken(account);
        var (refreshTokenId, refreshToken, refreshExpiresOn) = tokenService.GenerateRefreshToken(account);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var saveResult = await repository.SaveChangesAsync(account, stoppingToken);
        await tokenRepository.SaveAsync(TokenLog.Create(refreshTokenId, refreshExpiresOn, account), stoppingToken);
        scope.Complete();

        cookieService.Set(CookieKeys.RefreshToken, refreshToken, refreshExpiresOn);
        var response = new AccessTokenResponse(accessToken, expiresOn);

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.Ok(response),
            onFailure: error => TypedResults.Problem(error.Message));
    }
}
