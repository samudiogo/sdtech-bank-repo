namespace SdtechBank.Shared.DTOs.Payments.Requests;

public class BankAccountRequest
{
    public string? FullName { get; set; }
    public string? Cpf { get; set; }
    public string? BankCode { get; set; }
    public string? Branch { get; set; }
    public string? Account { get; set; }
}
