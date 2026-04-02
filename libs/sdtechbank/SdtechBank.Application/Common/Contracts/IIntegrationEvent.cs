namespace SdtechBank.Application.Common.Contracts;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
    string CorrelationId { get; }
    string RoutingKey { get; }
}
