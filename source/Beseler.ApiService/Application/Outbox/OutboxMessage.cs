namespace Beseler.ApiService.Application.Outbox;

public sealed record OutboxMessage
{
    private OutboxMessage() { }
    public int Id { get; init; }
    public required string ServiceId { get; init; }
    public required string Payload { get; init; }
    public DateTime CreatedOn { get; init; }
    public DateTime InvisibleUntil { get; init; }
    public int ReceivesRemaining { get; init; }
    public static OutboxMessage Create(string serviceId, string payload, int receiveMaxCount) =>
        new()
        {
            Id = 0,
            ServiceId = serviceId,
            Payload = payload,
            CreatedOn = DateTime.UtcNow,
            InvisibleUntil = DateTime.UtcNow,
            ReceivesRemaining = receiveMaxCount
        };
}
