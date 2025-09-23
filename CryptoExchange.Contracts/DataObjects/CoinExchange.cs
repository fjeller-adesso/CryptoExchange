namespace CryptoExchange.Contracts.DataObjects;

public class CoinExchange
{
	public Guid Id { get; set; } = Guid.CreateVersion7();

	public required string Name { get; set; }

	public required decimal AvailableCrypto { get; set; }

	public required decimal AvailableEuro { get; set; }

	public List<CoinExchangeOrder> Bids { get; set; } = [];

	public List<CoinExchangeOrder> Asks { get; set; } = [];
}
