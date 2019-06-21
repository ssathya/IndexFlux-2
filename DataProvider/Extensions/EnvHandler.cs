using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DataProvider.Extensions
{
	public class EnvHandler
	{

		#region Private Fields

		private readonly ILogger<EnvHandler> _log;

		#endregion Private Fields


		#region Public Constructors

		public EnvHandler(ILogger<EnvHandler> log)
		{
			_log = log;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<QuotesFromWorldTrading> ObtainFromWorldTrading(string tickersToUse)
		{
			var apiKey = GetApiKey("WorldTradingDataKey");

			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogError("Did not find api key; calls will fail");
			}
			string urlStr = $@"https://www.worldtradingdata.com/api/v1/stock?symbol={tickersToUse}&api_token={apiKey}";
			string data = "{}";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlStr);
				}
				var parsedData = JObject.Parse(data);
				var indexData = JsonConvert.DeserializeObject<QuotesFromWorldTrading>(data);
				return indexData;
			}
			catch (Exception ex)
			{
				_log.LogCritical($"Error processing data from World Trading.\n{ex.Message}");
				return new QuotesFromWorldTrading
				{
					Data = new Datum[0]
				};
			}
		}

		#endregion Public Methods


		#region Internal Methods

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

		#endregion Internal Methods
	}
}