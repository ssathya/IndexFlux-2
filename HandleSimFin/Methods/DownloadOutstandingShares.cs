﻿using HandleSimFin.Utils;
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
    public class DownloadOutstandingShares
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
			try
			{
				using (var wc = new WebClient())
				{
					var urlToUse = UrlTemplate.Replace(@"{API-KEY}", apiKey)
						.Replace(@"{companyId}", simId);
					string data = "";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					var fullOutstandingList = JsonConvert.DeserializeObject<IEnumerable<OutstandingValue>>(data);
					outstandingShares.OutstandingValues = fullOutstandingList
						.Where(os => os.Period == "FY" && os.Figure.Equals("common-outstanding-diluted"))
						.OrderByDescending(os => os.Fyear)
						.Take(4)
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
			return outstandingShares;
		}

    }
}
