using Beseler.ApiService.Accounts.EndpointHandlers;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Beseler.ApiService.Accounts;

internal static class AccountEndpoints
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .WithTags("Accounts");

        group.MapPost(Endpoints.Accounts.Register, RegisterAccountHandler.HandleAsync)
            .Produces(Status204NoContent)
            .Produces(Status400BadRequest);

        group.MapPost(Endpoints.Accounts.Login, LoginAccountHandler.HandleAsync)
            .Produces<AccessTokenResponse>(Status200OK)
            .Produces(Status400BadRequest);

        group.MapDelete(Endpoints.Accounts.Refresh, LogoutAccountHandler.HandleAsync)
            .Produces(Status204NoContent);

        group.MapPost(Endpoints.Accounts.Refresh, RefreshTokenHandler.HandleAsync)
            .Produces<AccessTokenResponse>(Status200OK);

        group.MapGet(Endpoints.Accounts.ConfirmEmail, ResendVerificationEmailHandler.HandleAsync)
            .Produces(Status204NoContent)
            .Produces(Status400BadRequest);

        group.MapPost(Endpoints.Accounts.ConfirmEmail, ConfirmEmailHandler.HandleAsync)
            .Produces(Status200OK)
            .Produces(Status400BadRequest);

        group.MapGet(Endpoints.Accounts.ResetPassword, ForgotPasswordHandler.HandleAsync)
            .Produces(Status204NoContent)
            .Produces(Status400BadRequest);

        group.MapPost(Endpoints.Accounts.ResetPassword, ResetPasswordHandler.HandleAsync)
            .Produces(Status204NoContent)
            .Produces(Status400BadRequest);

        return app;
    }
}
