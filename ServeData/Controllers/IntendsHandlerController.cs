using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestSharp;
using ServeData.MessageProcessors;

namespace ServeData.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class IntendsHandlerController : ControllerBase
	{
		#region Private Fields

		private readonly ILogger<IntendsHandlerController> _log;

		#endregion Private Fields

		#region Private Properties

		private ProcessMessages _processMessages { get; }

		#endregion Private Properties

		#region Public Constructors

		public IntendsHandlerController(ILogger<IntendsHandlerController> log, ProcessMessages processMessages)
		{
			_log = log;
			_processMessages = processMessages;
		}

		#endregion Public Constructors

		#region Public Methods

		// POST: api/IntendsHandler
		[HttpPost]
		public IActionResult Post([FromBody] GoogleCloudDialogflowV2WebhookRequest value)
		{
			return ExecuteKnownValues(value);
		}

		#endregion Public Methods

		#region Private Methods

		private static WebhookResponse StdErrorMessageGenerator()
		{
			return new WebhookResponse
			{
				FulfillmentText = Utilities.ErrorReturnMsg() + Utilities.EndOfCurrentRequest()
			};
		}

		private static string BuildActionMethod(string intendDisplayName)
		{
			string returnString = "";
			switch (intendDisplayName)
			{
				case "fundamentals":
					returnString = "/api/Fundamentals";
					break;

				case "recommend":
					returnString = "/api/Recommendations";
					break;

				case "marketSummary":
					returnString = "/api/MarketSummary";
					break;

				case "newsFetch":
					returnString = "/api/NewsFetch";
					break;

				case "stockQuote":
					returnString = "/api/StockQuote";
					break;

				default:
					break;
			}
			return returnString;
		}

		private IActionResult ExecuteKnownValues(GoogleCloudDialogflowV2WebhookRequest value)
		{
			WebhookResponse returnValue = null;
			var baseURL = Request.Host.ToString();
			var keyUsedToAccess = Request.Headers["key"].ToString();

			string intendDisplayName = value.QueryResult.Intent.DisplayName;
			var client = new RestClient("https://" + baseURL);
			var actionLink = BuildActionMethod(intendDisplayName);
			if (string.IsNullOrWhiteSpace(actionLink))
			{
				returnValue = StdErrorMessageGenerator();
			}
			else
			{
				var request = new RestRequest(actionLink, Method.POST);
				request.AddHeader("key", keyUsedToAccess);
				request.AddJsonBody(value);
				var response = client.Execute(request);
				if (response.IsSuccessful)
				{
					var returnMsg = response.Content;
					return new ContentResult
					{
						Content = returnMsg,
						ContentType = "application/json",
						StatusCode = 200
					};
				}
				else
				{
					returnValue = StdErrorMessageGenerator();
				}
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

		#endregion Private Methods
	}
}