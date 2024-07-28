namespace Beseler.ApiService.Queues;

public sealed class MessageQueue
{
    public MessageQueue(string queueName, int messageRetentionDays, int messageMaxReceives, int? deadLetterQueueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(messageRetentionDays, nameof(messageRetentionDays));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(messageRetentionDays, 14, nameof(messageRetentionDays));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(messageMaxReceives, nameof(messageMaxReceives));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(messageMaxReceives, 256, nameof(messageMaxReceives));

        QueueId = Guid.NewGuid();
        QueueName = queueName;
        MessageRetentionDays = messageRetentionDays;
        MessageMaxReceives = messageMaxReceives;
        CreatedOn = DateTime.UtcNow;
        DeadLetterQueueId = deadLetterQueueId;
    }
    public Guid QueueId { get; init; }
    public required string QueueName { get; init; }
    public int MessageRetentionDays { get; private set; }
    public int MessageMaxReceives { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public int? DeadLetterQueueId { get; private set; }
}
