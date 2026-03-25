namespace SdtechBank.Shared.DTOs.Payments.Requests;

public class CreatePaymentRequest
{
    public required decimal Amount { get; set; }
    public required string PayerId { get; set; }
    public PaymentReceiverRequest Receiver { get; set; } = default!;
}
