namespace CryptoExchangeConsole.Services;

internal interface IConsoleService
{

	Task ExecuteBuyOrderAsync( decimal amount );

	Task ExecuteSellOrderAsync( decimal amount );

	Task ResetDatabaseAsync();

}
