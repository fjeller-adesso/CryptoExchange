using CryptoExchange.Contracts.DataObjects;
using CryptoExchange.Contracts.Models;
using CryptoExchange.Contracts.Repositories;
using CryptoExchange.Contracts.Services;
using CryptoExchange.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Core.Services;

public class BitcoinOrderService : IBitcoinOrderService
{
	private readonly ICryptoExchangeRepository _cryptoRepository;
	private readonly ILogger<BitcoinOrderService> _logger;

	public BitcoinOrderService( ICryptoExchangeRepository cryptoRepository, ILogger<BitcoinOrderService> logger )
	{
		_cryptoRepository = cryptoRepository;
		_logger = logger;
	}

	/// <summary>
	/// Finds the optimal orders to buy the specified amount of Bitcoin for the lowest possible price
	/// across all exchanges. Orders can be executed multiple times if they offer the best price.
	/// Updates and persists exchange balances and order amounts after successful transactions.
	/// </summary>
	/// <param name="bitcoinAmountToBuy">Amount of Bitcoin to purchase</param>
	/// <returns>Result containing the optimal orders to execute</returns>
	private async Task<BuyResult> FindOptimalBuyOrdersAsync( decimal bitcoinAmountToBuy )
	{
		if ( bitcoinAmountToBuy <= 0 )
		{
			_logger.LogInformation( "Bitcoin amount to buy must be greater than 0, but is {Amount}", bitcoinAmountToBuy );
			return BuyResult.Error( "Bitcoin amount to buy must be greater than 0" );
		}

		// Remark: The fact that the exchange data is loaded at the begin of the transactions and saved after is not optimal,
		// however due to limited time I decided that for a test application this is good enough.
		List<WorkingBuyOrder> workingOrders = ( await _cryptoRepository.GetOrderedWorkingBuyOrdersAsync() ).ToList();

		var executedOrders = new List<OrderExecution>();
		decimal remainingToBuy = bitcoinAmountToBuy;
		decimal totalCost = 0;

		var exchangeUpdates = new Dictionary<Guid, decimal>();

		while ( remainingToBuy > 0 )
		{
			bool foundOrder = false;

			foreach ( WorkingBuyOrder workingOrder in workingOrders )
			{
				if ( workingOrder.RemainingAmount <= 0 || workingOrder.ExchangeCrypto <= 0 )
				{
					continue;
				}

				decimal maxAmountByOrder = Math.Min( remainingToBuy, workingOrder.RemainingAmount );
				decimal maxAmountByExchangeCrypto = workingOrder.ExchangeCrypto;
				decimal amountFromThisOrder = Math.Min( maxAmountByOrder, maxAmountByExchangeCrypto );

				if ( amountFromThisOrder <= 0 )
				{
					continue;
				}

				decimal costFromThisOrder = amountFromThisOrder * workingOrder.OriginalOrder.Price;

				executedOrders.Add( new OrderExecution
				{
					Order = workingOrder.OriginalOrder,
					ExchangeId = workingOrder.ExchangeId,
					ExchangeName = workingOrder.ExchangeName,
					AmountToBuy = amountFromThisOrder,
					TotalCost = costFromThisOrder
				} );

				remainingToBuy -= amountFromThisOrder;
				totalCost += costFromThisOrder;

				exchangeUpdates.TryAdd( workingOrder.ExchangeId, 0 );
				exchangeUpdates[workingOrder.ExchangeId] += amountFromThisOrder;

				workingOrder.RemainingAmount -= amountFromThisOrder;
				workingOrder.ExchangeCrypto -= amountFromThisOrder;

				workingOrder.OriginalOrder.Amount -= amountFromThisOrder;

				foundOrder = true;
				break;
			}

			if ( !foundOrder )
			{
				break;
			}
		}

		await _cryptoRepository.UpdateAvailableCryptoAsync( exchangeUpdates );

		IEnumerable<CoinExchangeOrder> ordersToUpdate = executedOrders.Select( o => o.Order );
		await _cryptoRepository.UpdateFulfilledWorkOrdersAsync( ordersToUpdate );

		bool successfullyPurchased = remainingToBuy <= 0;
		decimal actualBitcoinPurchased = bitcoinAmountToBuy - remainingToBuy;

		BuyResult result = new()
		{
			ExecutedOrders = executedOrders.Select( o => o.MapToBuyOrderItem() ).ToList(),
			TotalBitcoinPurchased = actualBitcoinPurchased,
			TotalCost = totalCost,
			IsSuccessful = successfullyPurchased,
			ErrorMessage = successfullyPurchased ? null : $"Only {actualBitcoinPurchased} Bitcoin could be purchased. Remaining needed: {remainingToBuy}"
		};

		return result;
	}

	/// <summary>
	/// Finds the optimal orders to sell the specified amount of Bitcoin for the highest possible price
	/// across all exchanges. Orders can be executed multiple times if they offer the best price,
	/// respecting the available Euro funds at each exchange.
	/// Updates and persists exchange balances and order amounts after successful transactions.
	/// </summary>
	/// <param name="bitcoinAmountToSell">Amount of Bitcoin to sell</param>
	/// <returns>Result containing the optimal orders to execute</returns>
	public async Task<SellResult> FindOptimalSellOrdersAsync( decimal bitcoinAmountToSell )
	{
		if ( bitcoinAmountToSell <= 0 )
		{
			_logger.LogInformation( "Bitcoin amount to sell must be greater than 0, but is {Amount}", bitcoinAmountToSell );
			return SellResult.Error( "Bitcoin amount to sell must be greater than 0" );
		}

		// Remark: The fact that the exchange data is loaded at the begin of the transactions and saved after is not optimal,
		// however due to limited time I decided that for a test application this is good enough.
		List<WorkingSellOrder> workingOrders = ( await _cryptoRepository.GetOrderedWorkingSellOrdersAsync() ).ToList();

		List<SellExecution> executedOrders = [];
		decimal remainingToSell = bitcoinAmountToSell;
		decimal totalReceived = 0;

		Dictionary<Guid, (decimal CryptoGained, decimal EuroSpent)> exchangeUpdates = new();

		while ( remainingToSell > 0 )
		{
			bool foundOrder = false;

			foreach ( WorkingSellOrder workingOrder in workingOrders )
			{
				if ( workingOrder.RemainingAmount <= 0 || workingOrder.ExchangeFunds <= 0 )
				{
					continue;
				}

				decimal maxAmountByOrder = Math.Min( remainingToSell, workingOrder.RemainingAmount );
				decimal maxAmountByFunds = workingOrder.ExchangeFunds / workingOrder.OriginalOrder.Price;

				decimal amountToThisOrder = Math.Min( maxAmountByOrder, maxAmountByFunds );

				if ( amountToThisOrder <= 0 )
				{
					continue;
				}

				decimal receivedFromThisOrder = amountToThisOrder * workingOrder.OriginalOrder.Price;

				executedOrders.Add( new SellExecution
				{
					Order = workingOrder.OriginalOrder,
					ExchangeId = workingOrder.ExchangeId,
					ExchangeName = workingOrder.ExchangeName,
					AmountToSell = amountToThisOrder,
					TotalReceived = receivedFromThisOrder
				} );

				remainingToSell -= amountToThisOrder;
				totalReceived += receivedFromThisOrder;

				if ( !exchangeUpdates.ContainsKey( workingOrder.ExchangeId ) )
				{
					exchangeUpdates[workingOrder.ExchangeId] = (0, 0);
				}

				(decimal CryptoGained, decimal EuroSpent) currentUpdate = exchangeUpdates[workingOrder.ExchangeId];

				exchangeUpdates[workingOrder.ExchangeId] = (
					currentUpdate.CryptoGained + amountToThisOrder,
					currentUpdate.EuroSpent + receivedFromThisOrder
				);

				workingOrder.RemainingAmount -= amountToThisOrder;
				workingOrder.ExchangeFunds -= receivedFromThisOrder;
				workingOrder.OriginalOrder.Amount -= amountToThisOrder;

				foundOrder = true;
				break;
			}

			if ( !foundOrder )
			{
				break;
			}
		}

		await _cryptoRepository.UpdateAvailableFundsAsync( exchangeUpdates );

		IEnumerable<CoinExchangeOrder> ordersToUpdate = executedOrders.Select( o => o.Order );
		await _cryptoRepository.UpdateFulfilledWorkOrdersAsync( ordersToUpdate );

		bool successfullySold = remainingToSell <= 0;
		decimal actualBitcoinSold = bitcoinAmountToSell - remainingToSell;

		SellResult result = new()
		{
			ExecutedOrders = executedOrders.Select( o => o.MapToSellOrderItem() ).ToList(),
			TotalBitcoinSold = actualBitcoinSold,
			TotalReceived = totalReceived,
			IsSuccessful = successfullySold,
			ErrorMessage = successfullySold ? null : $"Only {actualBitcoinSold} Bitcoin could be sold. Remaining unsold: {remainingToSell}"
		};

		return result;
	}

	/// <inheritdoc />
	public async Task<BuyResult> BuyAsync( decimal bitcoinAmountToBuy ) => await FindOptimalBuyOrdersAsync( bitcoinAmountToBuy );

	/// <inheritdoc />
	public async Task<SellResult> SellAsync( decimal bitCoinAmountToSell ) => await FindOptimalSellOrdersAsync( bitCoinAmountToSell );
}