using FluentAssertions;
using SdtechBank.Domain.Shared.Documents;

namespace SdtechBank.Domain.Tests.Shared.Documents;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData("11144477735")]
    [InlineData("935.411.347-80")]
    public void IsCpfValid_WhenCpfIsValid_ShouldReturnTrue(string cpf)
    {
        // Act
        var result = cpf.IsCpfValid();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void IsCpfValid_WhenValueIsNullOrWhiteSpace_ShouldReturnFalse(string? cpf)
    {
        // Act
        var result = cpf.IsCpfValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    [InlineData("123")]
    [InlineData("529.982.247-2")]
    public void IsCpfValid_WhenLengthIsDifferentFrom11_ShouldReturnFalse(string cpf)
    {
        // Act
        var result = cpf.IsCpfValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("333.333.333-33")]
    [InlineData("99999999999")]
    public void IsCpfValid_WhenAllDigitsAreEqual_ShouldReturnFalse(string cpf)
    {
        // Act
        var result = cpf.IsCpfValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("52998224724")]
    [InlineData("52998224726")]
    [InlineData("11144477734")]
    [InlineData("93541134781")]
    public void IsCpfValid_WhenCheckDigitsAreInvalid_ShouldReturnFalse(string cpf)
    {
        // Act
        var result = cpf.IsCpfValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("529.982abc247-25")]
    [InlineData("529 982 247 25")]
    [InlineData("529-982-247.25")]
    public void IsCpfValid_WhenCpfContainsFormattingCharacters_ShouldStillValidate(string cpf)
    {
        // Act
        var result = cpf.IsCpfValid();

        // Assert
        result.Should().BeTrue();
    }
}