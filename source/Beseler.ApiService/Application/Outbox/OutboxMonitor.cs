namespace Beseler.ApiService.Application.Outbox;

internal sealed class OutboxMonitor(EventHandlingService factory, OutboxRepository repository, ILogger<OutboxMonitor> logger) : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                while (await repository.DequeueAsync(stoppingToken) is { } message)
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing {ServiceName}", nameof(OutboxMonitor));
            }
        }
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken stoppingToken)
    {
        try
        {
            var @event = JsonSerializer.Deserialize<Event>(message.Payload, JsonSerializerOptionsExt.Web)
                ?? throw new InvalidOperationException("Failed to deserialize event from message payload.");

            using var activity = Telemetry.Source.StartActivity("OutboxMonitor.ProcessMessage", ActivityKind.Internal, @event.TraceId);
            activity?.SetTag("message.id", message.Id);
            activity?.SetTag("event.id", @event.EventId);

            await factory.HandleAsync(@event, stoppingToken);
            await repository.DeleteAsync(message, stoppingToken);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred processing message: {MessageId}", message.Id);
        }
    }
}
