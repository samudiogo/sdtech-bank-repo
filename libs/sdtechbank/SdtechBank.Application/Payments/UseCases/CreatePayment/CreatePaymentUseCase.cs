using SdtechBank.Application.Common;
using SdtechBank.Application.Common.Errors;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.Extensions;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.UseCases.CreatePayment;

public class CreatePaymentUseCase(IPaymentOrderRepository repository,
                                  IOutboxService outboxService,
                                  CreatePaymentValidator validator) : ICreatePaymentUseCase
{

    public async Task<Result<PaymentResponse>> ExecuteAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {

        var validation = ValidateRequest(request);

        if (!validation.IsSuccess)
            return Result<PaymentResponse>.Failure(validation.Errors);

        var existing = await repository.GetByIdempotencyKeyAsync(new IdempotencyKey(request.IdempotencyKey!), cancellationToken);

        if (existing is not null)
            return Result<PaymentResponse>.Success(existing.ToResponse());

        var similarExists = await repository.ExistsRecentSimilarAsync(
                                                                    Guid.Parse(request.PayerId!), request.Receiver!.ToEntity(),
                                                                    new Money(request.Amount!.Value, CurrencyType.BRL),
                                                                    TimeSpan.FromMinutes(2), cancellationToken);

        if (similarExists)
        {
            // Evoluir no futuro Pendindo Confirmação
            return Result<PaymentResponse>.Failure([new Error("400", "Ordem de Pagamento já solicitado em menos de dois minutos", ErrorType.Business)]);
        }

        var payment = request.ToEntity();

        await repository.SaveAsync(payment);

        await outboxService.AddEventAsync(payment.ToPaymentCreatedIntegrationEvent(), cancellationToken);

        return Result<PaymentResponse>.Success(payment.ToResponse());
    }

    private Result ValidateRequest(CreatePaymentRequest request)
    {
        var result = validator.Validate(request);

        if (!result.IsValid)
            return Result.Failure(result.Errors.FromValidation());

        return Result.Success();
    }

}