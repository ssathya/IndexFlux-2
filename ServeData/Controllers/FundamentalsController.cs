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
	public class FundamentalsController : ControllerBase
	{
		private readonly ILogger<FundamentalsController> _log;
		private readonly ObtainFundamentals _obtainFundamentals;

		public FundamentalsController(ILogger<FundamentalsController> log, ObtainFundamentals obtainFundamentals)
		{
			_log = log;
			_obtainFundamentals = obtainFundamentals;
		}

		// POST: api/Fundamentals
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] GoogleCloudDialogflowV2WebhookRequest intent)
		{
			WebhookResponse returnValue = null;
			returnValue = await _obtainFundamentals.GetCompanyRatings(intent);
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