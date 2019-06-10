using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
    public class ObtainStockQuote
    {
		private const string baseUrl = @"https://api.iextrading.com/1.0/stock/symbol/batch?types=quote";
		private readonly ILogger<ObtainStockQuote> _log;
		private readonly DownloadCompanyNames _downloadCompanyNames;
		private readonly EnvHandler _envHandler;
		private string companyName = "CompanyName";

		public ObtainStockQuote(ILogger<ObtainStockQuote> log, DownloadCompanyNames downloadCompanyNames, EnvHandler envHandler)
		{
			_log = log;
			_downloadCompanyNames = downloadCompanyNames;
			_envHandler = envHandler;
		}
		public async Task<WebhookResponse> GetMarketData(GoogleCloudDialogflowV2WebhookRequest stockQuoteParameter)
		{
			_log.LogTrace("Starting to obtain quotes");
			var companyNameToResolve = stockQuoteParameter.QueryResult.Parameters[companyName].ToString();
			var tickersToUse = await _downloadCompanyNames.ResolveCompanyNameOrTicker(companyNameToResolve);
			if (string.IsNullOrWhiteSpace(tickersToUse))
			{
				return new WebhookResponse
				{
					FulfillmentText = $"Could not resolve {companyNameToResolve}"
				};				
			}
			var quotes = await _envHandler.ObtainFromWorldTrading(tickersToUse);
			string returnValueMsg = BuildOutputMsg(quotes);
			var returnValue = new WebhookResponse
			{
				FulfillmentText = returnValueMsg
			};
			return returnValue;

		}

		private string BuildOutputMsg(QuotesFromWorldTrading quotes)
		{
			if (quotes.Data.Length == 0)
			{
				return "Could not resolve requested firm or ticker";
			}
			var tmpStr = new StringBuilder();
			var dateToUse = quotes.Data[0].Last_trade_time;
			tmpStr.Append($"As of {dateToUse.ToString("MMMM dd, hh:mm tt")} EST ");
			foreach (var quote in quotes.Data)
			{
				tmpStr.Append($"{quote.Name} with ticker {quote.Symbol} was traded at {Math.Round(quote.Price,2)}.");
				tmpStr.Append(quote.Day_change > 0 ? " Up by " : "Down by ");
				var changeNumber = string.Format("{0:0.00}", Math.Abs(quote.Day_change));
				tmpStr.Append($"{changeNumber} points.\n\n ");
			}
			return tmpStr.ToString();
		}
	}
}
