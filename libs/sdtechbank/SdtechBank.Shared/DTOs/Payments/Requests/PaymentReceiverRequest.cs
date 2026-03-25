namespace SdtechBank.Shared.DTOs.Payments.Requests;

public class PaymentReceiverRequest
{
    public string? PixKey { get; set; }
    public BankAccountRequest? BankAccount { get; set; }
}
