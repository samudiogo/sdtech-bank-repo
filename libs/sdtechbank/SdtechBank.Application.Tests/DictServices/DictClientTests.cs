using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using Moq.Protected;
using SdtechBank.Application.DictServices;
using SdtechBank.Domain.PaymentOrders.Enums;
using SdtechBank.Domain.PaymentOrders.ValueObjects;


namespace SdtechBank.Application.Tests.DictServices;

public class DictClientTests
{
    [Fact]
    public async Task GetKeyAsync_WhenResponseIs200_ShouldReturnDeserializedObject()
    {
        // Arrange
        var json = """
        {
          "key": "samuel@email.com",
          "key_normalized": "samuel@email.com",
          "key_type": "Email",
          "status": "ACTIVE",
          "owner": {
            "name": "Samuel",
            "document": "12345678901",
            "document_type": "CPF"
          },
          "account": {
            "ispb": "12345678",
            "bank_code": "001",
            "bank_name": "Banco Teste",
            "branch": "0001",
            "number": "12345",
            "digit": "9",
            "account_type": "Checking"
          },
          "metadata": {
            "version": 1
          }
        }
        """;

        var handler = new Mock<HttpMessageHandler>();

        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://fake-api")
        };

        var sut = new DictClient(httpClient);

        var key = new PixKey("samuel@email.com", PixKeyType.EMAIL);

        // Act
        var result = await sut.GetKeyAsync(key, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Key.Should().Be("samuel@email.com");
        result.KeyNormalized.Should().Be("samuel@email.com");
        result.KeyType.Should().Be("Email");
        result.Status.Should().Be("ACTIVE");

        result.Owner.Name.Should().Be("Samuel");
        result.Owner.Document.Should().Be("12345678901");

        result.Account.BankCode.Should().Be("001");
        result.Account.BankName.Should().Be("Banco Teste");

        result.Metadata.Version.Should().Be(1);
    }

    [Fact]
    public async Task GetKeyAsync_WhenResponseIs404_ShouldReturnNull()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();

        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://fake-api")
        };

        var sut = new DictClient(httpClient);

        var key = new PixKey("naoexiste@email.com", PixKeyType.EMAIL);

        // Act
        var result = await sut.GetKeyAsync(key, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetKeyAsync_WhenResponseIs500_ShouldThrowHttpRequestException()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();

        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://fake-api")
        };

        var sut = new DictClient(httpClient);

        var key = new PixKey("erro@email.com", PixKeyType.EMAIL);

        // Act
        var act = () => sut.GetKeyAsync(key, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetKeyAsync_ShouldSendEscapedUrl()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        var handler = new Mock<HttpMessageHandler>();

        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://fake-api")
        };

        var sut = new DictClient(httpClient);

        var key = new PixKey("samuel+teste@email.com", PixKeyType.EMAIL);

        // Act
        await sut.GetKeyAsync(key, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.PathAndQuery.Should().Be("/keys/samuel%2Bteste%40email.com?keyType=EMAIL");
    }
}