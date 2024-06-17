using Beseler.ApiService.Accounts;
using System.Text.Json.Serialization;

namespace Beseler.ApiService.Application;

[JsonPolymorphic]
[JsonDerivedType(typeof(AccountCreatedDomainEvent), nameof(AccountCreatedDomainEvent))]
[JsonDerivedType(typeof(AccountLockedDomainEvent), nameof(AccountLockedDomainEvent))]
public abstract record Event
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string? TraceId { get; init; } = Activity.Current?.Id ?? Activity.Current?.ParentId;
}
