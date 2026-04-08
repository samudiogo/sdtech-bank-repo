using FluentAssertions;
using FluentAssertions.Extensions;
using SdtechBank.Domain.Accounts;

namespace SdtechBank.Domain.Tests.Accounts;

public class AccountTests
{
    private static Account CreateValidAccount() => Account.Create(
                                            fullName: "Samuel",
                                            cpf: "00012345680",
                                            bankCode: "001",
                                            branch: "1234",
                                            accountCode: "12345-6",
                                            type: AccountType.PERSONAL);

    [Fact]
    public void Account_Create_Should_Status_Active()
    {
        //arrange
        var account = CreateValidAccount();

        //assert:
        account.Status.Should().Be(AccountStatus.ACTIVE);
        account.FullName.Should().Be("Samuel");
        account.Cpf.Should().Be("00012345680");
        account.BankCode.Should().Be("001");
        account.Branch.Should().Be("1234");
        account.AccountCode.Should().Be("12345-6");
        account.Type.Should().Be(AccountType.PERSONAL);
        account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Hours());
    }

    [Fact]
    public void Account_InactiveAccount_Should_Status_Inactive()
    {
        //arrange
        var account = CreateValidAccount();
        var reason = "reason";
        var now = DateTime.UtcNow;

        //act:
        account.InactiveAccount(reason);

        //assert:
        account.InactiveReason.Should().Be(reason);
        account.InactivatedAt.Should().BeCloseTo(now, 1.Hours());
        account.Status.Should().Be(AccountStatus.INACTIVE);
    }
}
