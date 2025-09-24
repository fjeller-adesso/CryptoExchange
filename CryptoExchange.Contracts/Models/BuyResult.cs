namespace CryptoExchange.Contracts.Models;

public class BuyResult
{
	public List<BuyOrderItem> ExecutedOrders { get; set; } = [];
	public required decimal TotalBitcoinPurchased { get; set; }
	public required decimal TotalCost { get; set; }
	public required bool IsSuccessful { get; set; }
	public string? ErrorMessage { get; set; }

	public static BuyResult Error(string errorMessage )
	{
		BuyResult result = new()
		{
			ExecutedOrders = [],
			TotalBitcoinPurchased = 0,
			IsSuccessful = false,
			TotalCost = 0,
			ErrorMessage = errorMessage
		};

		return result;
	}
}
