using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.Application.Payments.CreatePayment;

public interface ICreatePaymentUseCase
{
    Task<PaymentResponse> ExecuteAsync(CreatePaymentRequest request);
}
