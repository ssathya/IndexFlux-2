using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataProvider.BusLogic;
using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ServeData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockQuoteController : ControllerBase
    {
		private readonly ILogger<StockQuoteController> _log;
		private readonly ObtainStockQuote obtainStockQuote;

		public StockQuoteController(ILogger<StockQuoteController> log, ObtainStockQuote obtainStockQuote)
		{
			this._log = log;
			this.obtainStockQuote = obtainStockQuote;
		}
		public async Task<IActionResult> Post([FromBody] GoogleCloudDialogflowV2WebhookRequest intent)
		{
			WebhookResponse returnValue = await obtainStockQuote.GetMarketData(intent);
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
	}
}
