namespace CryptoExchangeConsole.Services;

internal interface IConsoleService
{
	/// <summary>
	/// Executes a buy order with the provided amount
	/// </summary>
	/// <param name="amount">the amount of bitcoin to buy</param>
	/// <returns>void</returns>
	Task ExecuteBuyOrderAsync( decimal amount );

	/// <summary>
	/// Executes a sell order with the provided amount
	/// </summary>
	/// <param name="amount">the amount of bitcoin to sell</param>
	/// <returns>void</returns>
	Task ExecuteSellOrderAsync( decimal amount );

	/// <summary>
	/// Resets the database (deletes all records) and re-seeds it from the json-files
	/// </summary>
	/// <returns>void</returns>
	Task ResetDatabaseAsync();

}
