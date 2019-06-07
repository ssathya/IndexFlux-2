using DataProvider.Extensions;
using Google.Cloud.Dialogflow.V2;
using Kevsoft.Ssml;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Google.Cloud.Dialogflow.V2.Intent.Types;

namespace DataProvider.BusLogic
{
	public class ObtainMarketSummary
	{
		private readonly ILogger<ObtainMarketSummary> _log;

		public ObtainMarketSummary(ILogger<ObtainMarketSummary> log)
		{
			_log = log;
		}

		public async Task<WebhookResponse> GetIndicesValuesAsync()
		{
			var tickers = new List<string>
			{
				"^DJI",
				"^IXIC",
				"^INX"
			};
			var tmpStr = new StringBuilder();
			tmpStr.AppendJoin(',', tickers);

			IndexData indexData = await ObtainFromWorldTrading(tmpStr.ToString());
			
			WebhookResponse returnValue = await BuildOutputMessage(indexData);			
			return returnValue;
		}

		private async Task<IndexData> ObtainFromWorldTrading(string tickersToUse)
		{
			var apiKey = Environment.GetEnvironmentVariable("WorldTradingDataKey", EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in process");
				apiKey = Environment.GetEnvironmentVariable("WorldTradingDataKey", EnvironmentVariableTarget.Machine);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable("WorldTradingDataKey", EnvironmentVariableTarget.User);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable("WorldTradingDataKey");
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogError("Did not find api key; calls will fail");
			}
			string urlStr = $@"https://www.worldtradingdata.com/api/v1/stock?symbol={tickersToUse}&api_token={apiKey}";
			string data = "{}";
			using (var wc = new WebClient())
			{
				data = await wc.DownloadStringTaskAsync(urlStr);
			}
			var indexData = JsonConvert.DeserializeObject<IndexData>(data);
			return indexData;
		}

		private async Task<WebhookResponse> BuildOutputMessage(IndexData indexData)
		{
			

			StringBuilder tmpStr = new StringBuilder();
			if (indexData.Data.Length >= 1)
			{
				var dateToUse = indexData.Data[0].Last_trade_time;
				tmpStr.Append("As of ");
				tmpStr.Append($"{dateToUse.ToString("MMMM dd, hh:mm tt")} EST ");
			}
			foreach (var idxData in indexData.Data)
			{
				//string direction = idxData.Change_pct < 0 ? "downward" : "upward";
				tmpStr.Append($"{idxData.Name}  is at  {Math.Round(idxData.Price, 0)}. ");
				tmpStr.Append(idxData.Day_change > 0 ? " Up by " : "Down by ");
				tmpStr.Append($"{Math.Abs(Math.Round(idxData.Day_change, 0))} points.\n\n ");
			}
			
			var xml = await new Ssml().Say("As of ")
				.Say(indexData.Data[0].Last_trade_time).As(DateFormat.MonthDayYear)
				.Say(indexData.Data[0].Last_trade_time.TimeOfDay).In(TimeFormat.TwelveHour)
				.Say(indexData.Data[0].Name + " is at ")
				.Say(Convert.ToInt32(indexData.Data[0].Price)).AsCardinalNumber()
				.Say(indexData.Data[0].Day_change > 0 ? " Up by " : " Down by ")
				.Say(Convert.ToInt32(Math.Abs(Math.Round(indexData.Data[0].Day_change)))).AsCardinalNumber()
				.Say(indexData.Data[1].Name + " is at ")
				.Say(Convert.ToInt32(indexData.Data[1].Price)).AsCardinalNumber()
				.Say(indexData.Data[1].Day_change > 0 ? " Up by " : " Down by ")
				.Say(Convert.ToInt32(Math.Abs(Math.Round(indexData.Data[1].Day_change)))).AsCardinalNumber()
				.Say(indexData.Data[2].Name + " is at ")
				.Say(Convert.ToInt32(indexData.Data[2].Price)).AsCardinalNumber()
				.Say(indexData.Data[2].Day_change > 0 ? " Up by " : " Down by ")
				.Say(Convert.ToInt32(Math.Abs(Math.Round(indexData.Data[2].Day_change)))).AsCardinalNumber()
				.ToStringAsync();


			var returnValue = new WebhookResponse
			{
				FulfillmentText = tmpStr.ToString()
			};
			

			return returnValue;
		}
	}
}