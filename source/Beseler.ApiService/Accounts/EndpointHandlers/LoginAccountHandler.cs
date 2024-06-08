using Beseler.ApiService.Application.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Beseler.ApiService.Accounts.EndpointHandlers;

internal static class LoginAccountHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
        LoginAccountRequest request,
        TokenService tokenService,
        CookieService cookieService,
        IPasswordHasher<Account> passwordHasher,
        AccountRepository accountRepository,
        TokenRepository tokenRepository,
        CancellationToken stoppingToken)
    {
        var account = await accountRepository.GetByEmailAsync(request.Email, stoppingToken);
        if (account is null) { return TypedResults.Unauthorized(); }
        if (account.IsLocked) { return TypedResults.Forbid(); }

        var passwordCheckResult = passwordHasher.VerifyHashedPassword(account, account.SecretHash ?? "", request.Secret);
        if (passwordCheckResult is not PasswordVerificationResult.Success)
        {
            account.FailedLogin();

            using var failedLoginScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await accountRepository.SaveChangesAsync(account, stoppingToken);
            failedLoginScope.Complete();
            return TypedResults.Unauthorized();
        }

        account.Login();

        var (_, accessToken, expiresOn) = tokenService.GenerateAccessToken(account);
        var (refreshTokenId, refreshToken, refreshExpiresOn) = tokenService.GenerateRefreshToken(account);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await tokenRepository.SaveAsync(TokenLog.Create(refreshTokenId, refreshExpiresOn, account), stoppingToken);
        var saveResult = await accountRepository.SaveChangesAsync(account, stoppingToken);
        scope.Complete();

        cookieService.Set(CookieKeys.RefreshToken, refreshToken, refreshExpiresOn);
        var response = new AccessTokenResponse(accessToken, expiresOn);

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.Ok(response),
            onFailure: error => TypedResults.Problem(error.Message));
    }
}
