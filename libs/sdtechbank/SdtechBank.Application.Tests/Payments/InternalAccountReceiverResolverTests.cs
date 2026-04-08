using FluentAssertions;
using Moq;
using SdtechBank.Application.Payments.Resolvers;
using SdtechBank.Domain.Accounts;
using SdtechBank.Domain.Accounts.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Enums;
using SdtechBank.Domain.Shared.ValueObjects;

namespace SdtechBank.Application.Tests.Payments;

public class InternalAccountReceiverResolverTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly InternalAccountReceiverResolver _resolver;

    public InternalAccountReceiverResolverTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _resolver = new InternalAccountReceiverResolver(_accountRepositoryMock.Object);
    }
    [Fact]
    public async Task Should_Resolve_Internal_Account_By_BankAccount()
    {
        //arrange
        var bankAccount = new BankAccount
        {
            FullName = "Samuel",
            Cpf = "00012345680",
            BankCode = "001",
            Branch = "1234",
            Account = "123456"
        };

        var payment = PaymentOrder.Create(
           Guid.NewGuid(),
           PaymentDestination.FromBankAccount(bankAccount),
           new Money(100, CurrencyType.BRL)
       );

        var internalAccount = Account.Create(
                                            fullName: "Samuel",
                                            cpf: "00012345680",
                                            bankCode:"001",
                                            branch: "1234", 
                                            accountCode: "12345-6", 
                                            type: AccountType.PERSONAL);


        _accountRepositoryMock
           .Setup(r => r.GetByBankAccountAsync(bankAccount, CancellationToken.None))
           .ReturnsAsync(internalAccount);


        //act:
        var result = await _resolver.ResolveAsync(payment, CancellationToken.None);

        //assert:
        result.Should().Be(internalAccount.Id);
        _accountRepositoryMock.Verify(a => a.GetByBankAccountAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Null_When_Account_Not_Found()
    {
        //arrange
        var bankAccount = new BankAccount
        {
            FullName = "Samuel",
            Cpf = "00012345680",
            BankCode = "001",
            Branch = "1234",
            Account = "123456"
        };

        var payment = PaymentOrder.Create(
           Guid.NewGuid(),
           PaymentDestination.FromBankAccount(bankAccount),
           new Money(100, CurrencyType.BRL)
       );

        Account? internalAccount = null;

        _accountRepositoryMock
           .Setup(r => r.GetByBankAccountAsync(bankAccount, CancellationToken.None))
           .ReturnsAsync(internalAccount);


        //act:
        var result = await _resolver.ResolveAsync(payment, CancellationToken.None);

        //assert:
        result.Should().BeNull();
        _accountRepositoryMock.Verify(a => a.GetByBankAccountAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Null_When_Destination_Is_Pix()
    {
        //arrange
        var pixKey = "pix@key.com";

        var payment = PaymentOrder.Create(
           Guid.NewGuid(),
           PaymentDestination.FromPixKey(pixKey),
           new Money(100, CurrencyType.BRL)
       );

        //act:
        var result = await _resolver.ResolveAsync(payment, CancellationToken.None);

        //assert:
        result.Should().BeNull();
        _accountRepositoryMock.Verify(a => a.GetByBankAccountAsync(It.IsAny<BankAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
