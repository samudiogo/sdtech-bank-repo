using Microsoft.AspNetCore.Mvc;
using SdtechBank.Application.Accounts.CreateAccount;
using SdtechBank.PixManagerApi.Extensions;


namespace SdtechBank.PixManagerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController(ICreateAccountUseCase useCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterDepositAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await useCase.RegisterAccountAsync(request, cancellationToken);

        return result.ToActionResult();
    }
}
