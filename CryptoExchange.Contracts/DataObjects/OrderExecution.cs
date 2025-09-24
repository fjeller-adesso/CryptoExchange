namespace CryptoExchange.Contracts.DataObjects;

public class OrderExecution
{
	public required CoinExchangeOrder Order { get; set; }

	public required Guid ExchangeId { get; set; }

	public required string ExchangeName { get; set; }

	public required decimal AmountToBuy { get; set; }

	public required decimal TotalCost { get; set; }
}

