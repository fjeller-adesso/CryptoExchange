using CryptoExchange.Contracts.Results;

namespace CryptoExchange.Contracts.Seeding;

public interface IDataSeeder
{
	/// <summary>
	/// Seeds the database with the data from the json-files
	/// </summary>
	/// <returns>A <see cref="SeedResult"/> value showing if the operation was successful</returns>
	SeedResult SeedDatabase();

}
