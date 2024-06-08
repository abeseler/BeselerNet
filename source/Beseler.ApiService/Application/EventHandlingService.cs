using Beseler.ApiService.Accounts;
using Beseler.ApiService.Accounts.EventHandlers;

namespace Beseler.ApiService.Application;

internal sealed class EventHandlingService(IServiceProvider provider)
{
    public Task Handle(Event @event, CancellationToken stoppingToken)
    {
        using var scope = provider.CreateScope();
        return @event switch
        {
            AccountCreatedDomainEvent accountCreatedDomainEvent => scope.ServiceProvider
                .GetRequiredService<SendVerificationEmailWhenAccountCreatedHandler>()
                .HandleAsync(accountCreatedDomainEvent, stoppingToken),

            AccountLockedDomainEvent accountLockedDomainEvent => scope.ServiceProvider
                .GetRequiredService<SendAccountLockedEmailWhenAccountLockedHandler>()
                .HandleAsync(accountLockedDomainEvent, stoppingToken),

            _ => throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}")
        };
    }
}
