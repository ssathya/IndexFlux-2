using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
		public async Task<QuotesFromWorldTrading> ObtainFromWorldTrading(string tickersToUse)
		{


			var apiKey = GetApiKey("WorldTradingDataKey");

			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogError("Did not find api key; calls will fail");
			}
			string urlStr = $@"https://www.worldtradingdata.com/api/v1/stock?symbol={tickersToUse}&api_token={apiKey}";
			string data = "{}";
			using (var wc = new WebClient())
			{
				data = await wc.DownloadStringTaskAsync(urlStr);
			}
			var indexData = JsonConvert.DeserializeObject<QuotesFromWorldTrading>(data);
			return indexData;
		}
	}
}
