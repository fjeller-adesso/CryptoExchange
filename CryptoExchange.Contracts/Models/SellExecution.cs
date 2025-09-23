using CryptoExchange.Contracts.DataObjects;

namespace CryptoExchange.Contracts.Models;

public class SellExecution
{
	public required CoinExchangeOrder Order { get; set; }
	public required Guid ExchangeId { get; set; }
	public required decimal AmountToSell { get; set; }
	public required decimal TotalReceived { get; set; }
}