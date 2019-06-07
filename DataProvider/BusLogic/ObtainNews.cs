using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
	public class ObtainNews
	{

		#region Private Fields

		private const string newsParameter = "newsSource";
		private readonly ILogger<ObtainNews> _log;

		#endregion Private Fields


		#region Public Constructors

		public ObtainNews(ILogger<ObtainNews> log)
		{
			_log = log;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<WebhookResponse> GetExternalNews(GoogleCloudDialogflowV2WebhookRequest intent)
		{
			var newsSourceRequested = intent.QueryResult.Parameters[newsParameter].ToString();
			string urlToUse = "";
			urlToUse = BuildUrlToUse(newsSourceRequested, out string readableParameter);
			NewsExtract extracts = await ObtainNewAPIDta(urlToUse);
			if (extracts == null)
			{
				return new WebhookResponse
				{
					FulfillmentText = Utilities.ErrorReturnMsg()
				};
			}

			string returnString = ExtractHeadlines(extracts);

			var returnValue = new WebhookResponse
			{
				FulfillmentText = returnString
			};
			// returnValue = ExtractHeadlines(extracts, returnValue);

			return returnValue;
		}

		#endregion Public Methods


		#region Private Methods

		private string BuildUrlToUse(string newsSourceRequested, out string readableParameter)
		{
			readableParameter = "";

			string newsMedia = "sources=";
			switch (newsSourceRequested.ToLower())
			{
				case "cnbc":
					newsMedia += newsSourceRequested.ToLower().Trim();
					break;

				case "the-new-york-times":
					newsMedia += newsSourceRequested.ToLower().Trim();
					break;

				case "the-wall-street-journal":
					newsMedia += newsSourceRequested.ToLower().Trim();
					break;

				case "the-hindu":
					newsMedia += newsSourceRequested.ToLower().Trim();
					break;

				default:
					newsMedia = "country=us";
					break;
			}
			string apiKey = GetApiKey();
			var urlStr = $"https://newsapi.org/v2/top-headlines?{newsMedia}&apiKey={apiKey}";
			return urlStr;
		}

		private string ExtractHeadlines(NewsExtract extracts)
		{
			var returnMsg = new StringBuilder();
			if (extracts.totalResults == 0 || !extracts.status.Equals("ok"))
			{
				return "Cannot obtain news at this time\n";
			}
			returnMsg.Append("Here are the headlines from " + extracts.articles[0].source.name + ".\n");
			foreach (var article in extracts.articles)
			{
				returnMsg.Append(article.title + ".\n");
			}
			return returnMsg.ToString();
		}

		private string GetApiKey()
		{
			var apiKey = Environment.GetEnvironmentVariable("NewsAPI", EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in process");
				apiKey = Environment.GetEnvironmentVariable("NewsAPI", EnvironmentVariableTarget.Machine);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable("NewsAPI", EnvironmentVariableTarget.User);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable("NewsAPI");
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogError("Did not find api key; calls will fail");
			}
			return apiKey;
		}

		private async Task<NewsExtract> ObtainNewAPIDta(string urlToUse)
		{
			string data = "{}";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				var newsData = JsonConvert.DeserializeObject<NewsExtract>(data);
				return newsData;
			}
			catch (Exception ex)
			{
				_log.LogCritical($"Error while obtaining news\n{ex.Message}");
				return null;
			}
		}

		#endregion Private Methods
	}
}