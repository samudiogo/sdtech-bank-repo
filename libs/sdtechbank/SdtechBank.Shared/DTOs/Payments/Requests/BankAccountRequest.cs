namespace SdtechBank.Shared.DTOs.Payments.Requests;

public class BankAccountRequest
{
    public required string FullName { get; set; }
    public required string Cpf { get; set; }
    public required string BankCode { get; set; }
    public required string Branch { get; set; }
    public required string Account { get; set; }
}
