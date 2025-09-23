namespace CryptoExchange.Contracts.DataObjects;

public class CoinExchangeOrder
{
	public required Guid Id { get; set; }

	public required DateTime TimeUtc { get; set; }

	public required string Type { get; set; }

	public required string Kind { get; set; }

	public required decimal Amount { get; set; }

	public required decimal Price { get; set; }
}
