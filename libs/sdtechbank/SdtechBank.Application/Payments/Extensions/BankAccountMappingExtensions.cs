using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.Application.Payments.Extensions;

public static class BankAccountMappingExtensions
{
    public static BankAccount ToEntity(this BankAccountRequest request)
    {
        return new BankAccount { FullName = request.FullName!, Document = request.Cpf!, BankCode = request.BankCode!, Account = request.Account!, Branch = request.Branch! };
    }
}
