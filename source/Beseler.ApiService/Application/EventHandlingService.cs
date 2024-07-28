using Beseler.ApiService.Accounts;
using Beseler.ApiService.Accounts.EventHandlers;

namespace Beseler.ApiService.Application;

internal sealed class EventHandlingService(IServiceProvider provider)
{
    public Task HandleAsync(Event @event, CancellationToken stoppingToken)
    {
        using var scope = provider.CreateScope();
        return @event switch
        {
            AccountCreatedDomainEvent accountCreatedDomainEvent => scope.ServiceProvider
                .GetRequiredService<SendVerificationEmailWhenAccountCreatedHandler>()
                .Handle(accountCreatedDomainEvent, stoppingToken),

            AccountLockedDomainEvent accountLockedDomainEvent => scope.ServiceProvider
                .GetRequiredService<SendAccountLockedEmailWhenAccountLockedHandler>()
                .Handle(accountLockedDomainEvent, stoppingToken),

            _ => throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}")
        };
    }
}
