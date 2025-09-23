using Projects;

namespace CryptoExchange.Net9.AppHost;

internal class Program
{
	private static void Main( string[] args )
	{
		var builder = DistributedApplication.CreateBuilder( args );

		var postgres = builder.AddPostgres( "postgres" )
			.WithLifetime( ContainerLifetime.Persistent )
			.WithPgAdmin();

		var postgresDatabase =	postgres.AddDatabase( "cryptoexchange" );

		var cryptoExchangeApi = builder.AddProject<CryptoExchangeApi>( "cryptoexchangeapi" )
			.WithExternalHttpEndpoints()
			.WithReference( postgresDatabase )
			.WaitFor( postgresDatabase );

		builder.Build().Run();
	}
}