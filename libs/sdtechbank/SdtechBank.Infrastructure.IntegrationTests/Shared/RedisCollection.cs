namespace SdtechBank.Infrastructure.IntegrationTests.Shared;

[CollectionDefinition("redis")]
public class RedisCollection :
    ICollectionFixture<RedisFixture>
{
}