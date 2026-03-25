using FluentValidation;
using SdtechBank.Application.Payments.Extensions;
using SdtechBank.Application.Ports;
using SdtechBank.Domain.Contracts;
using SdtechBank.Domain.Entities;
using SdtechBank.Shared.DTOs.Payments.Extensions;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.Application.Payments.CreatePayment;

public class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private readonly IPaymentOrderRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly CreatePaymentValidator _validator;

    public CreatePaymentUseCase(IPaymentOrderRepository repository, IEventBus eventBus, CreatePaymentValidator validator)
    {
        _repository = repository;
        _eventBus = eventBus;
        _validator = validator;
    }

    public async Task<PaymentOrder> ExecuteAsync(CreatePaymentRequest request)
    {
       _validator.Validate(request, opt => opt.ThrowOnFailures());       

        var payment = request.ToEntity();

        await _repository.SaveAsync(payment);

        await _eventBus.PublishAsync(payment);

        return payment;
    }
}