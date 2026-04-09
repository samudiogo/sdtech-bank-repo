using FluentAssertions;
using Moq;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Transactions.Contracts.Events;
using SdtechBank.Domain.Shared.Messaging;

namespace SdtechBank.Application.Tests.IntegrationEvents;

public class OutboxServiceTests
{
    private readonly Mock<IOutboxRepository> _repositoryMock;
    private readonly OutboxService _sut;

    public OutboxServiceTests()
    {
        _repositoryMock = new Mock<IOutboxRepository>();
        _sut = new OutboxService(_repositoryMock.Object);
    }

    [Fact]
    public async Task AddEventAsync_WhenValidEvent_ShouldPersistMessageWithCorrectRoutingKey()
    {
        //arrange:
        var @event = new TransactionCompletedIntegrationEvent() { Amount = 100, TransactionId = Guid.NewGuid(), PaymentId = Guid.NewGuid() };

        //act:
        await _sut.AddEventAsync(@event, CancellationToken.None);

        //assert:
        _repositoryMock.Verify(r => r.AddAsync(
            It.Is<OutboxMessage>(m =>
            m.RoutingKey == @event.RoutingKey &&
            m.Type == nameof(TransactionCompletedIntegrationEvent) &&
            m.Payload.Contains(@event.TransactionId.ToString(), StringComparison.InvariantCultureIgnoreCase)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task AddEventAsync_WhenValidEvent_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var @event = new TransactionCompletedIntegrationEvent
        {
            Amount = 100,
            TransactionId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid()
        };

        // Act
        await _sut.AddEventAsync(@event, cts.Token);

        // Assert — garante que o token não é descartado internamente
        _repositoryMock.Verify(r => r.AddAsync(
            It.IsAny<OutboxMessage>(),
            cts.Token),  // token específico, não IsAny
            Times.Once);
    }

    [Fact]
    public async Task AddEventAsync_WhenEventDoesNotImplementIIntegrationEvent_ShouldThrowInvalidCastException()
    {
        // Arrange — evento sem a interface
        var invalidEvent = new { Foo = "bar" };

        // Act
        var act = () => _sut.AddEventAsync(invalidEvent, CancellationToken.None);

        // Assert — o cast explícito deve explodir de forma previsível
        await act.Should().ThrowAsync<InvalidCastException>();
    }

    [Fact]
    public async Task AddEventAsync_WhenRepositoryThrows_ShouldNotSwallowException()
    {
        // Arrange
        var @event = new TransactionCompletedIntegrationEvent
        {
            Amount = 100,
            TransactionId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB indisponível"));

        // Act
        var act = () => _sut.AddEventAsync(@event, CancellationToken.None);

        // Assert — OutboxService não deve engolir a exceção
        await act.Should().ThrowAsync<Exception>().WithMessage("DB indisponível");
    }
}
