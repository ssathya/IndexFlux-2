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
	public class DownloadStockQuote : IDownloadStockQuote
	{
		private const string baseUrl = @"https://api.iextrading.com/1.0/stock/symbol/batch?types=quote";
		private readonly ILogger<DownloadStockQuote> _logger;

		public DownloadStockQuote(ILogger<DownloadStockQuote> logger)
		{
			_logger = logger;
		}
		public async Task<MarketQuote> DownloadQuote(string ticker)
		{
			var urlToUse = baseUrl.Replace("symbol", ticker);
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					var stockRTD = JsonConvert.DeserializeObject<MarketQuote>(data);
					return stockRTD;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while getting data from IEX Trading");
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
