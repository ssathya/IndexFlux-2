using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
	public class ObtainStockQuote
	{

		#region Private Fields

		private const string baseUrl = @"https://api.iextrading.com/1.0/stock/symbol/batch?types=quote";
		private readonly DownloadCompanyNames _downloadCompanyNames;
		private readonly EnvHandler _envHandler;
		private readonly ILogger<ObtainStockQuote> _log;
		private string companyName = "CompanyName";

		#endregion Private Fields


		#region Public Constructors

		public ObtainStockQuote(ILogger<ObtainStockQuote> log, DownloadCompanyNames downloadCompanyNames, EnvHandler envHandler)
		{
			_log = log;
			_downloadCompanyNames = downloadCompanyNames;
			_envHandler = envHandler;
		}

		#endregion Public Constructors


		#region Public Methods

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

		#endregion Public Methods


		#region Private Methods

		private string BuildOutputMsg(QuotesFromWorldTrading quotes)
		{
			if (quotes.Data.Length == 0)
			{
				return "Could not resolve requested firm or ticker";
			}
			var tmpStr = new StringBuilder();
			DateTime dateToUse = quotes.Data[0].Last_trade_time != null ? (DateTime)quotes.Data[0].Last_trade_time : DateTime.Parse("01-01-2000");

			tmpStr.Append($"As of {dateToUse.ToString("MMMM dd, hh:mm tt")} EST ");
			foreach (var quote in quotes.Data)
			{
				float price = quote.Price != null ? (float)quote.Price : 0;
				quote.Day_change = quote.Day_change == null ? 0 : quote.Day_change;
				tmpStr.Append($"{quote.Name} with ticker {quote.Symbol} was traded at {price.ToString("N")}.");
				tmpStr.Append(quote.Day_change > 0 ? " Up by " : " Down by ");
				tmpStr.Append($"{((float)quote.Day_change).ToString("N")} points.\n\n ");
			}
			return tmpStr.ToString();
		}

		#endregion Private Methods
	}
}