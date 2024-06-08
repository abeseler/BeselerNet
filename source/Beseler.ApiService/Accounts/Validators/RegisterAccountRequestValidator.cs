using FluentValidation;

namespace Beseler.ApiService.Accounts.Validators;

public sealed class RegisterAccountRequestValidator : AbstractValidator<RegisterAccountRequest>
{
    public RegisterAccountRequestValidator(AccountRepository repository)
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty().MinimumLength(7).MaximumLength(320);
        RuleFor(x => x.GivenName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FamilyName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

        RuleFor(x => x).CustomAsync(async (request, context, stoppingToken) =>
        {
            if (await repository.GetByEmailAsync(request.Email, stoppingToken) is not null)
                context.AddFailure(nameof(request.Email), $"Account already exists for '{request.Email}'.");
        });
    }
}
