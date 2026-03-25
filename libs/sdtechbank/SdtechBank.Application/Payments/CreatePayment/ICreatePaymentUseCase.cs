using SdtechBank.Domain.Entities;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.Application.Payments.CreatePayment;

public interface ICreatePaymentUseCase
{
    Task<PaymentOrder> ExecuteAsync(CreatePaymentRequest request);
}
