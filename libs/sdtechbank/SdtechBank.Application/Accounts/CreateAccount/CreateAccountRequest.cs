using System;
using System.Collections.Generic;
using System.Text;

namespace SdtechBank.Application.Accounts.CreateAccount;

public record CreateAccountRequest(string FullName, string Cpf, string BankCode, string Branch, string AccountCode, string AccountType );
