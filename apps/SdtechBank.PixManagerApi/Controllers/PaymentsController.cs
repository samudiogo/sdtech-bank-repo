using Microsoft.AspNetCore.Mvc;
using SdtechBank.Application.Payments.CreatePayment;
using SdtechBank.Shared.DTOs.Payments.Requests;
using SdtechBank.Shared.DTOs.Payments.Responses;

namespace SdtechBank.PixManagerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController (ICreatePaymentUseCase createPaymentUseCase) : ControllerBase
{

    [HttpPost()]
    public async Task<ActionResult<PaymentResponse>> CreatePaymentOrder(CreatePaymentRequest request)
    {
        var result = await createPaymentUseCase.ExecuteAsync(request);

        return result;
    }
}
