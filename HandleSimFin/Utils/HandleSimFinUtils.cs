using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandleSimFin.Utils
{
    internal static class HandleSimFinUtils<T> where T:class
    {
		internal static string GetApiKey(ILogger<T> _logger)
		{
			var apiKey = Environment.GetEnvironmentVariable("SimFinKey", EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogDebug("Did not find API key in process");
				apiKey = Environment.GetEnvironmentVariable("SimFinKey", EnvironmentVariableTarget.Machine);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogDebug("Did not find API key in Machine");
				apiKey = Environment.GetEnvironmentVariable("SimFinKey", EnvironmentVariableTarget.User);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogDebug("Did not find API key in Machine");
				apiKey = Environment.GetEnvironmentVariable("SimFinKey");
			}

			return apiKey;
		}
	}
}
