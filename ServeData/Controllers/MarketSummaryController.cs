using DataProvider.BusLogic;
using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServeData.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MarketSummaryController : ControllerBase
	{
		private readonly ILogger<MarketSummaryController> _log;
		private readonly ObtainMarketSummary _obtainMarketSummary;

		public MarketSummaryController(ILogger<MarketSummaryController> log, ObtainMarketSummary obtainMarketSummary)
		{
			_log = log;
			_obtainMarketSummary = obtainMarketSummary;
		}

		// POST: api/MarketSummary
		[HttpPost]
		public async Task<IActionResult> PostAsync(GoogleCloudDialogflowV2WebhookRequest value)
		{
			WebhookResponse returnValue = null;
			returnValue = await _obtainMarketSummary.GetIndicesValuesAsync();
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
			var responseString = returnValue.ToString();
			_log.LogTrace("Completed processing request");
			return new ContentResult
			{
				Content = responseString,
				ContentType = "application/json",
				StatusCode = 200
			};
		}

		// PUT: api/MarketSummary/5
	}
}