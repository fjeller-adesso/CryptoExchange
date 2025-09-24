using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Contracts.Models;

namespace CryptoExchange.Contracts.Services;

public interface IResetDatabaseService
{
	Task<ResetResult> ResetDatabaseAsync();
}
