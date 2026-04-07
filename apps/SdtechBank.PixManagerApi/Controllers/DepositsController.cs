using Microsoft.AspNetCore.Mvc;
using SdtechBank.Application.Deposits.UseCases;
using SdtechBank.PixManagerApi.Extensions;

namespace SdtechBank.PixManagerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepositsController (ICreateDepositUseCase useCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterDepositAsync(CreateDepositRequest request, CancellationToken cancellationToken)
    {
        var result = await useCase.RegisterDepositAsync(request, cancellationToken);

        return result.ToActionResult();
    }
}
