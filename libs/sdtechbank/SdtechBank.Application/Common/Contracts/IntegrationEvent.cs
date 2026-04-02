namespace SdtechBank.Application.Common.Contracts;

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTime.UtcNow;
    public abstract string RoutingKey { get; }
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
