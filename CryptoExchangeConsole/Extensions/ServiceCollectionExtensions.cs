using CryptoExchange.Contracts.Repositories;
using CryptoExchange.Contracts.Seeding;
using CryptoExchange.Contracts.Services;
using CryptoExchange.Core.Services;
using CryptoExchange.Data.PostgreSql.DataAccess.Context;
using CryptoExchange.Data.PostgreSql.Repositories;
using CryptoExchange.Data.PostgreSql.Seeding;
using CryptoExchangeConsole.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoExchangeConsole.Extensions;

internal static class ServiceCollectionExtensions
{
	private static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration )
	{
		string? connectionString = configuration.GetConnectionString( "cryptoexchange" );
		if ( connectionString == null )
		{
			throw new InvalidOperationException( "The connection string for the database is not present" );
		}

		services.AddDbContext<CryptoDataContext>( options => options.UseNpgsql( connectionString ) );

		return services;
	}

	public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration )
	{
		services
			.ConfigureDatabase(configuration)
			.AddTransient<IDataSeeder, DataSeeder>()
			.AddScoped<ICryptoExchangeRepository, CryptoExchangeRepository>()
			.AddScoped<IResetDatabaseService, ResetDatabaseService>()
			.AddScoped<IBitcoinOrderService, BitcoinOrderService>()
			.AddScoped<IConsoleService, ConsoleService>()
			.AddSingleton( configuration );
	}
}
