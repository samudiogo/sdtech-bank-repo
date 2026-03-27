using FluentValidation;
using SdtechBank.Application.Common;
using SdtechBank.Application.Common.Errors;
using SdtechBank.Application.Contracts;
using SdtechBank.Application.Contracts.Events.Payments;
using SdtechBank.Application.Payments.Extensions;
using SdtechBank.Domain.Contracts;
using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.CreatePayment;

public class CreatePaymentUseCase(IPaymentOrderRepository repository, IEventBus eventBus, CreatePaymentValidator validator) : ICreatePaymentUseCase
{
    public async Task<Result<PaymentResponse>> ExecuteAsync(CreatePaymentRequest request)
    {

        var validation = ValidateRequest(request);

        if (validation.IsSuccess is false)
            return Result<PaymentResponse>.Failure(validation.Errors); //quebrou aqui

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