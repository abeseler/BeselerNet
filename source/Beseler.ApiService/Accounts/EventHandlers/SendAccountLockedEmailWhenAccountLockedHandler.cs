using Beseler.ApiService.Application;
using Beseler.ApiService.Application.SendGrid;
using System.Diagnostics;

namespace Beseler.ApiService.Accounts.EventHandlers;

internal sealed class SendAccountLockedEmailWhenAccountLockedHandler(EmailService emailService)
{
    public async Task HandleAsync(AccountLockedDomainEvent @event, CancellationToken stoppingToken = default)
    {
        using var activity = Telemetry.StartActivity("SendAccountLockedEmailWhenAccountLockedHandler.HandleAsync", Activity.Current?.Id ?? @event.TraceId);
        activity?.SetTag("event.id", @event.EventId);
        activity?.SetTag_AccountId(@event.AccountId);

        var emailMessage = new EmailMessage
        {
            ToEmail = @event.Email,
            ToName = @event.Email,
            Subject = "Your Account Has Been Locked",
            Body = $"Your account has been locked. Reason: {@event.Reason} Please contact support for assistance.",
            BodyHtml = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Your Account Has Been Locked</title>
                </head>
                <body style="font-family: Arial, sans-serif; background-color: #f5f5f5; color: #333; margin: 0; padding: 0;">
                <div style="max-width: 600px; margin: 20px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);">
                  <h2>Your Account Has Been Locked</h2>
                  <p>Your account has been locked for the following reason:</p>
                  <p>{@event.Reason}</p>
                  <p>Best regards,<br>The BSLR Team</p>
                </div>
                </body>
                </html>
                """
        };

        await emailService.SendAsync(emailMessage, stoppingToken);
    }
}
