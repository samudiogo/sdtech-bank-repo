using SdtechBank.Application.Common;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Common.Errors;
using SdtechBank.Application.Payments.Contracts.Events;
using SdtechBank.Application.Payments.Extensions;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.UseCases.CreatePayment;

public class CreatePaymentUseCase(IPaymentOrderRepository repository, IEventBus eventBus, CreatePaymentValidator validator) : ICreatePaymentUseCase
{
    public async Task<Result<PaymentResponse>> ExecuteAsync(CreatePaymentRequest request)
    {

        var validation = ValidateRequest(request);

        if (validation.IsSuccess is false)
            return Result<PaymentResponse>.Failure(validation.Errors);

        var payment = request.ToEntity();

        await repository.SaveAsync(payment);

        await eventBus.PublishAsync(payment.ToPaymentCreatedEvent());
                
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