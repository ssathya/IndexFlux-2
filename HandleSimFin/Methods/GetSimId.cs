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

		public async Task<string> GetSimIdByTicker(string ticker)
		{
			string apiKey = GetApiKey();
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Did not find API key; calls will fail");
				return "";
			}
			try
			{
				var urlToUse = urlForTickerToId.Replace(@"{tickerStr}", ticker.Trim());
				urlToUse = urlToUse.Replace(@"{API-KEY}", apiKey);
				string data = "{}";
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				//NameTickerSimId
				var allDetails = JsonConvert.DeserializeObject<IEnumerable<CompanyDetail>>(data);
				var firstDetail = allDetails.FirstOrDefault();
				if (firstDetail != null)
				{
					return firstDetail.SimId;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in GetSimId::GetSimIdByTicker;Details\n{ex.Message}");
				if (ex.InnerException != null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
			}
			return "";
		}

		#endregion Public Methods


		#region Private Methods

		private string GetApiKey()
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

		#endregion Private Methods
	}
}