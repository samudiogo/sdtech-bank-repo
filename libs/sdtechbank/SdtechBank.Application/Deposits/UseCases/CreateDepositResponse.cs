namespace SdtechBank.Application.Deposits.UseCases;

public record CreateDepositResponse(Guid CreditAccountId, string Amount, DateTime OccurredAt);
