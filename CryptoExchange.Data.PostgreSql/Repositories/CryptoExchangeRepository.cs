using CryptoExchange.Contracts.DataObjects;
using CryptoExchange.Contracts.Repositories;
using CryptoExchange.Data.PostgreSql.DataAccess.Context;
using CryptoExchange.Data.PostgreSql.DataAccess.Entities;
using CryptoExchange.Data.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Data.PostgreSql.Repositories;

public class CryptoExchangeRepository : ICryptoExchangeRepository
{
	private const string _TYPENAME_BID = "Buy";
	private const string _TYPENAME_ASK = "Sell";

	private readonly CryptoDataContext _dataContext;
	private readonly ILogger<CryptoDataContext> _logger;

	public CryptoExchangeRepository( CryptoDataContext dataContext, ILogger<CryptoDataContext> logger )
	{
		_dataContext = dataContext;
		_logger = logger;
	}

	public async Task<IEnumerable<CoinExchange>> GetExchangesAsync()
	{
		try
		{
			List<ExchangeEntity> exchanges = await _dataContext.ExchangeEntities
				.AsNoTracking()
				.Include( e => e.ExchangeOrders )
				.ToListAsync();

			IEnumerable<CoinExchange> result = exchanges.Select( e => e.MapToCoinExchange() );

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "Error while retrieving exchanges from database in {MethodName}", nameof( GetExchangesAsync ) );
			return [];
		}
	}

	public async Task<IEnumerable<WorkingBuyOrder>> GetOrderedWorkingBuyOrdersAsync()
	{
		List<ExchangeOrderEntity> orders = await _dataContext.ExchangeOrderEntities
			.AsNoTracking()
			.Include( e => e.Exchange )
			.Where( e => e.Type == _TYPENAME_ASK ).ToListAsync();

		IEnumerable<WorkingBuyOrder> result = orders.Select( o => o.MapToWorkingBuyOrder() )
			.OrderBy( o => o.OriginalOrder.Price );

		return result;
	}

	public async Task<IEnumerable<WorkingSellOrder>> GetOrderedWorkingSellOrdersAsync()
	{
		List<ExchangeOrderEntity> orders = await _dataContext.ExchangeOrderEntities
			.AsNoTracking()
			.Include( e => e.Exchange )
			.Where( e => e.Type == _TYPENAME_BID ).ToListAsync();

		IEnumerable<WorkingSellOrder> result = orders.Select( o => o.MapToWorkingSellOrder() )
			.OrderByDescending( o => o.OriginalOrder.Price );

		return result;
	}

	public async Task UpdateAvailableCryptoAsync( Dictionary<Guid, decimal> cryptoPerExchangeId )
	{
		foreach ( var (exchangeId, cryptoUsed) in cryptoPerExchangeId )
		{
			var exchange = await _dataContext.ExchangeEntities.FirstOrDefaultAsync( e => e.Id == exchangeId );
			if ( exchange != null )
			{
				exchange.AvailableCrypto -= cryptoUsed;
			}
		}

		await _dataContext.SaveChangesAsync();
	}

	public async Task UpdateAvailableFundsAsync( Dictionary<Guid, (decimal CryptoGained, decimal EuroSpent)> exchangeUpdates )
	{
		foreach ( var (exchangeId, updates) in exchangeUpdates )
		{
			ExchangeEntity? exchange = await _dataContext.ExchangeEntities.FirstOrDefaultAsync( e => e.Id == exchangeId );
			if (exchange == null)
			{
				continue;
			}

			exchange.AvailableCrypto += updates.CryptoGained;
			exchange.AvailableEuro -= updates.EuroSpent;
		}
	}

	public async Task UpdateFulfilledWorkOrdersAsync( IEnumerable<CoinExchangeOrder> orders )
	{
		try
		{
			foreach ( CoinExchangeOrder order in orders )
			{
				ExchangeOrderEntity? currentOrderEntity = await _dataContext.ExchangeOrderEntities.FirstOrDefaultAsync( o => o.Id == order.Id );
				if ( currentOrderEntity == null )
				{
					continue;
				}

				if ( order.Amount <= 0m )
				{
					_dataContext.ExchangeOrderEntities.Remove( currentOrderEntity );
				}
				else
				{
					currentOrderEntity.Amount = order.Amount;
				}
			}
			await _dataContext.SaveChangesAsync();
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "An error occured while updating the orders of the exchanges in {MethodName}", nameof( UpdateFulfilledWorkOrdersAsync ) );
		}
	}

	public async Task<bool> UpdateExchangeAsync( CoinExchange exchange )
	{
		try
		{
			ExchangeEntity? currentExchange = await _dataContext.ExchangeEntities.FirstOrDefaultAsync();

			if ( currentExchange == null )
			{
				_logger.LogError( "The exchange {ExchangeName} could not be found in the database", exchange.Name );
				return false;
			}

			currentExchange.UpdateEntity( exchange );

			await _dataContext.SaveChangesAsync();

			return true;
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "Error while updating Exchange in {MethodName}", nameof( UpdateExchangeAsync ) );
			return false;
		}
	}

	public async Task<bool> ClearDatabaseAsync()
	{
		try
		{
			List<ExchangeOrderEntity> allOrders = await _dataContext.ExchangeOrderEntities.ToListAsync();
			_dataContext.ExchangeOrderEntities.RemoveRange( allOrders );
			await _dataContext.SaveChangesAsync();

			List<ExchangeEntity> allExchanges = await _dataContext.ExchangeEntities.ToListAsync();
			_dataContext.ExchangeEntities.RemoveRange( allExchanges );
			await _dataContext.SaveChangesAsync();

			return true;
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "Error while clearing database in {MethodName}", nameof( ClearDatabaseAsync ) );
			return false;
		}
	}
}
