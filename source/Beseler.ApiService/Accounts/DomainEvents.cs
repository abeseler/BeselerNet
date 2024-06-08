using Beseler.ApiService.Application;

namespace Beseler.ApiService.Accounts;

internal sealed record AccountCreatedDomainEvent(string Email) : Event;
internal sealed record AccountLockedDomainEvent(string Email, string Reason) : Event;
