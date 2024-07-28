using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UUIDNext;

namespace Beseler.ApiService.Queues;

public sealed class QueuedMessage
{
    private QueuedMessage() { }
    public QueuedMessage(MessageQueue queue, string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload, nameof(payload));

        Id = Uuid.NewSequential();
        QueueId = queue.QueueId;
        Payload = payload;
        CreatedOn = DateTime.UtcNow;
        ExpiresOn = CreatedOn.AddDays(queue.MessageRetentionDays);
        InvisibleUntil = CreatedOn;
        ReceiveCount = 0;
    }

    private ReceiptHandle? _receiptHandle;
    public Guid Id { get; init; }
    public Guid QueueId { get; private set; }
    public required string Payload { get; init; }
    public DateTime CreatedOn { get; init; }
    public DateTime ExpiresOn { get; private set; }
    public DateTime? ReceivedOn { get; private set; }
    public DateTime InvisibleUntil { get; private set; }
    public int ReceiveCount { get; private set; }
    public ReceiptHandle ReceiptHandle => _receiptHandle ??= new ReceiptHandle(this);
}

public readonly struct ReceiptHandle : IEquatable<ReceiptHandle>
{
    public ReceiptHandle(QueuedMessage message)
    {
        QueueId = message.QueueId;
        MessageId = message.Id;
        ReceiveCount = message.ReceiveCount;
        ReceivedOn = message.ReceivedOn ?? DateTime.UtcNow;
    }

    public ReceiptHandle(string receiptHandle)
    {
        var parts = receiptHandle.Split('+');
        if (parts.Length != 3)
        {
            throw new ArgumentException("Invalid receipt handle", nameof(receiptHandle));
        }
        QueueId = new Guid(WebEncoders.Base64UrlDecode(parts[0]));
        MessageId = new Guid(WebEncoders.Base64UrlDecode(parts[1]));
        ReceiveCount = int.Parse(parts[2].AsSpan(0, 5), NumberStyles.HexNumber);
        ReceivedOn = new DateTime(long.Parse(parts[2].AsSpan(5), NumberStyles.HexNumber));
    }

    public Guid QueueId { get; }
    public Guid MessageId { get; }
    public int ReceiveCount { get; }
    public DateTime ReceivedOn { get; }

    public override bool Equals([AllowNull] object obj) => obj is ReceiptHandle handle && Equals(handle);
    public bool Equals(ReceiptHandle other) => QueueId == other.QueueId && MessageId.Equals(other.MessageId) && ReceiveCount == other.ReceiveCount;
    public override int GetHashCode() => HashCode.Combine(QueueId, MessageId, ReceiveCount);
    public static bool operator ==(ReceiptHandle left, ReceiptHandle right) => left.Equals(right);
    public static bool operator !=(ReceiptHandle left, ReceiptHandle right) => !(left == right);

    public override string ToString() => $"{WebEncoders.Base64UrlEncode(QueueId.ToByteArray())}+{WebEncoders.Base64UrlEncode(MessageId.ToByteArray())}+{ReceiveCount:X5}{ReceivedOn.Ticks:X16}";
}
