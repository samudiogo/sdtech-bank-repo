using SdtechBank.Domain.Deposits;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Deposits.UseCases;

public static class CreateDepositDtoMappingExtensions
{
    public static CreateDepositResponse ToResponse(this Deposit entity)
    {
        return new CreateDepositResponse(entity.CreditAccountId, entity.Amount.ToString(), entity.OccurredAt);
    }
    public static Deposit ToEntity(this CreateDepositRequest request)
    {
        var creditAccountId = Guid.Parse(request.CreditAccountId);
        var amount = new Money(request.Amount, CurrencyType.BRL);
        var source = Enum.Parse<DepositSource>(request.SourceCode);
        return Deposit.Create(creditAccountId, amount, source);
    }
}