using System.ComponentModel.DataAnnotations;

namespace CryptoExchange.Contracts.Models;

public class SellRequest
{
	/// <summary>
	/// The amount of bitcoin to sell at the highest possible price
	/// </summary>
	[Required]
	public decimal Amount { get; set; }
}
