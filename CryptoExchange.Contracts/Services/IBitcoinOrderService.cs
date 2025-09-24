using CryptoExchange.Contracts.Models;

namespace CryptoExchange.Contracts.Services;

public interface IBitcoinOrderService
{

	/// <summary>
	/// Purchases the specified amount of Bitcoin for the lowest possible price.
	/// </summary>
	/// <param name="bitcoinAmountToBuy">The amount of Bitcoin to purchase, specified as a decimal value. Must be greater than zero.</param>
	/// <returns>A <see cref="BuyResult"/> object that provides details about the purchase</returns>
	Task<BuyResult> BuyAsync( decimal bitcoinAmountToBuy );

	/// <summary>
	/// Sells the specified amount of Bitcoin for the highest possible price.
	/// </summary>
	/// <param name="bitCoinAmountToSell">The amount of Bitcoin to sell, specified as a decimal value. Must be greater than zero.</param>
	/// <returns>A <see cref="SellResult"/> object that provides details about the sale</returns>
	Task<SellResult> SellAsync( decimal bitCoinAmountToSell );
}
