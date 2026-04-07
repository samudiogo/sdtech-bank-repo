using SdtechBank.Application.Common;

namespace SdtechBank.Application.Deposits.UseCases;

public interface ICreateDepositUseCase
{
    Task<Result<CreateDepositResponse>> RegisterDepositAsync(CreateDepositRequest request, CancellationToken cancellationToken);
}
