using CryptoExchange.Contracts.DataObjects;
using CryptoExchange.Contracts.JsonObjects;
using CryptoExchange.Data.PostgreSql.DataAccess.Entities;

namespace CryptoExchange.Data.PostgreSql.Extensions;

internal static class MappingExtensions
{
	private const string _TYPENAME_BID = "buy";
	private const string _TYPENAME_ASK = "sell";

	public static CoinExchangeOrder MapToCoinExchangeOrder( this Order order )
	{
		CoinExchangeOrder result = new()
		{
			Amount = order.Amount,
			Id = new Guid( order.Id ),
			Kind = order.Kind,
			Price = order.Price,
			TimeUtc = order.Time,
			Type = order.Type
		};

		return result;
	}

	public static CoinExchangeOrder MapToCoinExchangeOrder( this Ask ask ) => ask.Order.MapToCoinExchangeOrder();

	public static CoinExchangeOrder MapToCoinExchangeOrder( this Bid bid ) => bid.Order.MapToCoinExchangeOrder();

	public static CoinExchange MapToCoinExchange( this JsonOrderBook book )
	{
		CoinExchange result = new()
		{
			AvailableCrypto = book.AvailableFunds.Crypto,
			AvailableEuro = book.AvailableFunds.Euro,
			Name = book.Id,
			Bids = book.OrderBook.Bids.Select( b => b.MapToCoinExchangeOrder() ).ToList(),
			Asks = book.OrderBook.Asks.Select( b => b.MapToCoinExchangeOrder() ).ToList()
		};

		return result;
	}

	public static WorkingBuyOrder MapToWorkingBuyOrder(this ExchangeOrderEntity entity )
	{
		WorkingBuyOrder result = new()
		{
			ExchangeCrypto = entity.Exchange.AvailableCrypto,
			ExchangeId = entity.Exchange.Id,
			ExchangeName = entity.Exchange.Name,
			RemainingAmount = entity.Amount,
			OriginalOrder = entity.MapToCoinExchangeOrder()
		};

		return result;
	}

	public static WorkingSellOrder MapToWorkingSellOrder(this ExchangeOrderEntity entity )
	{
		WorkingSellOrder result = new()
		{
			ExchangeFunds = entity.Exchange.AvailableEuro,
			ExchangeId = entity.Exchange.Id,
			ExchangeName = entity.Exchange.Name,
			RemainingAmount = entity.Amount,
			OriginalOrder = entity.MapToCoinExchangeOrder()
		};

		return result;
	}

	public static ExchangeOrderEntity MapToEntity( this CoinExchangeOrder order, Guid exchangeId )
	{
		ExchangeOrderEntity result = new()
		{
			Amount = order.Amount,
			ExchangeId = exchangeId,
			Id = order.Id,
			Kind = order.Kind,
			Price = order.Price,
			Type = order.Type,
			Time = order.TimeUtc
		};

		return result;
	}

	public static void UpdateEntity(this ExchangeEntity exchange, CoinExchange data )
	{
		exchange.AvailableCrypto = data.AvailableCrypto;
		exchange.AvailableEuro = data.AvailableEuro;
	}

	public static CoinExchangeOrder MapToCoinExchangeOrder( this ExchangeOrderEntity entity )
	{
		CoinExchangeOrder result = new()
		{
			Amount = entity.Amount,
			Id = entity.Id,
			Kind = entity.Kind,
			Price = entity.Price,
			TimeUtc = entity.Time,
			Type = entity.Type
		};

		return result;
	}

	public static ExchangeEntity MapToEntity( this CoinExchange exchange )
	{
		ExchangeEntity result = new()
		{
			AvailableCrypto = exchange.AvailableCrypto,
			AvailableEuro = exchange.AvailableEuro,
			Id = exchange.Id,
			Name = exchange.Name
		};

		return result;
	}

	public static CoinExchange MapToCoinExchange( this ExchangeEntity entity )
	{
		CoinExchange result = new()
		{
			AvailableCrypto = entity.AvailableCrypto,
			AvailableEuro = entity.AvailableEuro,
			Name = entity.Name,
			Id = entity.Id
		};

		List<CoinExchangeOrder> orders = entity.ExchangeOrders.Select( o => o.MapToCoinExchangeOrder() ).ToList();

		result.Bids = orders.Where( o => _TYPENAME_BID.Equals( o.Type, StringComparison.OrdinalIgnoreCase ) ).ToList();
		result.Asks = orders.Where( o => _TYPENAME_ASK.Equals( o.Type, StringComparison.OrdinalIgnoreCase ) ).ToList();

		return result;
	}


}
