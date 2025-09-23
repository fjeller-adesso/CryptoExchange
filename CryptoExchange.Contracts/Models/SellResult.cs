namespace CryptoExchange.Contracts.Models;

public class SellResult
{
	public required List<SellExecution> ExecutedOrders { get; set; }
	public required decimal TotalBitcoinSold { get; set; }
	public required decimal TotalReceived { get; set; }
	public required bool SuccessfullySold { get; set; }
	public string? ErrorMessage { get; set; }

	public static SellResult Error( string errorMessage )
	{
		SellResult result = new()
		{
			ExecutedOrders = [],
			TotalBitcoinSold = 0,
			SuccessfullySold = false,
			TotalReceived = 0,
			ErrorMessage = errorMessage
		};

		return result;
	}

	public static SellResult Success( List<SellExecution> executedOrders, decimal bitCoinSold, decimal totalReceived )
	{
		SellResult result = new()
		{
			ExecutedOrders = executedOrders,
			TotalBitcoinSold = bitCoinSold,
			SuccessfullySold = true,
			TotalReceived = totalReceived,
			ErrorMessage = null
		};

		return result;
	}
}
