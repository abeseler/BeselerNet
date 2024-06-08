using Beseler.ApiService.Application.Jwt;
using Beseler.ApiService.Application.SendGrid;
using Microsoft.AspNetCore.Authorization;

namespace Beseler.ApiService.Accounts.EndpointHandlers;

internal static class ForgotPasswordHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(string email, AccountRepository repository, TokenService tokenService, EmailService emailService, CancellationToken stoppingToken)
    {
        var account = await repository.GetByEmailAsync(email, stoppingToken);
        if (account is null)
            return TypedResults.NotFound();

        if (account.IsLocked)
            return TypedResults.Forbid();

        var (_, token, _) = tokenService.GenerateToken(account, TimeSpan.FromMinutes(10), [AppClaims.ResetPasswordClaim(tokenService.Audience)]);

        await emailService.SendAsync(new()
        {
            ToEmail = account.Email,
            ToName = account.GivenName ?? account.Email,
            Subject = "Reset your password",
            Body = $"To reset your password, navigate to the following url in your browser: {tokenService.Audience}/account/reset-password?token={token}",
            BodyHtml = $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Password Reset Request</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;">
            <div style="max-width: 600px; margin: 20px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);">
              <h2>Password Reset Request</h2>
              <p>We received a request to reset your password. If you didn't make this request, you can safely ignore this email. Otherwise, please click the button below to reset your password:</p>
              <p style="text-align: center;"><a href="{tokenService.Audience}/account/reset-password?token={token}" style="display: inline-block; padding: 12px 24px; background-color: #007bff; color: #fff; text-decoration: none; border-radius: 4px;">Reset My Password</a></p>
              <p>If the button above doesn't work, you can also copy and paste the following link into your browser:</p>
              <p>{tokenService.Audience}/account/reset-password?token={token}</p>
              <p>This link will expire in 5 minutes. If you do not reset your password within this time, you may need to submit another request.</p>
              <p>If you did not request a password reset, please ensure your account is secure by changing your password and enabling additional security measures.</p>
              <p>Best regards,<br>The BSLR Team</p>
            </div>
            </body>
            </html>            
            """
        }, stoppingToken);

        return TypedResults.NoContent();
    }
}
