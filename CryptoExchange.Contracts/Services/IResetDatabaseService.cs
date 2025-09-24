using CryptoExchange.Contracts.Models;

namespace CryptoExchange.Contracts.Services;

public interface IResetDatabaseService
{
	Task<ResetResult> ResetDatabaseAsync();
}
