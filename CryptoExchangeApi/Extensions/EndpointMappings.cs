using CryptoExchange.Contracts.Models;
using CryptoExchange.Contracts.Seeding;
using CryptoExchange.Contracts.Services;

namespace CryptoExchangeApi.Extensions;

public static class EndpointMappings
{
	public static void MapEndpoints( this WebApplication app )
	{
		app.MapPost( "/api/bitcoin/sell", async ( IBitcoinOrderService bitcoinService, SellRequest request ) =>
			{
				if ( request.Amount <= 0 )
				{
					return Results.BadRequest( new { error = "Amount must be greater than 0" } );
				}

				SellResult result = await bitcoinService.SellAsync( request.Amount );

				if ( result.IsSuccessful )
				{
					return Results.Ok( result );
				}

				return Results.BadRequest( result );
			} )
			.WithName( "SellBitcoin" )
			.WithSummary( "Sell Bitcoin" )
			.WithDescription( "Executes optimal sell orders across exchanges to sell the specified amount of Bitcoin" )
			.WithOpenApi();

		app.MapPost( "/api/bitcoin/buy", async ( IBitcoinOrderService bitcoinService, BuyRequest request ) =>
			{
				if ( request.Amount <= 0 )
				{
					return Results.BadRequest( new { error = "Amount must be greater than 0" } );
				}

				BuyResult result = await bitcoinService.BuyAsync( request.Amount );

				if ( result.IsSuccessful )
				{
					return Results.Ok( result );
				}

				return Results.BadRequest( result );
			} )
			.WithName( "BuyBitcoin" )
			.WithSummary( "Buy Bitcoin" )
			.WithDescription( "Executes optimal buy orders across exchanges to purchase the specified amount of Bitcoin" )
			.WithOpenApi();

		app.MapPost( "/api/reset", async ( IResetDatabaseService resetDatabaseService ) =>
		{
			ResetResult result = await resetDatabaseService.ResetDatabaseAsync();

			return Results.Ok( result );
		} )
			.WithName( "ResetDatabase" )
			.WithSummary( "Reset the database" )
			.WithDescription( "Reset the database by clearing all entries and seeding it with the original data." )
			.WithOpenApi();
	}
}
