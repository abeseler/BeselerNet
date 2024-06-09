using System.Text.Json;

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
                if (await repository.Dequeue(stoppingToken) is { } message)
                {
                    await ProcessMessage(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing {ServiceName}", nameof(OutboxMonitor));
            }
        }
    }

    public override Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {Service}: {ServiceId}", nameof(OutboxMonitor), OutboxRepository.ServiceId);
        return base.StartAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Stopping {Service}: {ServiceId}", nameof(OutboxMonitor), OutboxRepository.ServiceId);
        return base.StopAsync(stoppingToken);
    }

    private async Task ProcessMessage(OutboxMessage message, CancellationToken stoppingToken)
    {
        try
        {
            var @event = JsonSerializer.Deserialize<Event>(message.Payload, JsonSerializerOptionsExt.Web)
                ?? throw new InvalidOperationException("Failed to deserialize event from message payload.");

            using var activity = Telemetry.StartActivity("OutboxMonitor.ProcessMessage", @event.TraceId);
            activity?.SetTag("message.id", message.Id);
            activity?.SetTag("event.id", @event.EventId);

            try
            {
                await factory.Handle(@event, stoppingToken);
                await repository.DeleteAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred processing event: {EventId}", @event.EventId);
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred processing message: {MessageId}", message.Id);
        }
    }
}
