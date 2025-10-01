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

	#region private methods

	/// <summary>
	/// Updates the available Cryptos for the exchanges. Is used inside a transaction and does not save the changes to the database itself.
	/// </summary>
	/// <param name="cryptoPerExchangeId">a dictionary with exchange ids and the new crypto value for the exchange</param>
	/// <returns>void</returns>
	private async Task UpdateAvailableCryptoAsync( Dictionary<Guid, decimal> cryptoPerExchangeId )
	{
		foreach ( var (exchangeId, cryptoUsed) in cryptoPerExchangeId )
		{
			var exchange = await _dataContext.ExchangeEntities.FirstOrDefaultAsync( e => e.Id == exchangeId );
			if ( exchange != null )
			{
				exchange.AvailableCrypto -= cryptoUsed;
			}
		}
	}

	/// <summary>
	/// Updates the available Funds for the exchanges. Is used inside a transaction and does not save the changes to the database itself.
	/// </summary>
	/// <param name="exchangeUpdates">a dictionary with exchange ids and the new funds for the exchange</param>
	/// <returns>void</returns>
	private async Task UpdateAvailableFundsAsync( Dictionary<Guid, (decimal CryptoGained, decimal EuroSpent)> exchangeUpdates )
	{
		foreach ( var (exchangeId, updates) in exchangeUpdates )
		{
			ExchangeEntity? exchange = await _dataContext.ExchangeEntities.FirstOrDefaultAsync( e => e.Id == exchangeId );
			if ( exchange == null )
			{
				continue;
			}

			exchange.AvailableCrypto += updates.CryptoGained;
			exchange.AvailableEuro -= updates.EuroSpent;
		}
	}

	/// <summary>
	/// Updates the orders that were used in the last request and deletes those who were depleted (have 0 
	/// bitcoin remaining). This is to update the orders correctly for the next run. This method 
	/// is used inside a transaction and does not save the changes to the database itself.
	/// </summary>
	/// <param name="orders">the orders to update or delete</param>
	/// <returns>void</returns>
	private async Task UpdateFulfilledWorkOrdersAsync( IEnumerable<CoinExchangeOrder> orders )
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
	}

	#endregion

	#region public methods

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	public async Task UpdateFundsAndOrdersAsync( Dictionary<Guid, (decimal CryptoGained, decimal EuroSpent)> exchangeUpdates, IEnumerable<CoinExchangeOrder> ordersToUpdate )
	{
		await using var transaction = await _dataContext.Database.BeginTransactionAsync();
		try
		{
			await UpdateAvailableFundsAsync( exchangeUpdates );
			await UpdateFulfilledWorkOrdersAsync( ordersToUpdate );
			await _dataContext.SaveChangesAsync();
			await transaction.CommitAsync();
		}
		catch ( Exception ex )
		{
			await transaction.RollbackAsync();
			_logger.LogError( ex, "An error occured while updating funds and orders in {Methodname}. The transaction was rolled back", nameof( UpdateFundsAndOrdersAsync ) );
		}
	}

	public async Task UpdateCryptoAndOrdersAsync( Dictionary<Guid, decimal> cryptoUpdates, IEnumerable<CoinExchangeOrder> ordersToUpdate )
	{
		await using var transaction = await _dataContext.Database.BeginTransactionAsync();
		try
		{
			await UpdateAvailableCryptoAsync( cryptoUpdates );
			await UpdateFulfilledWorkOrdersAsync( ordersToUpdate );
			await _dataContext.SaveChangesAsync();
			await transaction.CommitAsync();
		}
		catch ( Exception ex )
		{
			await transaction.RollbackAsync();
			_logger.LogError( ex, "An error occured while updating funds and orders in {Methodname}. The transaction was rolled back", nameof( UpdateFundsAndOrdersAsync ) );
		}
	}

	/// <inheritdoc />
	public async Task<bool> UpdateExchangeAsync( CoinExchange exchange )
	{
		try
		{
			ExchangeEntity? currentExchange = await _dataContext.ExchangeEntities.FirstOrDefaultAsync( e => e.Id == exchange.Id );

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

	/// <inheritdoc />
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

	#endregion
}
