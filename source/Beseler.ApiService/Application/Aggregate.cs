namespace Beseler.ApiService.Application;

public abstract class Aggregate
{
    private List<Event>? _domainEvents;

    public IReadOnlyCollection<Event> DomainEvents => _domainEvents ??= [];
    public bool HasUnsavedChanges { get; private set; }

    public void SavedChanges()
    {
        _domainEvents?.Clear();
        HasUnsavedChanges = false;
    }

    protected void HasUnsavedChange() => HasUnsavedChanges = true;
    protected void AddDomainEvent(Event domainEvent)
    {
        _domainEvents ??= [];
        _domainEvents.Add(domainEvent);
    }
}
