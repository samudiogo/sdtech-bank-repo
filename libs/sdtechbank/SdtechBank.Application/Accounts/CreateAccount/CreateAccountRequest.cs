namespace SdtechBank.Application.Accounts.CreateAccount;

public record CreateAccountRequest(string FullName, string Cpf, string BankCode, string Branch, string AccountCode, string AccountType );
