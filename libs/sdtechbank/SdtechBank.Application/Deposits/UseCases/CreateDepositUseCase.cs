using SdtechBank.Application.Common;
using SdtechBank.Application.Common.Errors;
using SdtechBank.Domain.Deposits;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;

namespace SdtechBank.Application.Deposits.UseCases;

public class CreateDepositUseCase(
    IDepositRepository depositRepository,
    ILedgerRepository ledgerRepository,
    CreateDepositRequestValidator validator) : ICreateDepositUseCase
{
    public async Task<Result<CreateDepositResponse>> RegisterDepositAsync(CreateDepositRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateRequest(request);

        if (!validation.IsSuccess)
            return Result<CreateDepositResponse>.Failure(validation.Errors);

        var entity = request.ToEntity();

        var ledgerCredit = LedgerEntry.CreateCredit(entity.Id, entity.CreditAccountId, entity.Amount);

        await depositRepository.SaveAsync(entity);

        await ledgerRepository.AddRangeAsync([ledgerCredit]);

        entity.MarkAsCompleted();
        await depositRepository.SaveAsync(entity);


        return Result<CreateDepositResponse>.Success(entity.ToResponse());
    }

    private Result ValidateRequest(CreateDepositRequest depositRequest)
    {
        var result = validator.Validate(depositRequest);

        if (!result.IsValid)
            return Result.Failure(result.Errors.FromValidation());

        return Result.Success();
    }
}
