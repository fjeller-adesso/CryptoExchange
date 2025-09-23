namespace CryptoExchange.Contracts.JsonObjects;

public class OrderBook
{
	public Bid[] Bids { get; set; } = [];

	public Ask[] Asks { get; set; } = [];
}
