using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
	public class ObtainTrenders
	{
		#region Private Fields

		private const string baseUrl = @"https://api.iextrading.com/1.0/stock/market/list/";
		private const string EntityTrendParameter = "trendParameter";
		private readonly ILogger<ObtainTrenders> _log;

		#endregion Private Fields

		#region Public Constructors

		public ObtainTrenders(ILogger<ObtainTrenders> log)
		{
			_log = log;
		}

		#endregion Public Constructors

		#region Public Methods

		public async Task<WebhookResponse> GetTrendingAsync(GoogleCloudDialogflowV2WebhookRequest intent)
		{
			var parameter = intent.QueryResult.Parameters[EntityTrendParameter].ToString();
			var urlToUse = BuildUrlToUse(parameter, out string readableParameter);
			if (string.IsNullOrWhiteSpace(urlToUse))
			{
				return new WebhookResponse
				{
					FulfillmentText = Utilities.ErrorReturnMsg()
				};
			}
			string outputMsg = await GetIexTradingDataAsync(urlToUse, readableParameter);
			return new WebhookResponse
			{
				FulfillmentText = outputMsg
			};
		}

		#endregion Public Methods

		#region Private Methods

		private string BuildUrlToUse(string requestAction, out string readableParameter)
		{
			string urlToUse = "";
			switch (requestAction.ToLower())
			{
				case "mostactive":
					urlToUse = baseUrl + "mostactive";
					readableParameter = "Most active ";
					break;

				case "gainers":
					urlToUse = baseUrl + "gainers";
					readableParameter = "Top gainers ";
					break;

				case "losers":
					urlToUse = baseUrl + "losers";
					readableParameter = "Biggest losers ";
					break;

				case "infocus":
				case "in focus":
					urlToUse = baseUrl + "infocus";
					readableParameter = "In focus stocks ";
					break;

				default:
					readableParameter = "Nothing defined";
					_log.LogDebug($"Should have never come here\n Obtain trends with parameter {requestAction}");
					break;
			}

			return urlToUse;
		}

		private async Task<string> GetIexTradingDataAsync(string urlToUse, string readableParameter)
		{
			string data;
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
			}
			catch (Exception ex)
			{
				_log.LogCritical(ex, "Exception occurred while getting external data");
				return "";
			}
			var trendsRoot = JsonConvert.DeserializeObject<IEnumerable<Trend>>(data);
			var finalOutput = new StringBuilder();
			foreach (var trend in trendsRoot)
			{
				finalOutput.Append(trend.CompanyName + ", ");
			}
			finalOutput.Replace(" Inc.", " ");
			finalOutput.Replace(" Corporation", "");
			data = readableParameter + " for the day are " + finalOutput.ToString();
			return data;
		}

		#endregion Private Methods
	}
}