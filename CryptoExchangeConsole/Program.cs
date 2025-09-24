using System.Globalization;
using CryptoExchangeConsole.Extensions;
using CryptoExchangeConsole.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoExchangeConsole;

internal class Program
{
	private static async Task<int> Main( string[] args )
	{
		// Build configuration
		var configuration = new ConfigurationBuilder()
			.SetBasePath( Directory.GetCurrentDirectory() )
			.AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
			.Build();

		// Build host with DI container
		IHost host = Host.CreateDefaultBuilder( args )
			.ConfigureServices( ( context, services ) =>
			{
				services.ConfigureServices( configuration );
			} )
			.ConfigureLogging( logging =>
			{
				logging.ClearProviders();
				logging.AddConsole();
				logging.SetMinimumLevel( LogLevel.Warning );
			} )
			.Build();

		int result = await ExecuteCommandsAsync( host, args );

		return result;
	}

	private static async Task<int> ExecuteCommandsAsync( IHost host, string[] args )
	{
		try
		{
			// Parse command line arguments
			if ( args.Length == 0 )
			{
				ShowHelp();
				return 1;
			}

			string command = args[0].ToLowerInvariant();

			using IServiceScope scope = host.Services.CreateScope();
			IConsoleService consoleService = scope.ServiceProvider.GetRequiredService<IConsoleService>();

			switch ( command )
			{
				case "buy":
					return await HandleBuyCommand( args, consoleService );

				case "sell":
					return await HandleSellCommand( args, consoleService );

				case "reset":
				case "--reset":
					return await HandleResetCommand( consoleService );

				case "help":
				case "--help":
				case "-help":
				case "-h":
				case "-?":
				case "?":
					ShowHelp();
					return 0;

				default:
					Console.WriteLine( $"Unknown command: {command}" );
					Console.WriteLine();
					ShowHelp();
					return 1;
			}
		}
		catch ( Exception ex )
		{
			Console.WriteLine( $"An unexpected error occurred: {ex.Message}" );
			return 1;
		}
	}

	private static bool TryParseAmount( string[] args, out decimal amount )
	{
		amount = 0;

		for ( int i = 0; i < args.Length - 1; i++ )
		{
			if ( args[i] == "--amount" || args[i] == "-a" )
			{
				return decimal.TryParse( args[i + 1], CultureInfo.InvariantCulture, out amount ) && amount > 0;
			}
		}

		return false;
	}

	private static async Task<int> HandleBuyCommand( string[] args, IConsoleService consoleService )
	{
		if ( !TryParseAmount( args, out decimal amount ) )
		{
			Console.WriteLine( "Invalid or missing amount for buy command." );
			Console.WriteLine( "Usage: dotnet run buy --amount 1.5" );
			return 1;
		}

		await consoleService.ExecuteBuyOrderAsync( amount );
		return 0;
	}

	private static async Task<int> HandleSellCommand( string[] args, IConsoleService consoleService )
	{
		if ( !TryParseAmount( args, out decimal amount ) )
		{
			Console.WriteLine( "Invalid or missing amount for sell command." );
			Console.WriteLine( "Usage: dotnet run sell --amount 0.75" );
			return 1;
		}

		await consoleService.ExecuteSellOrderAsync( amount );
		return 0;
	}

	private static async Task<int> HandleResetCommand( IConsoleService consoleService )
	{
		await consoleService.ResetDatabaseAsync();
		return 0;
	}

	private static void ShowHelp()
	{
		Console.WriteLine( "CryptoExchange Console Application" );
		Console.WriteLine( "==================================" );
		Console.WriteLine();
		Console.WriteLine( "Commands:" );
		Console.WriteLine( "  reset                          Resets the database and seeds it with initial exchange data" );
		Console.WriteLine( "  buy --amount <decimal>         Buy Bitcoin using optimal orders across exchanges" );
		Console.WriteLine( "  sell --amount <decimal>        Sell Bitcoin using optimal orders across exchanges" );
		Console.WriteLine( "  help                           Show this help message" );
		Console.WriteLine();
		Console.WriteLine( "Examples:" );
		Console.WriteLine( "  dotnet run reset" );
		Console.WriteLine( "  dotnet run buy --amount 2.5" );
		Console.WriteLine( "  dotnet run buy -a 2.5" );
		Console.WriteLine( "  dotnet run sell --amount 1.0" );
		Console.WriteLine( "  dotnet run sell -a 1.0" );
		Console.WriteLine();
		Console.WriteLine( "Options:" );
		Console.WriteLine( "  --amount, -a <decimal>         Amount of Bitcoin to buy or sell" );
		Console.WriteLine( "  --help, -h                     Show help information" );
	}
}
