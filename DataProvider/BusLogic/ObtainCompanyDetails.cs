using DataProvider.Extensions;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
	public class ObtainCompanyDetails
	{
		private const string iexTradingProvider = "IEXTrading";
		private const string apiKey = "{api-key}";
		private readonly ILogger<ObtainCompanyDetails> _log;
		private readonly EnvHandler _envHandler;
		private static List<SecuritySymbol> symbols;
		private static DateTime lastSymbolUpdate;
		private readonly string iexSymbolListURL = @"https://cloud.iexapis.com/stable/ref-data/symbols?token={api-key}";
		private readonly string iexCompanyDetailsURL = @"https://cloud.iexapis.com/stable/stock/{ticker}/company?token={api-key}";

		public ObtainCompanyDetails(ILogger<ObtainCompanyDetails> log, EnvHandler envHandler)
		{
			_log = log;
			_envHandler = envHandler;
		}
		public async Task<string> ResolveCompanyNameOrTicker(string companyName)
		{
			symbols = await ObtainSymbolsFromIeXAsync();
			_log.LogTrace($"Trying to resolve {companyName}");
			if (symbols == null || symbols.Count <= 5)
			{
				return "";
			}
			var tick = new List<string>();
			
			var localCompanyName = companyName.ToLower();			
			var tickerSearch = (from s in symbols
							where s.Symbol.ToLower().Equals(localCompanyName)
							select s.Symbol).FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(tickerSearch))
			{
				tick.Add(tickerSearch);
			}
			else
			{
				tick.AddRange((from s in symbols
							   where s.Name.ToLower().Contains(companyName.ToLower())
							   select s.Symbol).ToList());
				tick = tick.Distinct().Take(5).ToList();
			}
			string returnString = tick.Aggregate((i, j) => i + "," + j);
			_log.LogTrace($"Company name {companyName} was resolved as {returnString}");
			return returnString;
		}
		private async Task<List<SecuritySymbol>> ObtainSymbolsFromIeXAsync()
		{
			if (symbols != null && symbols.Count >= 5 && (DateTime.Now - lastSymbolUpdate).TotalDays < 1)
			{
				return symbols;
			}
			var urlToUse = iexSymbolListURL.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					symbols = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data).ToList();
					lastSymbolUpdate = DateTime.Now;
					return symbols;
				}
			}
			catch (Exception ex)
			{
				_log.LogError("Error while getting data from IEX trading");
				_log.LogError(ex.Message);
				return new List<SecuritySymbol>();
			}
		}
		public async Task<CompanyOverview> ObtainCompanyOverview(string ticker)
		{
			var urlToUse = iexCompanyDetailsURL.Replace("{ticker}", ticker)
				.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					var companyOverview = JsonConvert.DeserializeObject<CompanyOverview>(data);
					return companyOverview;
				}
			}
			catch (Exception ex)
			{
				_log.LogError("Error while getting data from IEX Trading");
				_log.LogError(ex.Message);
				if (ex.InnerException != null)
				{
					_log.LogError(ex.InnerException.Message);
				}
				return new CompanyOverview();
			}
		}
	}

}

