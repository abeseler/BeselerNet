using Beseler.ApiService.Accounts.EndpointHandlers;

namespace Beseler.ApiService.Accounts;

internal static class AccountEndpoints
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("")
            .WithTags("Accounts");

        group.MapPost(Endpoints.Accounts.Register, RegisterAccountHandler.HandleAsync)
            .Produces(204)
            .Produces(400);

        group.MapPost(Endpoints.Accounts.Login, LoginAccountHandler.HandleAsync)
            .Produces<AccessTokenResponse>(200)
            .Produces(400);

        group.MapDelete(Endpoints.Accounts.Refresh, LogoutAccountHandler.HandleAsync)
            .Produces(204);

        group.MapPost(Endpoints.Accounts.Refresh, RefreshTokenHandler.HandleAsync)
            .Produces<AccessTokenResponse>(200);

        group.MapGet(Endpoints.Accounts.ConfirmEmail, ResendVerificationEmailHandler.HandleAsync)
            .Produces(204)
            .Produces(400);

        group.MapPost(Endpoints.Accounts.ConfirmEmail, ConfirmEmailHandler.HandleAsync)
            .Produces(200)
            .Produces(400);

        group.MapGet(Endpoints.Accounts.ResetPassword, ForgotPasswordHandler.HandleAsync)
            .Produces(204)
            .Produces(404);

        group.MapPost(Endpoints.Accounts.ResetPassword, ResetPasswordHandler.HandleAsync)
            .Produces(204)
            .Produces(400);

        return app;
    }
}
