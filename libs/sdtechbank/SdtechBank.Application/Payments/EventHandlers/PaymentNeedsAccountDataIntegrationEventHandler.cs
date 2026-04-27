using SdtechBank.Application.IntegrationEvents;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.UseCases.ResolvePaymentDictUseCase;

namespace SdtechBank.Application.Payments.EventHandlers;

public sealed class PaymentNeedsAccountDataIntegrationEventHandler(IResolvePaymentDictUseCase useCase) : IIntegrationEventHandler<PaymentNeedsAccountDataIntegrationEvent>
{
    public async Task HandleAsync(PaymentNeedsAccountDataIntegrationEvent @event, CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(@event.PaymentId, cancellationToken);

        await Task.CompletedTask;
    }
}