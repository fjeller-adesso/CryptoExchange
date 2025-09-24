namespace CryptoExchange.Contracts.Models;

public class SellOrderItem
{
	public required Guid ExchangeId { get; set; }

	public required string ExchangeName { get; set; }

	public required decimal Price { get; set; }

	public required DateTime Time { get; set; }

	public required decimal AmountToSell { get; set; }

	public required decimal TotalReceived { get; set; }
}
