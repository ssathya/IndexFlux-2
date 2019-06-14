using DataProvider.BusLogic;
using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServeData.MessageProcessors
{
	public class ProcessMessages
	{

		#region Private Fields

		private readonly ILogger<ProcessMessages> _log;
		private readonly ObtainNews _obtainNews;
		private readonly ObtainStockQuote _obtainStockQuote;		
		private readonly ObtainTrenders _obtainTrends;
		private readonly ObtainMarketSummary _oms;

		#endregion Private Fields


		#region Public Constructors

		public ProcessMessages(ILogger<ProcessMessages> log,
			ObtainMarketSummary oms,
			ObtainTrenders obtainTrends,
			ObtainNews obtainNews,
			ObtainStockQuote obtainStockQuote)
		{
			_log = log;
			_oms = oms;
			_obtainTrends = obtainTrends;
			_obtainNews = obtainNews;
			_obtainStockQuote = obtainStockQuote;
			
		}

		#endregion Public Constructors


		#region Internal Methods

		internal async Task<IActionResult> ProcessValuesFromIntents(GoogleCloudDialogflowV2WebhookRequest value)
		{
			_log.LogTrace("Starting to process request");

			var response = await ProcessIntent(value);

			var responseString = response.ToString();
			_log.LogTrace("Completed processing request");
			return new ContentResult
			{
				Content = responseString,
				ContentType = "application/json",
				StatusCode = 200
			};
		}

		#endregion Internal Methods


		#region Private Methods

		private async Task<WebhookResponse> ProcessIntent(GoogleCloudDialogflowV2WebhookRequest intent)
		{
			string intendDisplayName = intent.QueryResult.Intent.DisplayName;
			WebhookResponse returnValue = null;
			switch (intendDisplayName)
			{
				case "marketSummary":
					returnValue = await _oms.GetIndicesValuesAsync();
					break;

				case "marketTrends":
					returnValue = await _obtainTrends.GetTrendingAsync(intent);
					break;
				case "newsFetch":
					returnValue = await _obtainNews.GetExternalNews(intent);
					break;
				case "stockQuote":
					returnValue = await _obtainStockQuote.GetMarketData(intent);
					break;
				case "fundamentals":
					break;
				default:
					break;
			}
			if (returnValue == null)
			{
				returnValue = new WebhookResponse
				{
					FulfillmentText = Utilities.ErrorReturnMsg() + Utilities.EndOfCurrentRequest()
				};
			}
			else if (returnValue.FulfillmentMessages.Count == 0 &&
				!returnValue.FulfillmentText.Contains(Utilities.EndOfCurrentRequest()))
			{
				returnValue.FulfillmentText = returnValue.FulfillmentText + "\n" + Utilities.EndOfCurrentRequest();
			}
			return returnValue;
		}

		#endregion Private Methods
	}
}