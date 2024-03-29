﻿using DataProvider.Extensions;
using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
	public class ObtainMarketSummary
	{
		#region Private Fields

		private readonly ILogger<ObtainMarketSummary> _log;
		private readonly EnvHandler _envHandler;

		#endregion Private Fields

		#region Public Constructors

		public ObtainMarketSummary(ILogger<ObtainMarketSummary> log, EnvHandler envHandler)
		{
			_log = log;
			_envHandler = envHandler;
		}

		#endregion Public Constructors

		#region Public Methods

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

			QuotesFromWorldTrading indexData = await _envHandler.ObtainFromWorldTrading(tmpStr.ToString());

			WebhookResponse returnValue = BuildOutputMessage(indexData);
			return returnValue;
		}

		#endregion Public Methods

		#region Private Methods

		private WebhookResponse BuildOutputMessage(QuotesFromWorldTrading indexData)
		{
			StringBuilder tmpStr = new StringBuilder();
			if (indexData.Data.Length >= 1)
			{
				var dateToUse = indexData.Data[0].Last_trade_time == null ? DateTime.Parse("01-01-2000") :
					(DateTime)indexData.Data[0].Last_trade_time;
				tmpStr.Append("As of ");
				tmpStr.Append($"{dateToUse.ToString("MMMM dd, hh:mm tt")} EST ");
			}
			foreach (var idxData in indexData.Data)
			{
				float price = idxData.Price != null ? (float)idxData.Price : 0;
				float dayChange = idxData.Day_change == null ? 0 : (float)idxData.Day_change;
				tmpStr.Append($"{idxData.Name}  is at  {Math.Round(price, 0)}. ");
				tmpStr.Append(idxData.Day_change > 0 ? " Up by " : "Down by ");
				tmpStr.Append($"{Math.Abs(Math.Round(dayChange, 0))} points.\n\n ");
			}

			//var xml = await new Ssml().Say("As of ")
			//	.Say(indexData.Data[0].Last_trade_time).As(DateFormat.MonthDayYear)
			//	.Say(indexData.Data[0].Last_trade_time.TimeOfDay).In(TimeFormat.TwelveHour)
			//	.Say(indexData.Data[0].Name + " is at ")
			//	.Say(Convert.ToInt32(indexData.Data[0].Price)).AsCardinalNumber()
			//	.Say(indexData.Data[0].Day_change > 0 ? " Up by " : " Down by ")
			//	.Say(Convert.ToInt32(Math.Abs(Math.Round(indexData.Data[0].Day_change)))).AsCardinalNumber()
			//	.Say(indexData.Data[1].Name + " is at ")
			//	.Say(Convert.ToInt32(indexData.Data[1].Price)).AsCardinalNumber()
			//	.Say(indexData.Data[1].Day_change > 0 ? " Up by " : " Down by ")
			//	.Say(Convert.ToInt32(Math.Abs(Math.Round(indexData.Data[1].Day_change)))).AsCardinalNumber()
			//	.Say(indexData.Data[2].Name + " is at ")
			//	.Say(Convert.ToInt32(indexData.Data[2].Price)).AsCardinalNumber()
			//	.Say(indexData.Data[2].Day_change > 0 ? " Up by " : " Down by ")
			//	.Say(Convert.ToInt32(Math.Abs(Math.Round(indexData.Data[2].Day_change)))).AsCardinalNumber()
			//	.ToStringAsync();

			var returnValue = new WebhookResponse
			{
				FulfillmentText = tmpStr.ToString()
			};

			return returnValue;
		}

		#endregion Private Methods
	}
}