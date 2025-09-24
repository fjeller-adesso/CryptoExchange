# CryptoExchange

Testapplication mimicking a Crypto Exchange with sample data

## Requirements
- Docker for Windows
- Visual Studio 2022, current version
- .NET 9

## Running the application (API)

### locally (for debugging)
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

### Docker
To create a docker image, the application uses a dockerfile which is located in the project folder of the project `CryptoExchangeApi`.
To use the database, you need to change the connection string for the database in the file `appSettings.json` to the correct
connection string pointing to your PostgreSQL-instance/database.

```json
{
    "ConnectionStrings": {
        "cryptoexchange": "<your connection string>"
    },

    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*"
}
```

This is especially important since the appliaction will not run without a database.

The files necessary to seed the database will be included in the docker-container, therefore the database should be properly seeded.

## Testing the application
Testing can be done in visual studio using the file `CryptoExchangeApi.http` in the root of the Project `CryptoExchangeApi`. 
When a request is executed, the best possible result is selected and the persisted data (Exchanges/Orders) are updated with the new
values. If an order is "used up", meaning that the amount of crypto currency for the order is 0 because everything was bought, that
order will be removed from the database.

### Endpoints

`POST /api/bitcoin/sell`: Calls the selling endpoint. Needs an object of type `SellRequest`. The object has only one property with the 
name "Amount", which expects a decimal value. So in the .http-file, the call would look like this:

```
@CryptoExchangeApi_HostAddress = http://localhost:5022

POST {{CryptoExchangeApi_HostAddress}}/api/bitcoin/sell
Content-Type: application/json
{
	"amount": 3.0
}

###
```

`POST /api/bitcoin/buy`: Calls the buying endpoint. Needs an object of type `BuyRequest`. The object has only one property with the 
name "Amount", which expects a decimal value. So in the .http-file, the call would look like this:

```
@CryptoExchangeApi_HostAddress = http://localhost:5022

POST {{CryptoExchangeApi_HostAddress}}/api/bitcoin/buy
Content-Type: application/json
{
	"amount": 3.0
}

###
```

`POST /api/reset`: Clears the database and re-seeds the original values, since those will be changed by the transactions. This call requires
no object, it's just a simple post-call.

```
@CryptoExchangeApi_HostAddress = http://localhost:5022

POST {{CryptoExchangeApi_HostAddress}}/api/reset
Accept: application/json
```

Here is a sample of a result when buying 3 BC with a freshly seeded database:

```json
{
  "executedOrders": [
    {
      "exchangeId": "01997a38-9fc9-7c79-b46d-9d1953d92bb1",
      "exchangeName": "exchange-01",
      "price": 57299.73,
      "time": "2024-03-01T00:46:50.389Z",
      "amountToBuy": 0.405,
      "totalCost": 23206.39065
    },
    {
      "exchangeId": "01997a38-9fca-7ad5-95c6-75d1029fe656",
      "exchangeName": "exchange-02",
      "price": 57299.73,
      "time": "2024-03-01T22:14:09.024Z",
      "amountToBuy": 0.405,
      "totalCost": 23206.39065
    },
    {
      "exchangeId": "01997a38-9fcb-7824-a970-1a9b5fcb8efa",
      "exchangeName": "exchange-03",
      "price": 57299.73,
      "time": "2024-03-01T19:53:01.111Z",
      "amountToBuy": 0.405,
      "totalCost": 23206.39065
    },
    {
      "exchangeId": "01997a38-9fcc-7025-99cc-a35ca376fa05",
      "exchangeName": "exchange-04",
      "price": 57299.73,
      "time": "2024-03-01T06:43:15.857Z",
      "amountToBuy": 0.405,
      "totalCost": 23206.39065
    },
    {
      "exchangeId": "01997a38-9fcd-74fa-ae4c-6ec368ee7c19",
      "exchangeName": "exchange-05",
      "price": 57299.73,
      "time": "2024-03-01T19:51:38.522Z",
      "amountToBuy": 0.405,
      "totalCost": 23206.39065
    },
    {
      "exchangeId": "01997a38-9fce-7122-a263-820844dcc4c8",
      "exchangeName": "exchange-06",
      "price": 57299.73,
      "time": "2024-03-01T02:25:18.75Z",
      "amountToBuy": 0.39731957,
      "totalCost": 22766.3040847161
    },
    {
      "exchangeId": "01997a38-9fcf-77e7-9a9e-73644961846d",
      "exchangeName": "exchange-07",
      "price": 57299.73,
      "time": "2024-03-01T14:24:28.029Z",
      "amountToBuy": 0.39731957,
      "totalCost": 22766.3040847161
    },
    {
      "exchangeId": "01997a38-9fcf-71c8-906a-5fdaf56b51a1",
      "exchangeName": "exchange-08",
      "price": 57299.73,
      "time": "2024-03-01T06:45:23.819Z",
      "amountToBuy": 0.18036086,
      "totalCost": 10334.6285805678
    }
  ],
  "totalBitcoinPurchased": 3.00000000,
  "totalCost": 171899.1900000000,
  "isSuccessful": true,
  "errorMessage": null
}
```

## Console Application
The console application uses the same functionality as the API and allows you to buy/sell bitcoin from the commadn line.
While the data will still be updated in the database, the console application will exit after each call. You can
still reset the database though by using the command line argument `--reset` or `reset`.

### Usage

Run the application from the command line with the following commands. The commands are shown as `dotnet run`-commands, which
allows you to run the application directly from the source code. If you use the final, compiled version of the application,
you will need to replace `dotnet run` with the application name, in this case `cryptoexchangeconsole`.

#### Reset Database
Re-initializes the database with the original exchange data:
```bash
dotnet run reset
```

#### Buy Bitcoin
Purchase Bitcoin using optimal buy orders across exchanges:
```bash
dotnet run buy --amount 1.5
dotnet run buy -a 1.5
```

#### Sell Bitcoin
Sell Bitcoin using optimal sell orders across exchanges:
```bash
dotnet run sell --amount 0.75
dotnet run sell -a 0.75
```

#### Help
Display help information:
```bash
dotnet run help
dotnet run --help
dotnet run -h
```


