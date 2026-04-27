
namespace SdtechBank.Domain.Accounts;

public class Account
{
    public Guid Id { get; set; }
    public string FullName { get; private set; } = default!;
    public string Document { get; private set; } = default!;
    public string BankCode { get; private set; } = default!;
    public string Branch { get; private set; } = default!;
    public string AccountCode { get; private set; } = default!;
    public AccountType Type { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? InactivatedAt { get; private set; }
    public DateTime? ActivatedAt { get; private set; }

    public string? InactiveReason { get; private set; }

    public static Account Create(string fullName, string document, string bankCode, string branch, string accountCode, AccountType type) => new()
    {
        FullName = fullName,
        Document = document,
        BankCode = bankCode,
        Branch = branch,
        AccountCode = accountCode,
        CreatedAt = DateTime.UtcNow,
        Type = type,
        Status = AccountStatus.ACTIVE,
        Id = Guid.NewGuid()
    };

    public void InactiveAccount(string reason)
    {
        if (Status == AccountStatus.INACTIVE) return;

        InactiveReason = reason;
        Status = AccountStatus.INACTIVE;
        InactivatedAt = DateTime.UtcNow;
    }

    public void ActiveAccount()
    {
        if (Status == AccountStatus.ACTIVE) return;

        ActivatedAt = DateTime.UtcNow;
    }
}
