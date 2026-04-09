
using FluentAssertions;
using Moq;
using SdtechBank.Application.Accounts.CreateAccount;
using SdtechBank.Domain.Accounts;
using SdtechBank.Domain.Accounts.Contracts;

namespace SdtechBank.Application.Tests.Accounts;

public class CreateAccountUseCaseTests
{
    private readonly Mock<IAccountRepository> _repository;
    private readonly CreateAccountValidator _validator;
    private CreateAccountUseCase _sut;

    public CreateAccountUseCaseTests()
    {
        _repository = new Mock<IAccountRepository>();
        _validator = new CreateAccountValidator();
        _sut = new CreateAccountUseCase(_repository.Object, _validator);
    }
      

    [Theory]
    [InlineData("PERSONAL")]
    [InlineData("INTERNAL")]
    [InlineData("ENTERPRISE")]
    public async Task RegisterAccountAsync_Should_Create_Account_Return_ResultSuccessAsync(string accountType)
    {
        //arrange:
        var request = new CreateAccountRequest("Samuel", "00000000000", "001", "0001", "00001-1", accountType);

        //act:
        var result = await _sut.RegisterAccountAsync(request, CancellationToken.None);

        //assert:

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FullName.Should().BeEquivalentTo(request.FullName);
        result.Value!.Cpf.Should().BeEquivalentTo(request.Cpf);
        result.Value!.BankCode.Should().BeEquivalentTo(request.BankCode);
        result.Value!.Branch.Should().BeEquivalentTo(request.Branch);
        result.Value!.AccountCode.Should().BeEquivalentTo(request.AccountCode);
        result.Value!.Type.ToString().Should().BeEquivalentTo(accountType);


    }

    [Fact]
    public async Task RegisterAccountAsync_WhenValid_Should_PassCancellationTokenToRepositoryAsync()
    {
        //arrange:
        var request = new CreateAccountRequest("Samuel", "00000000000", "001", "0001", "00001-1", "PERSONAL");
        var cts = new CancellationTokenSource();

        //act:
        var result = await _sut.RegisterAccountAsync(request, cts.Token);

        //assert:
        _repository.Verify(r => r.SaveAsync(It.Is<Account>(a => a.FullName.Contains(request.FullName)), cts.Token), Times.Once);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RegisterAccountAsync_Should_Fail_WhenCpf_IsInvalid(string cpf)
    {
        //arrange:
        var request = new CreateAccountRequest("Samuel", cpf, "001", "0001", "00001-1", "PERSONAL");

        //act:
        var result = await _sut.RegisterAccountAsync(request, CancellationToken.None);

        //assert:
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("cpf", StringComparison.InvariantCultureIgnoreCase));
    }

}
