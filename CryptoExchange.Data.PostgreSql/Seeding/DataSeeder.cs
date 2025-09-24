using System.Text.Json;
using CryptoExchange.Common.Extensions;
using CryptoExchange.Contracts.DataObjects;
using CryptoExchange.Contracts.JsonObjects;
using CryptoExchange.Contracts.Results;
using CryptoExchange.Contracts.Seeding;
using CryptoExchange.Data.PostgreSql.DataAccess.Context;
using CryptoExchange.Data.PostgreSql.Extensions;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Data.PostgreSql.Seeding;

public class DataSeeder : IDataSeeder
{
	private readonly CryptoDataContext _dataContext;
	private readonly ILogger<DataSeeder> _logger;

	public DataSeeder( CryptoDataContext dataContext, ILogger<DataSeeder> logger )
	{
		_dataContext = dataContext;
		_logger = logger;
	}

	/// <summary>
	/// Loads a single json file to seed the data into the database, returns a <see cref="CoinExchange"/> object
	/// or null if the file cannot be loaded.
	/// </summary>
	/// <param name="fileName">The name of the file to load</param>
	/// <returns>A <see cref="CoinExchange"/> object or null if the file cannot be loaded.</returns>
	private CoinExchange? LoadSingleSeedFile( string? fileName )
	{
		if ( fileName == null || !File.Exists( fileName ) )
		{
			_logger.LogWarning( "The file with the path {FileName} does not exist", fileName );
			return null;
		}

		try
		{
			string data = File.ReadAllText( fileName );
			JsonOrderBook? jsonOrderBook = JsonSerializer.Deserialize<JsonOrderBook>( data );
			if ( jsonOrderBook == null )
			{
				return null;
			}

			CoinExchange result = jsonOrderBook.MapToCoinExchange();

			return result;
		}
		catch ( Exception ex )
		{
			_logger.LogError( "Error while loading the exchanges, Message: {0}", ex.Message );

			return null;
		}
	}

	/// <summary>
	/// Loads the data to seed the database
	/// </summary>
	/// <returns>A <see cref="List{CoinExchange}"/> with the objects to insert into the database</returns>
	private List<CoinExchange> LoadSeedingData()
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string seedDataPath = Path.Combine( baseDirectory, "SeedData" );

		if ( !Directory.Exists( seedDataPath ) )
		{
			_logger.LogWarning( "Directory with data to seed into database not found. Directory: {Path}", seedDataPath );
			return [];
		}

		IEnumerable<string> jsonFiles = Directory.EnumerateFiles( seedDataPath, "*.json" );

		List<CoinExchange> result = jsonFiles.Select( LoadSingleSeedFile ).WhereNotNull().ToList();

		return result;
	}

	/// <summary>
	/// Inserts a <see cref="CoinExchange"/> object into the database
	/// </summary>
	/// <param name="exchange">The <see cref="CoinExchange"/> instance to insert into the database</param>
	private void InsertCoinExchange( CoinExchange exchange )
	{
		try
		{
			var entity = exchange.MapToEntity();

			var orders = exchange.Bids.Select( b => b.MapToEntity( exchange.Id ) ).ToList();

			orders.AddRange( exchange.Asks.Select( b => b.MapToEntity( exchange.Id ) ) );

			orders.ForEach( o => entity.ExchangeOrders.Add( o ) );

			_dataContext.ExchangeEntities.Add( entity );

			_dataContext.SaveChanges();
		}
		catch ( Exception ex )
		{
			_logger.LogError( ex, "Error while seeding data in {Method}", nameof( InsertCoinExchange ) );
		}
	}

	/// <inheritdoc />
	public SeedResult SeedDatabase()
	{
		if ( _dataContext.ExchangeEntities.Any() )
		{
			_logger.LogInformation( "data already exists in database, no seeding" );
			return SeedResult.DataAlreadyExists;
		}

		var exchanges = LoadSeedingData();

		if ( !exchanges.Any() )
		{
			_logger.LogError( "Loading of seeding data failed, no seeding." );
			return SeedResult.SeedDataNotFound;
		}

		foreach ( var exchange in exchanges )
		{
			InsertCoinExchange( exchange );
		}

		return SeedResult.DataSeeded;
	}
}
