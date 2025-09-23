using CryptoExchange.Contracts.DataObjects;

namespace CryptoExchange.Contracts.Repositories;

public interface ICryptoExchangeRepository
{
	/// <summary>
	/// Gets all the exchanges in the database including all possible orders
	/// </summary>
	/// <returns>an <see cref="IEnumerable{CoinExchange}"/> with the exchange data</returns>
	Task<IEnumerable<CoinExchange>> GetExchangesAsync();

	/// <summary>
	/// Gets all buy orders from the database and also references the appropriate exchange
	/// </summary>
	/// <returns>an <see cref="IEnumerable{WorkingBuyOrder}"/> with the order data</returns>
	Task<IEnumerable<WorkingBuyOrder>> GetOrderedWorkingBuyOrdersAsync();

	/// <summary>
	/// Gets all sell orders from the database and also references the appropriate exchange
	/// </summary>
	/// <returns>an <see cref="IEnumerable{WorkingSellOrder}"/> with the order data</returns>
	Task<IEnumerable<WorkingSellOrder>> GetOrderedWorkingSellOrdersAsync();

	/// <summary>
	/// Updates the available Cryptos for the exchanges
	/// </summary>
	/// <param name="cryptoPerExchangeId">a dictionary with exchange ids and the new crypto value for the exchange</param>
	/// <returns>void</returns>
	Task UpdateAvailableCryptoAsync( Dictionary<Guid, decimal> cryptoPerExchangeId );

	/// <summary>
	/// Updates the available Funds for the exchanges
	/// </summary>
	/// <param name="exchangeUpdates">a dictionary with exchange ids and the new funds for the exchange</param>
	/// <returns>void</returns>
	Task UpdateAvailableFundsAsync( Dictionary<Guid, (decimal CryptoGained, decimal EuroSpent)> exchangeUpdates );

	/// <summary>
	/// Updates the orders that were used in the last request and deletes those who were depleted (have 0 
	/// bitcoin remaining). This is to update the orders correctly for the next run.
	/// </summary>
	/// <param name="orders">the orders to update or delete</param>
	/// <returns>void</returns>
	Task UpdateFulfilledWorkOrdersAsync( IEnumerable<CoinExchangeOrder> orders );

	/// <summary>
	/// Updates the data of a (changed) <see cref="CoinExchange"/> in the database.
	/// </summary>
	/// <param name="exchange">The <see cref="CoinExchange"/> to store</param>
	/// <returns>true if the update was successful, otherwise false</returns>
	Task<bool> UpdateExchangeAsync( CoinExchange exchange );

	/// <summary>
	/// Clears the database for a fresh start if wanted during runtime. 
	/// </summary>
	/// <returns>true if the database was cleared, otherwise false</returns>
	Task<bool> ClearDatabaseAsync();

}
