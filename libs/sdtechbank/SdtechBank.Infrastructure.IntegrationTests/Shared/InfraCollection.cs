
using SdtechBank.Infrastructure.IntegrationTests.Messaging;
namespace SdtechBank.Infrastructure.IntegrationTests.Shared;

[CollectionDefinition("Infra")]
public class InfraCollection : ICollectionFixture<RedisFixture>, ICollectionFixture<RabbitMqFixture>, ICollectionFixture<MongoDbFixture> { }