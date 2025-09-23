namespace CryptoExchange.Contracts.Models;

public class BuyResult
{
	public List<OrderExecution> ExecutedOrders { get; set; } = [];
	public required decimal TotalBitcoinPurchased { get; set; }
	public required decimal TotalCost { get; set; }
	public required bool SuccessfullyPurchased { get; set; }
	public string? ErrorMessage { get; set; }

	public static BuyResult Error(string errorMessage )
	{
		BuyResult result = new()
		{
			ExecutedOrders = [],
			TotalBitcoinPurchased = 0,
			SuccessfullyPurchased = false,
			TotalCost = 0,
			ErrorMessage = errorMessage
		};

		return result;
	}

	public static BuyResult Success(List<OrderExecution> executedOrders, decimal bitCoinPurchased, decimal totalCost )
	{
		BuyResult result = new()
		{
			ExecutedOrders = executedOrders,
			TotalBitcoinPurchased = bitCoinPurchased,
			SuccessfullyPurchased = true,
			TotalCost = totalCost,
			ErrorMessage = null
		};

		return result;
	}
}
