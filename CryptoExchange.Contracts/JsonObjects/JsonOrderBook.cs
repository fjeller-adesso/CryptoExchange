namespace CryptoExchange.Contracts.JsonObjects;

public class JsonOrderBook
{
	public required string Id { get; set; }
	public required Availablefunds AvailableFunds { get; set; }
	public required OrderBook OrderBook { get; set; }
}







