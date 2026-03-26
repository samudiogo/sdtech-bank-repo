namespace SdtechBank.Shared.DTOs.Payments.Requests;

public class CreatePaymentRequest
{
    public decimal? Amount { get; set; }
    public string? PayerId { get; set; }
    public PaymentReceiverRequest? Receiver { get; set; }
}
