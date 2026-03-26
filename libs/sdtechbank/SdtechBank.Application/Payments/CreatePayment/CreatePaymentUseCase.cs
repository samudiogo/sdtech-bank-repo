using FluentValidation;
using SdtechBank.Application.Contracts;
using SdtechBank.Application.Contracts.Events.Payments;
using SdtechBank.Application.Payments.Extensions;
using SdtechBank.Domain.Contracts;
using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.CreatePayment;

public class CreatePaymentUseCase(IPaymentOrderRepository repository, IEventBus eventBus, CreatePaymentValidator validator) : ICreatePaymentUseCase
{
    public async Task<PaymentResponse> ExecuteAsync(CreatePaymentRequest request)
    {
       validator.Validate(request, opt => opt.ThrowOnFailures());       

        var payment = request.ToEntity();

        await repository.SaveAsync(payment);

        await eventBus.PublishAsync(payment.ToPaymentCreatedEvent());

        return payment.ToResponse();
    }
}