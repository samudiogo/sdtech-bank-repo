using FluentAssertions;
using Moq;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.Application.Tests.Payments;

public class CreatePaymentUseCaseTests
{
    private readonly Mock<IPaymentOrderRepository> _repositoryMock;
    private readonly Mock<IOutboxService> _outboxServiceMock;
    private readonly CreatePaymentValidator _validator;
    private readonly CreatePaymentUseCase _useCase;

    public CreatePaymentUseCaseTests()
    {
        _repositoryMock = new Mock<IPaymentOrderRepository>();
        _outboxServiceMock = new Mock<IOutboxService>();
        _validator = new CreatePaymentValidator();

        _useCase = new CreatePaymentUseCase(_repositoryMock.Object, _outboxServiceMock.Object, _validator);
    }

    private static CreatePaymentRequest CreateValidRequest()
    {
        return new CreatePaymentRequest
        {
            Amount = 100,
            PayerId = Guid.NewGuid().ToString(),
            Receiver = new PaymentReceiverRequest
            {
                BankAccount = new BankAccountRequest
                {
                    FullName = "Samuel",
                    BankCode = "236",
                    Branch = "1234",
                    Account = "123456-7",
                    Cpf = "00012345680"
                }
            }
        };
    }

    [Fact]
    public async Task CreatePayment_Should_Return_Success_When_Request_IsValid()
    {
        var request = CreateValidRequest();

        //act:
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        //Asert:
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]

    public async Task CreatePayment_Should_Fail_When_Amount_Pix_Is_InvalidAsync(decimal amount)
    {
        var request = new CreatePaymentRequest
        {
            Amount = amount,
            PayerId = Guid.NewGuid().ToString(),
            Receiver = new PaymentReceiverRequest
            {
                PixKey = Guid.NewGuid().ToString()
            }
        };

        //act:
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        //Asert:
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Amount"));
    }

    [Fact]
    public async Task CreatePayment_Should_Fail_When_PayerId_Is_InvalidAsync()
    {
        var request = new CreatePaymentRequest
        {
            Amount = 100,
            PayerId = string.Empty,
            Receiver = new PaymentReceiverRequest
            {
                PixKey = Guid.NewGuid().ToString()
            }
        };

        //act:
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        //Asert:
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("PayerId"));
    }

    [Fact]
    public async Task CreatePayment_Should_Fail_When_Receiver_Is_InvalidAsync()
    {
        var request = new CreatePaymentRequest
        {
            Amount = 100,
            PayerId = Guid.NewGuid().ToString()
        };

        //act:
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        //Asert:
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Receiver"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456")]
    public async Task CreatePayment_Should_Fail_When_Receiver_Have_PixKey_And_Bank_AccountAsync(string cpf)
    {
        var request = new CreatePaymentRequest
        {
            Amount = 100,
            PayerId = Guid.NewGuid().ToString(),
            Receiver = new PaymentReceiverRequest
            {
                PixKey = null,
                BankAccount = new BankAccountRequest
                {
                    FullName = string.Empty,
                    BankCode = string.Empty,
                    Branch = string.Empty,
                    Account = string.Empty,
                    Cpf = cpf
                }
            }
        };
        string[] fieldsToBeValidated = ["FullName", "BankCode", "Branch", "Account", "Cpf"];

        //act:
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        //Asert:
        result.IsSuccess.Should().BeFalse();
        fieldsToBeValidated.Should().AllSatisfy(field => result.Errors.Should().Contain(e => e.Message.Contains(field, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task CreatePayment_Should_Fail_When_Receiver_Bank_Account_InvalidAsync()
    {
        var request = new CreatePaymentRequest
        {
            Amount = 100,
            PayerId = Guid.NewGuid().ToString(),
            Receiver = new PaymentReceiverRequest
            {
                PixKey = Guid.NewGuid().ToString(),
                BankAccount = new BankAccountRequest
                {
                    FullName = "Samuel",
                    BankCode = "236",
                    Branch = "1234",
                    Account = "123456-7",
                    Cpf = "00012345680"
                }
            }
        };

        //act:
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        //Asert:
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("PixKey") && e.Message.Contains("BankAccount"));
    }

}
