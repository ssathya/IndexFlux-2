using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
	public class DownloadMarketSummary : IDownloadMarketSummary
	{
		private readonly ILogger<DownloadMarketSummary> _logger;
		private const string urlStr = @"https://www.worldtradingdata.com/api/v1/stock?symbol={tickersToUse}&api_token={apiKey}";
		public DownloadMarketSummary(ILogger<DownloadMarketSummary> logger)
		{
			_logger = logger;
		}

		public async Task<QuotesFromWorldTrading> GetIndexValues()
		{
			var indeces = @"^DJI,^INX,^IXIC"; //Dow30, S&P 500, and NASDAQ 100
			var apiKey = Environment.GetEnvironmentVariable("WorldTradingDataKey");
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Could not find API-Key for World Trading Data");
				return null;
			}
			var urlToUse = urlStr.Replace(@"{tickersToUse}", indeces)
				.Replace(@"{apiKey}", apiKey);
			try
			{
				var data = "{}";
				_logger.LogInformation("Starting to get data from World Trading");
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				_logger.LogInformation("Completed downloading data from World Trading");
				var indexData = JsonConvert.DeserializeObject<QuotesFromWorldTrading>(data);
				return indexData;

			}
			catch (Exception ex)
			{
				_logger.LogError("Exception while getting data from World Trading");
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
				return null;
			}
		}
	}
}
