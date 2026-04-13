namespace SdtechBank.Shared.DTOs.Payments.Requests;

public class CreatePaymentRequest
{
    public string? IdempotencyKey { get; set; }
    public decimal? Amount { get; set; }
    public string? PayerId { get; set; }    
    public PaymentReceiverRequest? Receiver { get; set; }
}
