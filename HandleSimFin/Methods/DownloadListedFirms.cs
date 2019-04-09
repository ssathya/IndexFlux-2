using HandleSimFin.Utils;
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
    public class DownloadListedFirms
    {
		private readonly ILogger<DownloadListedFirms> _logger;
		private readonly string _allEntities = @"https://simfin.com/api/v1/info/all-entities?api-key={API-KEY}";
		public DownloadListedFirms(ILogger<DownloadListedFirms> log)
		{
			_logger = log;
		}
		public async Task<List<CompanyDetail>> GetCompanyList()
		{
			string apiKey = HandleSimFinUtils.GetApiKey(_logger);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Did not find API key; calls will fail");
				return null;
			}
			var urlToUse = _allEntities.Replace(@"{API-KEY}", apiKey);
			string data = "";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				var allCompanyDetails = JsonConvert.DeserializeObject<IEnumerable<CompanyDetail>>(data);
				return allCompanyDetails.ToList();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in DownloadListedFirms::GetCompanyList\n{ex.Message}");
				if (ex.InnerException!=null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
				return null;
			}
		}
    }
}
