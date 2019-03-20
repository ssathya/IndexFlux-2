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
	public class GetSimId
	{

		#region Private Fields

		private const string urlForCompanyNameToId = @"https://simfin.com/api/v1/info/find-id/name-search/{companyName}?api-key={API-KEY}";
		private const string urlForTickerToId = @"https://simfin.com/api/v1/info/find-id/ticker/{tickerStr}?api-key={API-KEY}";

		#endregion Private Fields


		#region Public Properties

		public ILogger<GetSimId> _logger { get; }

		#endregion Public Properties


		#region Public Constructors

		public GetSimId(ILogger<GetSimId> log)
		{
			_logger = log;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<string> GetSimIdByCompanyName(string companyName)
		{			
			var urlToUse = urlForCompanyNameToId.Replace(@"{companyName}", companyName.Trim());			
			CompanyDetail firstDetail = await CallSimFinForSimId(urlToUse);
			return firstDetail != null ? firstDetail.SimId : "";
		}

		public async Task<string> GetSimIdByTicker(string ticker)
		{
			
			var urlToUse = urlForTickerToId.Replace(@"{tickerStr}", ticker.Trim());
			CompanyDetail firstDetail = await CallSimFinForSimId(urlToUse);
			return firstDetail != null ? firstDetail.SimId : "";
		}

		#endregion Public Methods


		#region Private Methods

		private async Task<CompanyDetail> CallSimFinForSimId(string urlToUse)
		{
			string apiKey = HandleSimFinUtils<GetSimId>.GetApiKey(_logger);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Did not find API key; calls will fail");
				return null;
			}
			urlToUse = urlToUse.Replace(@"{API-KEY}", apiKey);
			string data = "[]";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				//NameTickerSimId
				var allDetails = JsonConvert.DeserializeObject<IEnumerable<CompanyDetail>>(data);
				return allDetails.FirstOrDefault();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetSimId::GetSimIdByTicker;Details\n{ex.Message}");
				if (ex.InnerException != null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
				return null;
			}
		}

		

		#endregion Private Methods
	}
}