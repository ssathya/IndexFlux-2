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
    public class DownloadCompanyNames
    {
		private readonly ILogger<DownloadCompanyNames> _log;
		private readonly string tickerSearch = @"https://simfin.com/api/v1/info/find-id/ticker/{ticker}?api-key={api-key}";
		private readonly string nameSearch = @"https://simfin.com/api/v1/info/find-id/name-search/{name}?api-key={api-key}";
		private readonly string iexSymbolListURL = @"https://api.iextrading.com/1.0/ref-data/symbols";
		private static List<SecuritySymbol> symbols;
		private static DateTime lastSymbolUpdate;

		public DownloadCompanyNames(ILogger<DownloadCompanyNames> log)
		{
			_log = log;			
			if (symbols == null)
			{
				symbols = new List<SecuritySymbol>();
				lastSymbolUpdate = DateTime.Now.AddDays(-2);
			}
		}
		public async Task<string> ResolveCompanyNameOrTicker(string companyName)
		{
			string apiKey = HandleSimFinUtils.GetApiKey(_log);
			var tSearch = tickerSearch.Replace(@"{api-key}", apiKey);
			var nSearch = nameSearch.Replace(@"{api-key}", apiKey);
			tSearch = tSearch.Replace(@"{ticker}", companyName);
			nSearch = nSearch.Replace(@"{name}", companyName);
			var bcdL = new List<BasicCompanyDetails>();
			var tick = new List<string>();
			if (companyName.Length <=4)
			{
				tick.Add(companyName.ToLower());
			}
			else
			{
				if (symbols.Count <= 5 || (DateTime.Now - lastSymbolUpdate).TotalDays > 1)
				{
					symbols = await ObtainSymbolsFromIeXAsync();
					lastSymbolUpdate = DateTime.Now;
				}
				
				tick = (from s in symbols
						where s.name.ToLower().Contains(companyName.ToLower())
						select s.symbol).Take(4).ToList();
			}
			
			//var resolvedList1 = await ObtainValuesFromSimFin(tSearch);						
			//if (companyName.Length >= 4)
			//{
			//	var resolvedList2 = await ObtainValuesFromSimFin(nSearch);
			//	bcdL.AddRange(resolvedList2.Take(5));
			//}
			//bcdL.AddRange(resolvedList1);
			//var tickers = bcdL.Select(x => x.ticker).Distinct();
			//if (tickers.Count() == 0)
			//{
			//	return "";
			//}
			string returnString = tick.Aggregate((i, j) => i + "," + j);
			return returnString;

		}

		private async Task<List<SecuritySymbol>> ObtainSymbolsFromIeXAsync()
		{
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(iexSymbolListURL);
					var symbols = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data).ToList();
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

		private async Task<List<BasicCompanyDetails>> ObtainValuesFromSimFin(string tSearch)
		{
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(tSearch);
					var stockRTD = JsonConvert.DeserializeObject <IEnumerable<BasicCompanyDetails>>(data);
					return stockRTD.ToList();
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
				return new List<BasicCompanyDetails>();
			}
		}
	}
}
