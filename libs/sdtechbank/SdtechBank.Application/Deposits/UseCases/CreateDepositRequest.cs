namespace SdtechBank.Application.Deposits.UseCases;

public record CreateDepositRequest(string CreditAccountId, decimal Amount, string SourceCode);
