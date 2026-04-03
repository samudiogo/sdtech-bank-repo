using SdtechBank.Application.Common.Contracts;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Payments.Contracts.Events;

public sealed record PaymentValidatedIntegrationEvent : IntegrationEvent
{ 
    public required Guid PaymentId { get; init; }
    public required Guid PayerId { get; init; }
    public required Guid ReceiverId { get; init; }
    public required string IdempotencyKey { get; init; }

    public required Money Amount { get; init; }
    
    public required PaymentDestinationSnapshot Destination { get; init; }
    public override string RoutingKey => "payment.validated";
}
