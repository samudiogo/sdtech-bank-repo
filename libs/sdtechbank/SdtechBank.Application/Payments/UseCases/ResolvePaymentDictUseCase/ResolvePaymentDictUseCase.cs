using Microsoft.Extensions.Logging;
using SdtechBank.Application.DictServices;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Abstractions;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Application.Payments.UseCases.ResolvePaymentDictUseCase;

public interface IResolvePaymentDictUseCase
{
    Task ExecuteAsync(Guid paymentId, CancellationToken cancellation);
}
public class ResolvePaymentDictUseCase(IPaymentOrderRepository repository,
                                       IOutboxService outboxService,
                                       IDictClient dictClient,
                                       IReceiverResolver receiverResolver,
                                       ILogger<ResolvePaymentDictUseCase> logger) : IResolvePaymentDictUseCase
{
    public async Task ExecuteAsync(Guid paymentId, CancellationToken cancellation)
    {
        logger.LogInformation("iniciando o processamento para a ordem de pagamento {PaymentId}", paymentId);

        var paymmentOrder = await repository.GetByIdAsync(paymentId);
        if (paymmentOrder is null)
            return;

        var pixKey = paymmentOrder.Destination.PixKey!;
        logger.LogInformation("chave pix: {PixKey}", pixKey);

        var dict = await dictClient.GetKeyAsync(pixKey, cancellation);

        if (dict is null)
        {
            paymmentOrder.MarkAsFailed("Chave pix não encontrada");
            await repository.SaveAsync(paymmentOrder, cancellation);
            return;
        }

        paymmentOrder.DefineDestinationBankAccount(new BankAccount
        {
            FullName = dict.Owner.Name,
            Document = dict.Owner.Document,
            BankCode = dict.Account.BankCode,
            Branch = dict.Account.Branch,
            Account = $"{dict.Account.Number}-{dict.Account.Digit}"
        });

        await repository.SaveAsync(paymmentOrder, cancellation);

        var receiverId = await receiverResolver.ResolveAsync(paymmentOrder, cancellation);

        if (receiverId is null)
        {
            paymmentOrder.MarkAsFailed("Receiver not found");
            await repository.SaveAsync(paymmentOrder, cancellation);
            return;
        }

        paymmentOrder.MarkAsWaitingConfirmation();
        paymmentOrder.MarkAsReadyToTransfer();

        await repository.SaveAsync(paymmentOrder, cancellation);

        await outboxService.AddEventAsync(paymmentOrder.ToPaymentValidatedIntegrationEvent(receiverId: receiverId!.Value, correlationId: paymmentOrder.IdempotencyKey.ToString()), cancellation);

    }
}