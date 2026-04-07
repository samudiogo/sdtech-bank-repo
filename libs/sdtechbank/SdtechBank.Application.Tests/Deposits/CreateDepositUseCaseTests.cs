
using FluentAssertions;
using Moq;
using SdtechBank.Application.Deposits.UseCases;
using SdtechBank.Domain.Deposits;
using SdtechBank.Domain.Ledger.Contracts;
using SdtechBank.Domain.Ledger.Entities;
using SdtechBank.Domain.Ledger.Enums;


namespace SdtechBank.Application.Tests.Deposits;

public class CreateDepositUseCaseTests
{
    private readonly Mock<IDepositRepository> _depositRepositoryMock;
    private readonly Mock<ILedgerRepository> _ledgerRepositoryMock;
    private readonly CreateDepositRequestValidator _validator;

    private readonly CreateDepositUseCase _useCase;

    public CreateDepositUseCaseTests()
    {
        _depositRepositoryMock = new Mock<IDepositRepository>();
        _ledgerRepositoryMock = new Mock<ILedgerRepository>();
        _validator = new CreateDepositRequestValidator();

        _useCase = new CreateDepositUseCase(_depositRepositoryMock.Object, _ledgerRepositoryMock.Object, _validator);
    }

    private static CreateDepositRequest CreateValidRequest() => new(Guid.NewGuid().ToString(), 200, "1");


    [Fact]
    public async Task RegisterDeposit_Should_Return_Success_When_Request_Is_Valid()
    {

        var request = CreateValidRequest();

        // Act
        var result = await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }


    [Fact]
    public async Task RegisterDepositAsync_Should_Save_Deposit_Twice()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        _depositRepositoryMock.Verify(
            x => x.SaveAsync(It.IsAny<Deposit>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task RegisterDepositAsync_Should_Create_Ledger_Credit_Entry()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        _ledgerRepositoryMock.Verify(
            x => x.AddRangeAsync(
                It.Is<IEnumerable<LedgerEntry>>(entries =>
                    entries.Any(e => e.Type == LedgerEntryType.CREDIT)
                )
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterDepositAsync_Should_Mark_Deposit_As_Completed()
    {
        // Arrange
        var request = CreateValidRequest();

        Deposit? savedDeposit = null;

        _depositRepositoryMock
            .Setup(x => x.SaveAsync(It.IsAny<Deposit>()))
            .Callback<Deposit>(deposit => savedDeposit = deposit);

        // Act
        await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        savedDeposit.Should().NotBeNull();
        savedDeposit!.Status.Should().Be(DepositStatus.COMPLETED);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task RegisterDepositAsync_Should_Fail_When_Amount_Is_Invalid(decimal amount)
    {
        // Arrange
        var request = new CreateDepositRequest(
            CreditAccountId: Guid.NewGuid().ToString(),
            Amount: amount,
            SourceCode: "1"
        );

        // Act
        var result = await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Amount"));
    }

    [Fact]
    public async Task RegisterDepositAsync_Should_Fail_When_CreditAccountId_Is_Empty()
    {
        // Arrange
        var request = new CreateDepositRequest(
            CreditAccountId: string.Empty,
            Amount: 100,
            SourceCode: "1"
        );

        // Act
        var result = await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("CreditAccountId"));
    }

    [Fact]
    public async Task RegisterDepositAsync_Should_Fail_When_SourceCode_Is_Empty()
    {
        // Arrange
        var request = new CreateDepositRequest(
           CreditAccountId: Guid.NewGuid().ToString(),
           Amount: 100,
           SourceCode: string.Empty
       );

        // Act
        var result = await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("SourceCode"));
    }

    [Fact]
    public async Task RegisterDepositAsync_Should_Not_Call_Repositories_When_Request_Is_Invalid()
    {
        // Arrange
        var request = new CreateDepositRequest(
            CreditAccountId: string.Empty,
            Amount: -5,
            SourceCode: string.Empty
        );

        // Act
        await _useCase.RegisterDepositAsync(request, CancellationToken.None);

        // Assert
        _depositRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Deposit>()), Times.Never);
        _ledgerRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<LedgerEntry>>()), Times.Never);
    }
}


