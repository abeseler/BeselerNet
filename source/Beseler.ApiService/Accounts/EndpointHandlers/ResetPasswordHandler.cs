using Beseler.ApiService.Application.Jwt;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Beseler.ApiService.Accounts.EndpointHandlers;

internal static class ResetPasswordHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
        ResetPasswordRequest request,
        IValidator<ResetPasswordRequest> validator,
        TokenService tokenService,
        TokenRepository tokenRepository,
        AccountRepository accountRepository,
        IPasswordHasher<Account> passwordHasher,
        CancellationToken stoppingToken = default)
    {
        if (validator.Validate(request) is { IsValid: false } validationResult)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var principal = await tokenService.ValidateAsync(request.Token);
        if (principal?.Identity is not { IsAuthenticated: true, Name: not null } || int.TryParse(principal?.Identity?.Name, out var accountId) is false)
            return TypedResults.Unauthorized();

        if (principal.HasClaim(AppClaims.ResetPassword(tokenService.Audience)) is false)
            return TypedResults.Forbid();

        var account = await accountRepository.GetByIdAsync(accountId, stoppingToken);
        if (account is null)
            return TypedResults.NotFound();

        if (account.IsLocked)
            return TypedResults.Forbid();

        var hashedPassword = passwordHasher.HashPassword(account, request.Password);
        account.ResetPassword(hashedPassword);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await tokenRepository.RevokeTokensForAccount(accountId, stoppingToken);
        var saveResult = await accountRepository.SaveChangesAsync(account, stoppingToken);
        scope.Complete();

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.NoContent(),
            onFailure: error => TypedResults.Problem(error.Message));
    }
}
