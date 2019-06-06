using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ServeData.MessageProcessors
{
	public class ProcessMessages
	{
		private readonly ILogger<ProcessMessages> _log;

		public ProcessMessages(ILogger<ProcessMessages> log)
		{
			_log = log;
		}

		internal IActionResult ProcessValuesFromIntents(GoogleCloudDialogflowV2WebhookRequest value)
		{
			_log.LogTrace("Starting to process request");
			string intendDisplayName = value.QueryResult.Intent.DisplayName;
			StringBuilder presidentName = new StringBuilder();
			var parameters = value.QueryResult.Parameters;
			foreach (var key in parameters.Keys)
			{
				presidentName.Append($"Parameter {key} value {parameters[key]}\n");
			}
			var response = new WebhookResponse
			{
				FulfillmentText = $"Called from {intendDisplayName} with parameter {presidentName}",
				Source = "C# application"
			};
			var responseString = response.ToString();
			_log.LogTrace("Completed processing request");
			return new ContentResult
			{
				Content = responseString,
				ContentType = "application/json",
				StatusCode = 200
			};
		}

		//private static WebhookResponse ProcessIntent(GoogleCloudDialogflowV2WebhookRequest intent)
		//{
		//	string intendDisplayName = intent.QueryResult.Intent.DisplayName;
		//	switch (intendDisplayName)
		//	{
		//		var actor = new ObtainMakretSummary(intent);
		//	}
		//}
	}
}