using CryptoExchange.Contracts.Results;

namespace CryptoExchange.Contracts.Seeding;

public interface IDataSeeder
{
	SeedResult SeedDatabase();
}
