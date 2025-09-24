using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Contracts.Models;

public class ResetResult
{
	public bool IsSuccessful { get; set; }

	public required string Message { get; set; }

	public static ResetResult Success(string message)
	{
		ResetResult result = new()
		{
			IsSuccessful = true,
			Message = message
		};

		return result;
	}

	public static ResetResult Error( string message )
	{
		ResetResult result = new()
		{
			IsSuccessful = false,
			Message = message
		};

		return result;
	}
}
