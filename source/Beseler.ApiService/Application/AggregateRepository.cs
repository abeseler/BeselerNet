using Beseler.ApiService.Application.Outbox;
using System.Text.Json;

namespace Beseler.ApiService.Application;

public abstract class AggregateRepository(OutboxRepository repository)
{
    protected virtual async Task SaveChangesAsync(Aggregate model, CancellationToken stoppingToken = default)
    {
        if (model.DomainEvents.Count == 0) { return; }

        var messages = model.DomainEvents.Select(x => OutboxMessage.Create(
            OutboxRepository.ServiceId, JsonSerializer.Serialize(x, JsonSerializerOptionsExt.Web), 10));
        await repository.Enqueue(messages, stoppingToken);
        model.SavedChanges();
    }
}
