using CryptoExchange.Contracts.Repositories;
using CryptoExchange.Contracts.Seeding;
using CryptoExchange.Contracts.Services;
using CryptoExchange.Core.Services;
using CryptoExchange.Data.PostgreSql.DataAccess.Context;
using CryptoExchange.Data.PostgreSql.Repositories;
using CryptoExchange.Data.PostgreSql.Seeding;
using Microsoft.EntityFrameworkCore;

namespace CryptoExchangeApi.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDatabase( this IServiceCollection services, IConfiguration configuration )
	{
		string? connectionString = configuration.GetConnectionString( "cryptoexchange" );
		if ( connectionString == null )
		{
			throw new InvalidOperationException( "The connectionstring for the database is not present" );
		}

		services.AddDbContext<CryptoDataContext>( options => options.UseNpgsql( connectionString ) );

		return services;
	}

	public static IServiceCollection ConfigureServices( this IServiceCollection services, IConfiguration configuration )
	{
		// Add services to the container.
		services
			.AddAuthorization()
			.AddDatabase( configuration )
			.AddTransient<IDataSeeder, DataSeeder>()
			.AddScoped<ICryptoExchangeRepository, CryptoExchangeRepository>()
			.AddScoped<IResetDatabaseService, ResetDatabaseService>()
			.AddScoped<IBitcoinOrderService, BitcoinOrderService>()
			.AddOpenApi();

		return services;
	}
}
