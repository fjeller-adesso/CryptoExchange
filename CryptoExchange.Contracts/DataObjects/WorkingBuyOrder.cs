namespace CryptoExchange.Contracts.DataObjects;

public class WorkingBuyOrder
{
	public required CoinExchangeOrder OriginalOrder { get; set; }

	public required Guid ExchangeId { get; set; }

	public required string ExchangeName { get; set; }

	public required decimal RemainingAmount { get; set; }

	public required decimal ExchangeCrypto { get; set; }
}
