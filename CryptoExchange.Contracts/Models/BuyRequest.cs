using System.ComponentModel.DataAnnotations;

namespace CryptoExchange.Contracts.Models;

public class BuyRequest
{
	/// <summary>
	/// The amount of bitcoin to buy at the best possible price
	/// </summary>
	[Required]
	public decimal Amount { get; set; }
}
