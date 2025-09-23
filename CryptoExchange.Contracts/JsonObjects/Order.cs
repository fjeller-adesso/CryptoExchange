namespace CryptoExchange.Contracts.JsonObjects;

public class Order
{
	public required string Id { get; set; }

	public required DateTime Time { get; set; }
	
	public required string Type { get; set; }
	
	public required string Kind { get; set; }
	
	public required decimal Amount { get; set; }
	
	public required decimal Price { get; set; }
}
