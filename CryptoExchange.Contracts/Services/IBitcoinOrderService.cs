using CryptoExchange.Contracts.Models;

namespace CryptoExchange.Contracts.Services;

public interface IBitcoinOrderService
{
	Task<BuyResult> BuyAsync( decimal bitcoinAmountToBuy );

	Task<SellResult> SellAsync( decimal bitCoinAmountToSell );
}
