using SdtechBank.Application.Abstractions.Persistence;
using SdtechBank.Infrastructure.Shared.Mongo;

namespace SdtechBank.Infrastructure.Persistence;

public class UnitOfWork(MongoDbContext context) : IUnitOfWork
{
    public Task BeginAsync(CancellationToken ct) => context.StartSessionAsync(ct);

    public Task CommitAsync(CancellationToken ct) => context.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct) => context.RollbackAsync(ct);
}