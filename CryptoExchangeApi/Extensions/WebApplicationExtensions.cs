using CryptoExchange.Contracts.Results;
using CryptoExchange.Contracts.Seeding;
using CryptoExchange.Data.PostgreSql.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace CryptoExchangeApi.Extensions;

public static class WebApplicationExtensions
{
	public static IApplicationBuilder ApplyMigrations( this IApplicationBuilder app )
	{
		using IServiceScope scope = ( (WebApplication)app ).Services.CreateScope();

		CryptoDataContext dbContext = scope.ServiceProvider.GetRequiredService<CryptoDataContext>();
		ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<CryptoDataContext>>();

		// Check and apply pending migrations
		IEnumerable<string> pendingMigrations = dbContext.Database.GetPendingMigrations();
		if ( pendingMigrations.Any() )
		{
			logger.LogInformation( "{Classname}.{Methodname}: Pending migrations found, applying migrations ...",
				nameof( WebApplicationExtensions ),
				nameof( ApplyMigrations ) );

			dbContext.Database.Migrate();

			logger.LogInformation( "{Classname}.{Methodname}: Migrations applied successfully.",
				nameof( WebApplicationExtensions ),
				nameof( ApplyMigrations ) );
		}
		else
		{
			logger.LogInformation( "{Classname}.{Methodname}: No pending migrations found in the database.",
				nameof( WebApplicationExtensions ),
				nameof( ApplyMigrations ) );
		}

		return app;
	}

	public static IApplicationBuilder SeedDatabase( this IApplicationBuilder app )
	{
		using IServiceScope scope = ( (WebApplication)app ).Services.CreateScope();

		IDataSeeder seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
		ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<IDataSeeder>>();

		logger.LogInformation( "Seeding data if needed ..." );

		SeedResult result = seeder.SeedDatabase();

		switch ( result )
		{
			case SeedResult.DataAlreadyExists:
				logger.LogInformation( "The database entries already exist, no seeding necessary" );
				break;
			case SeedResult.DataSeeded:
				logger.LogInformation( "The database was seeded with the base data" );
				break;
			case SeedResult.SeedDataNotFound:
				logger.LogInformation( "The data to seed the database was not found" );
				break;
			default:
				logger.LogInformation( "The seeding returned no result, please check the log above for errors" );
				break;
		}

		return app;

	}

	public static IApplicationBuilder ConfigurePipeline( this WebApplication app, IConfiguration configuration, IWebHostEnvironment environment )
	{
		if ( environment.IsDevelopment() )
		{
			app.MapOpenApi();
		}

		app.ApplyMigrations();

		app.SeedDatabase();

		app.UseHttpsRedirection();

		app.UseAuthorization();

		app.MapEndpoints();

		return app;
	}
}
