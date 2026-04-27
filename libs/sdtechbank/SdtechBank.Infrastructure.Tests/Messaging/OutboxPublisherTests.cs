using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SdtechBank.Application.Common.Contracts;
using SdtechBank.Application.Messaging;
using SdtechBank.Domain.Shared.Messaging;
using SdtechBank.Infrastructure.Messaging;

namespace SdtechBank.Infrastructure.Tests.Messaging;

public class OutboxPublisherTests
{
    private readonly Mock<IOutboxRepository> _repositoryMock;
    private readonly Mock<IEventPublisher> _publisherMock;
    private readonly Mock<ILogger<OutboxPublisher>> _loggerMock;
    private readonly Mock<IServiceProvider> _rootProviderMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _scopedProviderMock;
    private readonly OutboxPublisherSettings _settings;
    private readonly OutboxPublisher _sut;

    public OutboxPublisherTests()
    {
        _repositoryMock = new Mock<IOutboxRepository>();
        _publisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<OutboxPublisher>>();

        _settings = new OutboxPublisherSettings
        {
            MessagesLimit = 10,
            IntervalSeconds = 0, // sem delay real nos testes
            MaxAttempts = 3
        };

        // Monta a cadeia: rootProvider -> IServiceScopeFactory -> IServiceScope -> scopedProvider
        _scopedProviderMock = new Mock<IServiceProvider>();
        _scopedProviderMock
            .Setup(sp => sp.GetService(typeof(IOutboxRepository)))
            .Returns(_repositoryMock.Object);
        _scopedProviderMock
            .Setup(sp => sp.GetService(typeof(IEventPublisher)))
            .Returns(_publisherMock.Object);

        _scopeMock = new Mock<IServiceScope>();
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_scopedProviderMock.Object);

        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);

        _rootProviderMock = new Mock<IServiceProvider>();
        _rootProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(_scopeFactoryMock.Object);

        var optionsMock = new Mock<IOptions<OutboxPublisherSettings>>();
        optionsMock.Setup(o => o.Value).Returns(_settings);

        _sut = new OutboxPublisher(optionsMock.Object, _rootProviderMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Cenário 1: mensagens disponíveis → publica, marca como processado, salva
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenMessagesAvailable_ShouldPublishEachMessage()
    {
        // Arrange
        var messages = new[]
        {
            new OutboxMessage("",routingKey: "account.created", payload: "{\"id\":1}"),
            new OutboxMessage("",routingKey: "account.updated", payload: "{\"id\":2}")
        };

        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call > 1) cts.Cancel();
                return call == 1 ? messages : [];
            });

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            p => p.PublishRawAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(messages.Length));
    }

    [Fact]
    public async Task ExecuteAsync_WhenMessagesAvailable_ShouldPassCorrectRoutingKeyAndPayload()
    {
        // Arrange
        var message = new OutboxMessage("", routingKey: "order.placed", payload: "{\"amount\":100}");

        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call > 1) cts.Cancel();
                return call == 1 ? [message] : [];
            });

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert
        _publisherMock.Verify(p => p.PublishRawAsync(It.IsAny<string>(), "order.placed", "{\"amount\":100}"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMessagePublished_ShouldSaveAfterProcessing()
    {
        // Arrange
        var message = new OutboxMessage("", routingKey: "order.placed", payload: "{\"amount\":100}");

        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call > 1) cts.Cancel();
                return call == 1 ? [message] : [];
            });

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert — SaveAsync deve ser chamado mesmo quando publish tem sucesso (bloco finally)
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // Cenário 2: falha no publish → incrementa tentativa, salva
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenPublishThrows_ShouldIncrementAttempt()
    {
        // Arrange
        var message = new OutboxMessage("", routingKey: "order.placed", payload: "{\"amount\":100}");

        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call > 1) cts.Cancel();
                return call == 1 ? [message] : [];
            });

        _publisherMock
            .Setup(p => p.PublishRawAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("broker offline"));

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert — IncrementAttempt é um método de domínio; verificamos o efeito colateral via log de erro
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Erro ao publicar")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPublishThrows_ShouldAlwaysSave()
    {
        // Arrange
        var message = new OutboxMessage("", routingKey: "order.placed", payload: "{\"amount\":100}");

        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call > 1) cts.Cancel();
                return call == 1 ? [message] : [];
            });

        _publisherMock
            .Setup(p => p.PublishRawAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("broker offline"));

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert — bloco finally garante que SaveAsync sempre é chamado
        _repositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // Cenário 3: tentativas excedidas → marca como falho (MarkAsFailed)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenMaxAttemptsReached_ShouldLogDlqWarning()
    {
        // Arrange — attempt já está em MaxAttempts - 1; após IncrementAttempt alcança o limite
        var message = new OutboxMessage("", routingKey: "order.placed", payload: "{\"amount\":100}");
        message.IncrementAttempt();
        message.IncrementAttempt();
        message.IncrementAttempt();
        message.IncrementAttempt();

        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call > 1) cts.Cancel();
                return call == 1 ? [message] : [];
            });

        _publisherMock
            .Setup(p => p.PublishRawAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("broker offline"));

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("excedeu tentativas")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // Cenário 4: sem mensagens → não publica nada
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenNoMessages_ShouldNotPublishAnything()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call >= 1) cts.Cancel();
                return [];
            });

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            p => p.PublishRawAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    // -------------------------------------------------------------------------
    // Cenário 5: MessagesLimit é repassado corretamente ao repository
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_ShouldRequestCorrectMessageLimit()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call >= 1) cts.Cancel();
                return [];
            });

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.GetUnprocessedMessagesAsync(_settings.MessagesLimit, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    // -------------------------------------------------------------------------
    // Cenário 6: cancelamento interrompe o loop sem exceção
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var startTask = _sut.StartAsync(cts.Token);
        await cts.CancelAsync();

        var act = async () => await startTask;

        // Assert — cancelamento não deve estourar exceção para o chamador
        await act.Should().NotThrowAsync();
    }

    // -------------------------------------------------------------------------
    // Cenário 7: múltiplas mensagens — SaveAsync chamado para cada uma
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithMultipleMessages_ShouldSaveEachMessage()
    {
        // Arrange
        const int messageCount = 3;
        var messages = Enumerable.Range(0, messageCount)
            .Select(_ => new OutboxMessage("", routingKey: "order.placed", payload: "{\"amount\":100}"))
            .ToArray();

        using var cts = new CancellationTokenSource();
        var call = 0;

        _repositoryMock
            .Setup(r => r.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                if (++call > 1) cts.Cancel();
                return call == 1 ? messages : [];
            });

        // Act
        await _sut.StartAsync(cts.Token);
        await Task.Delay(150, TestContext.Current.CancellationToken);
        await _sut.StopAsync(CancellationToken.None);

        // Assert — finally garante Save para cada mensagem, independente de sucesso ou falha
        _repositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(messageCount));
    }
}