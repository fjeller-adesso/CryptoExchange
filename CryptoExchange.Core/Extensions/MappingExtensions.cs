using CryptoExchange.Contracts.DataObjects;
using CryptoExchange.Contracts.Models;

namespace CryptoExchange.Core.Extensions;

internal static class MappingExtensions
{
	public static BuyOrderItem MapToBuyOrderItem( this OrderExecution buyOrder )
	{
		BuyOrderItem result = new()
		{
			AmountToBuy = buyOrder.AmountToBuy,
			ExchangeId = buyOrder.ExchangeId,
			ExchangeName = buyOrder.ExchangeName,
			Price = buyOrder.Order.Price,
			Time = buyOrder.Order.TimeUtc,
			TotalCost = buyOrder.TotalCost
		};

		return result;
	}

	public static SellOrderItem MapToSellOrderItem( this SellExecution sellOrder )
	{
		SellOrderItem result = new()
		{
			AmountToSell = sellOrder.AmountToSell,
			ExchangeId = sellOrder.ExchangeId,
			ExchangeName = sellOrder.ExchangeName,
			Price = sellOrder.Order.Price,
			Time = sellOrder.Order.TimeUtc,
			TotalReceived = sellOrder.TotalReceived
		};

		return result;
	}
}
