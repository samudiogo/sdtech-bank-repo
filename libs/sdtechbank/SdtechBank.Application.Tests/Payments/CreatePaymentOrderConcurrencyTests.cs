using System.Collections.Concurrent;
using FluentAssertions;
using Moq;
using SdtechBank.Application.Abstractions.Persistence;
using SdtechBank.Application.Messaging;
using SdtechBank.Application.Payments.UseCases.CreatePayment;
using SdtechBank.Domain.PaymentOrders.Contracts;
using SdtechBank.Domain.PaymentOrders.Entities;
using SdtechBank.Domain.PaymentOrders.ValueObjects;
using SdtechBank.Domain.Shared.Exceptions;
using SdtechBank.Domain.Shared.ValueObjects;
using SdtechBank.Shared.DTOs.Payments.Requests;

namespace SdtechBank.Application.Tests.Payments;

public class CreatePaymentOrderConcurrencyTests
{
    private readonly Mock<IPaymentOrderRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOutboxService> _outboxServiceMock;
    private readonly CreatePaymentValidator _validator;
    private readonly CreatePaymentUseCase _useCase;
    private ConcurrentDictionary<string, PaymentOrder> _storage;
    private ConcurrentDictionary<string, PaymentOrder> _transactionBuffer;
    private bool _inTransaction;

    public CreatePaymentOrderConcurrencyTests()
    {
        _repositoryMock = new Mock<IPaymentOrderRepository>();
        _outboxServiceMock = new Mock<IOutboxService>();
        _validator = new CreatePaymentValidator();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new CreatePaymentUseCase(_repositoryMock.Object, _outboxServiceMock.Object, _unitOfWorkMock.Object, _validator);

        _storage = new();
        _inTransaction = false;
        _transactionBuffer = new();


    }


    private void SetupRepository()
    {
        _storage.Clear();

        _repositoryMock
        .Setup(r => r.GetByIdempotencyKeyAsync(It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((IdempotencyKey key, CancellationToken _) =>
        {
            _storage.TryGetValue(key.Value, out var existing);
            return existing;
        });

        _repositoryMock
        .Setup(r => r.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()))
        .Callback((PaymentOrder order, CancellationToken _) =>
        {
            var target = _inTransaction ? _transactionBuffer : _storage;

            if (!target.TryAdd(order.IdempotencyKey.Value, order))
                throw new DuplicateKeyException("duplicate key");
        })
        .Returns(Task.CompletedTask);

        _repositoryMock
        .Setup(r => r.ExistsRecentSimilarAsync(
            It.IsAny<Guid>(),
            It.IsAny<PaymentDestination>(),
            It.IsAny<Money>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.BeginAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                _inTransaction = true;
                _transactionBuffer = new();
            })
        .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                foreach (var item in _transactionBuffer)
                    _storage[item.Key] = item.Value;

                _inTransaction = false;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                _transactionBuffer.Clear();
                _inTransaction = false;
            })
            .Returns(Task.CompletedTask);
    }
    private static CreatePaymentRequest CreateValidRequest(string key) =>
        new()
        {
            IdempotencyKey = key,
            PayerId = Guid.NewGuid().ToString(),
            Amount = 100,
            Receiver = new PaymentReceiverRequest { PixKey = "pix@key.com" }
        };

    [Fact]
    public async Task CreatePayment_Should_Create_Only_One_Payment_When_Concurrent_Requests_Occur()
    {
        //arrange:
        SetupRepository();
        var request = CreateValidRequest("teste-idempotente-123");
        var tasks = Enumerable.Range(0, 10).Select(_ => _useCase.ExecuteAsync(request, CancellationToken.None));

        // act:
        var results = await Task.WhenAll(tasks);

        //assert:
        _storage.Should().HaveCount(1);
        results.Should().OnlyContain(r => r.IsSuccess);
        _outboxServiceMock.Verify(x => x.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_Should_Remain_Idempotent_When_An_Intermediate_Failure_Occurs()
    {
        //arrange:
        SetupRepository();
        var request = CreateValidRequest("teste-idempotente-123");
        var firstCall = true;

        _outboxServiceMock
            .Setup(o => o.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                if (firstCall)
                {
                    firstCall = false;
                    throw new Exception("falha simulada");
                }

                return Task.CompletedTask;
            });

        // act:
        Func<Task> firstCallAction = () => _useCase.ExecuteAsync(request, CancellationToken.None);
        await firstCallAction.Should().ThrowAsync<Exception>().WithMessage("falha simulada");
        var secondResult = await _useCase.ExecuteAsync(request, CancellationToken.None);

        //assert:
        _storage.Should().HaveCount(1);
        secondResult.IsSuccess.Should().BeTrue();

        _outboxServiceMock.Verify(
        x => x.AddEventAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
        Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessPayment_Should_Detect_Duplicate_Payment_When_Requests_Occur_Within_One_Minute()
    {
        // arrange
        var payerId = Guid.NewGuid().ToString();

        var request1 = new CreatePaymentRequest
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
            PayerId = payerId,
            Amount = 100,
            Receiver = new PaymentReceiverRequest { PixKey = "pix@key.com" }
        };

        var request2 = new CreatePaymentRequest
        {
            IdempotencyKey = Guid.NewGuid().ToString(), // diferente
            PayerId = payerId, // mesmos dados
            Amount = 100,
            Receiver = new PaymentReceiverRequest { PixKey = "pix@key.com" }
        };

        // idempotência NÃO interfere (keys diferentes)
        _repositoryMock
            .Setup(r => r.GetByIdempotencyKeyAsync(It.IsAny<IdempotencyKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentOrder?)null);

        // primeira chamada: não existe similar
        // segunda chamada: existe similar
        _repositoryMock
            .SetupSequence(r => r.ExistsRecentSimilarAsync(
                It.IsAny<Guid>(),
                It.IsAny<PaymentDestination>(),
                It.IsAny<Money>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        var first = await _useCase.ExecuteAsync(request1, CancellationToken.None);
        var second = await _useCase.ExecuteAsync(request2, CancellationToken.None);

        // assert
        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeFalse();

        second.Errors.Should().Contain(e => e.Message.Contains("Ordem de Pagamento"));

        // garantia extra: só salvou uma vez
        _repositoryMock.Verify(
            r => r.SaveAsync(It.IsAny<PaymentOrder>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}