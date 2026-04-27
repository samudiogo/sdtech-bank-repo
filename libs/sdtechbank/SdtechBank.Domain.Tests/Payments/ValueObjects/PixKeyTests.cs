using FluentAssertions;
using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.ValueObjects;

namespace SdtechBank.Domain.Tests.Payments.ValueObjects;

public class PixKeyTests
{
    [Fact]
    public void Constructor_WhenKeyIsNull_ShouldThrowException()
    {
        // Act
        var act = () => new PixKey(null!);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Chave pix não pode ser vazia ou nula");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WhenKeyIsEmpty_ShouldThrowException(string key)
    {
        // Act
        var act = () => new PixKey(key);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Chave pix não pode ser vazia ou nula");
    }

    [Fact]
    public void Constructor_WhenEmail_ShouldNormalizeAndDetectType()
    {
        // Act
        var result = new PixKey(" Samuel@SDTECH.com ");

        // Assert
        result.Value.Should().Be("samuel@sdtech.com");
        result.Type.Should().Be(PixKeyType.EMAIL);
    }

    [Fact]
    public void Constructor_WhenCpf_ShouldNormalizeAndDetectType()
    {
        // Act
        var result = new PixKey("529.982.247-25");

        // Assert
        result.Value.Should().Be("52998224725");
        result.Type.Should().Be(PixKeyType.CPF);
    }

    [Fact]
    public void Constructor_WhenCnpj_ShouldNormalizeAndDetectType()
    {
        // Act
        var result = new PixKey("11.444.777/0001-61");

        // Assert
        result.Value.Should().Be("11444777000161");
        result.Type.Should().Be(PixKeyType.CNPJ);
    }

    [Fact]
    public void Constructor_WhenPhone_ShouldNormalizeAndDetectType()
    {
        // Act
        var result = new PixKey("(21) 98070-1947");

        // Assert
        result.Value.Should().Be("+5521980701947");
        result.Type.Should().Be(PixKeyType.PHONE);
    }

    [Fact]
    public void Constructor_WhenRandomKey_ShouldNormalizeAndDetectType()
    {
        // Act
        var result = new PixKey("550E8400-E29B-41D4-A716-446655440000");

        // Assert
        result.Value.Should().Be("550e8400-e29b-41d4-a716-446655440000");
        result.Type.Should().Be(PixKeyType.RANDOM);
    }

    [Fact]
    public void Constructor_WhenInvalidKey_ShouldThrowException()
    {
        // Act
        var act = () => new PixKey("chave-invalida");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Tipo de chave Pix inválido.");
    }

    [Fact]
    public void ConstructorWithExplicitType_ShouldKeepProvidedType()
    {
        // Act
        var result = new PixKey("529.982.247-25", PixKeyType.EMAIL);

        // Assert
        result.Value.Should().Be("52998224725");
        result.Type.Should().Be(PixKeyType.EMAIL);
    }

    [Fact]
    public void ConstructorWithExplicitType_ShouldStillValidateAndNormalizeKey()
    {
        // Act
        var result = new PixKey(" SamuEL@SDTECH.com ", PixKeyType.CPF);

        // Assert
        result.Value.Should().Be("samuel@sdtech.com");
        result.Type.Should().Be(PixKeyType.CPF);
    }

    [Fact]
    public void ConstructorWithExplicitType_WhenInvalidKey_ShouldThrowException()
    {
        // Act
        var act = () => new PixKey("abc", PixKeyType.EMAIL);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Tipo de chave Pix inválido.");
    }
}