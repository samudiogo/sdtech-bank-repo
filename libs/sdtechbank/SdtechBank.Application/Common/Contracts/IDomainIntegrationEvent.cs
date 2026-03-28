namespace SdtechBank.Application.Common.Contracts;

public interface IDomainIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
    string CorrelationId { get; }
    string RoutingKey { get; }
}
