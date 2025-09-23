# CryptoExchange

Testapplication mimicking a Crypto Exchange with sample data

## Requirements
- Docker for Windows
- Visual Studio 2022, current version
- .NET 9

## Running the application
The application was built using .NET Aspire, which is very helpful since we can easily add a database (or other toolings) for local development. 
Therefore, locally, the application can simply be started in Visual Studio and it will
- Create a PostgreSQL-instance as a container in Docker for Windows. The container keeps running after the application is stopped.
- Creates a database inside the container
- When running, also creates an instance of PGAdmin which can be used to access the database (This instance is not persistent and will be recreated for each run)
- Seed the initial data into the database
- Gives the developer a dashboard with vital information, e.g. logs, traces, etc. 

The startup-application, if not already set accordingly, must be `CryptoExchange.Net9.AppHost`. That is the aspire-orchestrator that gives us the dashboard
and all the nice tools.

As of right now, only a web api is created and working. The console application is still in the works, but will use the same logic.

### Testing the application
Testing can be done in visual studio using the file `CryptoExchangeApi.http` in the root of the Project `CryptoExchangeApi`. 
When a request is executed, the best possible result is selected and the persisted data (Exchanges/Orders) are updated with the new
values. If an order is "used up", meaning that the amount of crypto currency for the order is 0 because everything was bought, that
order will be removed from the database.

To reinitialize the database we will have an endpoint that removes all entries from the database, which will result in a Re-Seed of the 
database at the next application start.


