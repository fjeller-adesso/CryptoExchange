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
	/// Updates both available funds and fulfilled orders within a single transaction
	/// </summary>
	/// <param name="exchangeUpdates">Dictionary of exchange updates</param>
	/// <param name="ordersToUpdate">Orders to update or delete</param>
	/// <returns>void</returns>
	Task UpdateFundsAndOrdersAsync(
		Dictionary<Guid, (decimal CryptoGained, decimal EuroSpent)> exchangeUpdates,
		IEnumerable<CoinExchangeOrder> ordersToUpdate );

	/// <summary>
	/// Updates both available crypto and fulfilled orders within a single transaction
	/// </summary>
	/// <param name="cryptoUpdates">Dictionary of crypto updates per exchange</param>
	/// <param name="ordersToUpdate">Orders to update or delete</param>
	/// <returns>void</returns>
	Task UpdateCryptoAndOrdersAsync(
		Dictionary<Guid, decimal> cryptoUpdates,
		IEnumerable<CoinExchangeOrder> ordersToUpdate );

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
