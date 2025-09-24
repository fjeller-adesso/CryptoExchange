using CryptoExchange.Contracts.Models;
using CryptoExchange.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace CryptoExchangeConsole.Services;

internal class ConsoleService : IConsoleService
{
	private readonly IBitcoinOrderService _bitcoinOrderService;
	private readonly IResetDatabaseService _resetDatabaseService;

	private readonly ILogger<ConsoleService> _logger;
	

	public ConsoleService(IBitcoinOrderService bitcoinOrderService, IResetDatabaseService resetDatabaseService, ILogger<ConsoleService> logger)
	{
		_bitcoinOrderService = bitcoinOrderService;
		_resetDatabaseService = resetDatabaseService;
		_logger = logger;
	}

	public async Task ExecuteBuyOrderAsync( decimal amount )
	{
		try
		{
			Console.WriteLine( $"Executing buy order for {amount:F8} Bitcoin..." );
			Console.WriteLine();

			if ( amount <= 0 )
			{
				Console.WriteLine( "Error: Amount must be greater than 0" );
				return;
			}

			var result = await _bitcoinOrderService.BuyAsync( amount );

			if ( result.IsSuccessful )
			{
				Console.WriteLine( "Buy order executed successfully!" );
				Console.WriteLine( $"Total Bitcoin Purchased: {result.TotalBitcoinPurchased:F8} BTC" );
				Console.WriteLine( $"Total Cost: {result.TotalCost:F2} EUR" );
				Console.WriteLine( $"Average Price: €{( result.TotalCost / result.TotalBitcoinPurchased ):F2} per BTC" );
				Console.WriteLine();

				if ( result.ExecutedOrders.Any() )
				{
					Console.WriteLine( "Executed Orders:" );
					foreach ( var order in result.ExecutedOrders )
					{
						Console.WriteLine( $"    Exchange ID:   {order.ExchangeId}" );
						Console.WriteLine( $"    Exchange Name: {order.ExchangeName}");
						Console.WriteLine( $"    Amount:        {order.AmountToBuy:F8} BTC" );
						Console.WriteLine( $"    Price:         {order.Price:F2} EUR" );
						Console.WriteLine( $"    Cost:          {order.TotalCost:F2} EUR" );
						Console.WriteLine();
					}
				}
			}
			else
			{
				Console.WriteLine( "Buy order failed!" );
				Console.WriteLine( $"Bitcoin Purchased: {result.TotalBitcoinPurchased:F8} BTC" );
				Console.WriteLine( $"Total Cost: €{result.TotalCost:F2}" );
				if ( !string.IsNullOrEmpty( result.ErrorMessage ) )
				{
					Console.WriteLine( $"Error: {result.ErrorMessage}" );
				}
			}
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "Error executing buy order" );
			Console.WriteLine( $"❌ An error occurred: {ex.Message}" );
		}
	}

	public async Task ExecuteSellOrderAsync( decimal amount )
	{
		try
		{
			Console.WriteLine( $"Executing sell order for {amount:F8} Bitcoin..." );
			Console.WriteLine();

			if ( amount <= 0 )
			{
				Console.WriteLine( "Error: Amount must be greater than 0" );
				return;
			}

			var result = await _bitcoinOrderService.SellAsync( amount );

			if ( result.IsSuccessful )
			{
				Console.WriteLine( "Sell order executed successfully!" );
				Console.WriteLine( $"Total Bitcoin Sold: {result.TotalBitcoinSold:F8} BTC" );
				Console.WriteLine( $"Total Received: €{result.TotalReceived:F2}" );
				Console.WriteLine( $"Average Price: €{( result.TotalReceived / result.TotalBitcoinSold ):F2} per BTC" );
				Console.WriteLine();

				if ( result.ExecutedOrders.Any() )
				{
					Console.WriteLine( "Executed Orders:" );
					foreach ( var order in result.ExecutedOrders )
					{
						Console.WriteLine( $"    Exchange ID:   {order.ExchangeId}" );
						Console.WriteLine( $"    Exchange Name: {order.ExchangeName}" );
						Console.WriteLine( $"    Amount:        {order.AmountToSell:F8} BTC" );
						Console.WriteLine( $"    Price:	        {order.Price:F2} EUR" );
						Console.WriteLine( $"    Received:      {order.TotalReceived:F2}" );
						Console.WriteLine();
					}
				}
			}
			else
			{
				Console.WriteLine( "Sell order failed!" );
				Console.WriteLine( $"Bitcoin Sold: {result.TotalBitcoinSold:F8} BTC" );
				Console.WriteLine( $"Total Received: €{result.TotalReceived:F2}" );
				if ( !string.IsNullOrEmpty( result.ErrorMessage ) )
				{
					Console.WriteLine( $"Error: {result.ErrorMessage}" );
				}
			}
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "Error executing sell order" );
			Console.WriteLine( $"An error occurred: {ex.Message}" );
		}
	}

	public async Task ResetDatabaseAsync()
	{
		try
		{
			Console.WriteLine( $"Attempting to reset the database and seed it with original data ..." );
			Console.WriteLine();

			ResetResult result = await _resetDatabaseService.ResetDatabaseAsync();

			Console.WriteLine( result.Message );
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "Error resetting database" );
			Console.WriteLine( $"An error occurred: {ex.Message}" );
		}
	}
}
