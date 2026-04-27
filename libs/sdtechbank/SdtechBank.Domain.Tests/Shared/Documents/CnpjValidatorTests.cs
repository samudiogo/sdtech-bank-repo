using FluentAssertions;
using SdtechBank.Domain.Shared.Documents;

namespace SdtechBank.Domain.Tests.Shared.Documents;

public class CnpjValidatorTests
{
    [Theory]
    [InlineData("11444777000161")]
    [InlineData("11.444.777/0001-61")]
    [InlineData("27865757000102")]
    [InlineData("27.865.757/0001-02")]
    public void IsCnpjValid_WhenCnpjIsValid_ShouldReturnTrue(string cnpj)
    {
        // Act
        var result = cnpj.IsCnpjValid();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void IsCnpjValid_WhenValueIsNullOrWhiteSpace_ShouldReturnFalse(string? cnpj)
    {
        // Act
        var result = cnpj.IsCnpjValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890123")]
    [InlineData("123456789012345")]
    [InlineData("123")]
    [InlineData("11.444.777/0001-6")]
    public void IsCnpjValid_WhenLengthIsDifferentFrom14_ShouldReturnFalse(string cnpj)
    {
        // Act
        var result = cnpj.IsCnpjValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("00000000000000")]
    [InlineData("11111111111111")]
    [InlineData("22222222222222")]
    [InlineData("33333333333333")]
    [InlineData("99.999.999/9999-99")]
    public void IsCnpjValid_WhenAllDigitsAreEqual_ShouldReturnFalse(string cnpj)
    {
        // Act
        var result = cnpj.IsCnpjValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("11444777000160")]
    [InlineData("11444777000162")]
    [InlineData("27865757000101")]
    [InlineData("27865757000103")]
    public void IsCnpjValid_WhenCheckDigitsAreInvalid_ShouldReturnFalse(string cnpj)
    {
        // Act
        var result = cnpj.IsCnpjValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("11.444abc777/0001-61")]
    [InlineData("11 444 777 0001 61")]
    [InlineData("11-444-777/0001.61")]
    public void IsCnpjValid_WhenCnpjContainsFormattingCharacters_ShouldStillValidate(string cnpj)
    {
        // Act
        var result = cnpj.IsCnpjValid();

        // Assert
        result.Should().BeTrue();
    }
}