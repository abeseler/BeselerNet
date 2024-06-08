using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Beseler.ApiService.Accounts.EndpointHandlers;

internal static class RegisterAccountHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
        RegisterAccountRequest request,
        IValidator<RegisterAccountRequest> validator,
        IPasswordHasher<Account> passwordHasher,
        AccountRepository repository,
        CancellationToken stoppingToken)
    {
        if (await validator.ValidateAsync(request, stoppingToken) is { IsValid: false } validationResult)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var passwordHash = passwordHasher.HashPassword(default!, request.Password);
        var account = Account.Create(request.Email, request.GivenName, request.FamilyName, passwordHash);

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var saveResult = await repository.SaveChangesAsync(account, stoppingToken);
        transactionScope.Complete();

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.NoContent(),
            onFailure: error => TypedResults.Problem(error.Message));

    }
}
