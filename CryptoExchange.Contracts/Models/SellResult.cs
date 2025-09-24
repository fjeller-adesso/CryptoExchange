namespace CryptoExchange.Contracts.Models;

public class SellResult
{
	public required List<SellOrderItem> ExecutedOrders { get; set; }
	public required decimal TotalBitcoinSold { get; set; }
	public required decimal TotalReceived { get; set; }
	public required bool IsSuccessful { get; set; }
	public string? ErrorMessage { get; set; }

	public static SellResult Error( string errorMessage )
	{
		SellResult result = new()
		{
			ExecutedOrders = [],
			TotalBitcoinSold = 0,
			IsSuccessful = false,
			TotalReceived = 0,
			ErrorMessage = errorMessage
		};

		return result;
	}
}
