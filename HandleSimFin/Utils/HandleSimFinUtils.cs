using Microsoft.Extensions.Logging;
using System;

namespace HandleSimFin.Utils
{
	internal static class HandleSimFinUtils
	{
		internal static string GetApiKey(ILogger _logger)
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