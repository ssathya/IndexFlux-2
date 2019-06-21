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
	public class NewsFetchController : ControllerBase
	{
		private readonly ObtainNews _obtainNews;
		private readonly ILogger<NewsFetchController> _log;

		public NewsFetchController(ObtainNews obtainNews, ILogger<NewsFetchController> log)
		{
			this._obtainNews = obtainNews;
			this._log = log;
		}

		// POST: api/NewsFetch
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] GoogleCloudDialogflowV2WebhookRequest intent)
		{
			WebhookResponse returnValue = await _obtainNews.GetExternalNews(intent);
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