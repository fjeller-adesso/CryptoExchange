using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Contracts.Models;
using CryptoExchange.Contracts.Repositories;
using CryptoExchange.Contracts.Results;
using CryptoExchange.Contracts.Seeding;
using CryptoExchange.Contracts.Services;

namespace CryptoExchange.Core.Services;

public class ResetDatabaseService : IResetDatabaseService
{
	private readonly ICryptoExchangeRepository _cryptoRepository;
	private readonly IDataSeeder _dataSeeder;

	public ResetDatabaseService(ICryptoExchangeRepository cryptoRepository, IDataSeeder dataSeeder)
	{
		_cryptoRepository = cryptoRepository;
		_dataSeeder = dataSeeder;
	}

	public async Task<ResetResult> ResetDatabaseAsync()
	{
		bool databaseCleared = await _cryptoRepository.ClearDatabaseAsync();
		if ( !databaseCleared )
		{
			return ResetResult.Error("The database could not be cleared and was not re-seeded");
		}

		SeedResult seedResult = _dataSeeder.SeedDatabase();

		ResetResult result = seedResult == SeedResult.DataSeeded 
			? ResetResult.Success( "The database was re-seeded" ) 
			: ResetResult.Error( "The database could not be seeded, please check the logs." );

		return result;
	}
}
