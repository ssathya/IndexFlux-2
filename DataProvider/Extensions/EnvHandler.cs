using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Extensions
{
    public class EnvHandler
    {
		private readonly ILogger<EnvHandler> _log;

		public EnvHandler(ILogger<EnvHandler> log)
		{
			_log = log;
		}
		internal string GetApiKey(string provider)
		{
			var apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in process");
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Machine);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.User);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable("NewsAPI");
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogError($"Did not find api key for {provider}; calls will fail");
			}
			return apiKey;
		}
	}
}
