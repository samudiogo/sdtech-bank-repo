using SdtechBank.Application.Common.Contracts;

namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentCreatedEvent : IDomainIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTime.UtcNow;
    public string RoutingKey => "payment.created";
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public required Guid PaymentId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required Guid PayerId { get; init; }
    public required PaymentDestinationEvent Destination { get; init; }
}

