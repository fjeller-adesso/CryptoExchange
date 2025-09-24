namespace CryptoExchange.Contracts.Models;

public class BuyOrderItem
{
	public required Guid ExchangeId { get; set; }

	public required string ExchangeName { get; set; }

	public required decimal Price { get; set; }

	public required DateTime Time { get; set; }

	public required decimal AmountToBuy { get; set; }

	public required decimal TotalCost { get; set; }
}
