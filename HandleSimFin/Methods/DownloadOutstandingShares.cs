using HandleSimFin.Utils;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
	public class DownloadOutstandingShares : IDownloadOutstandingShares
	{
		private readonly ILogger<DownloadOutstandingShares> _logger;
		private const string UrlTemplate = @"https://simfin.com/api/v1/companies/id/{companyId}/shares/aggregated?api-key={API-KEY}";

		public DownloadOutstandingShares(ILogger<DownloadOutstandingShares> logger)
		{
			_logger = logger;
		}

		public async Task<OutstandingShares> ObtainAggregatedList(string simId)
		{
			var outstandingShares = new OutstandingShares
			{
				SimId = simId
			};
			string apiKey = HandleSimFinUtils.GetApiKey(_logger);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Did not find API key; calls will fail");
				return null;
			}
			_logger.LogInformation($"Starting getting Outstanding share count for {simId}");
			await HandleGettingData(simId, outstandingShares, apiKey);
			_logger.LogInformation($"Completed download of Outstanding shares for {simId}");
			return outstandingShares;
		}

		private async Task HandleGettingData(string simId, OutstandingShares outstandingShares, string apiKey)
		{
			try
			{
				using (var wc = new WebClient())
				{
					var urlToUse = UrlTemplate.Replace(@"{API-KEY}", apiKey)
						.Replace(@"{companyId}", simId);
					string data = "";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					var fullOutstandingList = JsonConvert.DeserializeObject<IEnumerable<OutstandingValue>>(data);
					var avgList = fullOutstandingList.Where(f => f.Figure.Equals("common-outstanding-diluted"));
					avgList = avgList.Select(al => new OutstandingValue
					{
						Figure = al.Figure,
						Type = al.Type,
						Measure = al.Measure,
						Date = null,
						Period = al.Period,
						Fyear = al.Fyear,
						Value = al.Value
					});

					outstandingShares.OutstandingValues = fullOutstandingList
						.Where(os => (os.Period == "FY" || os.Period == "TTM") && os.Figure.Equals("common-outstanding-diluted"))
						.OrderByDescending(os => os.Fyear)
						.Take(10)
						.ToList();
				}
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Exception in DownloadOutstandingShares:ObtainAggregatedList\n{ex.Message}");
				if (ex.InnerException != null)
				{
					_logger.LogCritical(ex.InnerException.Message);
				}
			}
		}
	}
}