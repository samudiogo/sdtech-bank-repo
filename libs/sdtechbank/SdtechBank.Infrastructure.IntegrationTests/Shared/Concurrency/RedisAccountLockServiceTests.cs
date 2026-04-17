using FluentAssertions;
using SdtechBank.Infrastructure.Shared.Concurrency;
using StackExchange.Redis;

namespace SdtechBank.Infrastructure.IntegrationTests.Shared.Concurrency;

public class RedisAccountLockServiceTests(RedisFixture fixture)
    : IClassFixture<RedisFixture>
{
    [Fact]
    public async Task Should_acquire_and_release_lock()
    {
        var redis =
            await ConnectionMultiplexer.ConnectAsync(
                fixture.ConnectionString);

        var sut =
            new RedisAccountLockService(redis);

        var accountId = Guid.NewGuid();

        using var cts =
            new CancellationTokenSource();

        var handle =
            await sut.AcquireLockAsync(
                accountId,
                cts.Token);

        handle.Should().NotBeNull();

        await handle.DisposeAsync();

        var second =
            await sut.AcquireLockAsync(
                accountId,
                cts.Token);

        second.Should().NotBeNull();

        await second.DisposeAsync();
    }

    [Fact]
    public async Task Should_block_second_request_until_release()
    {
        var redis =
            await ConnectionMultiplexer.ConnectAsync(
                fixture.ConnectionString);

        var sut =
            new RedisAccountLockService(redis);

        var accountId = Guid.NewGuid();

        using var cts =
            new CancellationTokenSource();

        var first =
            await sut.AcquireLockAsync(
                accountId,
                cts.Token);

        var secondTask =
            Task.Run(() =>
                sut.AcquireLockAsync(
                    accountId,
                    cts.Token));

        await Task.Delay(
            500,
            TestContext.Current.CancellationToken);

        secondTask.IsCompleted.Should().BeFalse();

        await first.DisposeAsync();

        var second = await secondTask;

        second.Should().NotBeNull();

        await second.DisposeAsync();
    }

    [Fact]
    public async Task Should_timeout_when_lock_is_busy()
    {
        var redis =
            await ConnectionMultiplexer.ConnectAsync(
                fixture.ConnectionString);

        var sut =
            new RedisAccountLockService(redis);

        var accountId = Guid.NewGuid();

        using var firstCts =
            new CancellationTokenSource();

        var first =
            await sut.AcquireLockAsync(
                accountId,
                firstCts.Token);

        using var secondCts =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(6));

        var act = async () =>
            await sut.AcquireLockAsync(
                accountId,
                secondCts.Token);

        await act.Should()
            .ThrowAsync<TimeoutException>();

        await first.DisposeAsync();
    }
}