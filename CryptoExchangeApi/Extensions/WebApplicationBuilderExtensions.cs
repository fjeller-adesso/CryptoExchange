using CryptoExchange.Net9.ServiceDefaults;

namespace CryptoExchangeApi.Extensions;

public static class WebApplicationBuilderExtensions
{
	/// <summary>
	/// Configures services for the application using the specified <see cref="WebApplicationBuilder"/>.
	/// </summary>
	/// <remarks>This method extends the <see cref="WebApplicationBuilder"/> to configure services by delegating  to
	/// the application's service configuration logic. It is typically called during application  initialization to
	/// register services with the dependency injection container.</remarks>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> used to configure the application's services.</param>
	/// <returns>The same <see cref="WebApplicationBuilder"/> instance, allowing for method chaining.</returns>
	private static WebApplicationBuilder ConfigureServices( this WebApplicationBuilder builder )
	{
		// Add services to the container.
		builder.Services.ConfigureServices( builder.Configuration );

		return builder;
	}

	/// <summary>
	/// Configures the HTTP request pipeline for the specified <see cref="WebApplication"/> instance.
	/// </summary>
	/// <param name="app">The <see cref="WebApplication"/> to configure.</param>
	/// <returns>The configured <see cref="WebApplication"/> instance.</returns>
	private static WebApplication ConfigurePipeline( this WebApplication app )
	{
		// Configure the HTTP request pipeline.
		app.ConfigurePipeline( app.Configuration, app.Environment );

		return app;
	}

	public static void Start( this WebApplicationBuilder builder )
	{
		builder
			.AddServiceDefaults()
			.ConfigureServices()
			.Build()
			.ConfigurePipeline()
			.Run();
	}
}
