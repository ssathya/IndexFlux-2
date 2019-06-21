using DataProvider.Extensions;
using Google.Cloud.Dialogflow.V2;
using HandleSimFin.Helpers;
using Microsoft.Extensions.Logging;
using Models;
using MongoHandler.Extensions;
using MongoHandler.Utils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
	public class ObtainGoodInvestments
	{
		private readonly ILogger<ObtainGoodInvestments> _log;
		private readonly IDBConnectionHandler<PiotroskiScoreMd> _ratingsConnectionHandler;
		private readonly IDBConnectionHandler<CompanyDetailMd> _dbconCompany;
		private readonly EnvHandler _envHandler;
		private const string iexTargetPrice = @"https://cloud.iexapis.com/stable/stock/{ticker}/price-target?token={api-key}";
		private const string iexLastTradePrice = @"https://cloud.iexapis.com/stable/stock/{ticker}/price?token={api-key}";
		private const string iexTradingProvider = "IEXTrading";

		public ObtainGoodInvestments(ILogger<ObtainGoodInvestments> log,
			IDBConnectionHandler<PiotroskiScoreMd> connectionHandlerCF,
			IDBConnectionHandler<CompanyDetailMd> dbconCompany,
			EnvHandler envHandler)
		{
			_log = log;
			_ratingsConnectionHandler = connectionHandlerCF;
			_dbconCompany = dbconCompany;
			_envHandler = envHandler;
			_ratingsConnectionHandler.ConnectToDatabase("PiotroskiScore");
			_dbconCompany.ConnectToDatabase("CompanyDetail");
		}

		public async Task<WebhookResponse> SelectRandomGoodFirms()
		{
			_log.LogTrace("Started to select better investments");
			try
			{
				var betterScores = _ratingsConnectionHandler.Get(r => r.Rating >= 7 && r.FYear == DateTime.Now.Year).ToList();
				if (betterScores == null || betterScores.Count == 0)
				{
					betterScores = _ratingsConnectionHandler.Get(r => r.Rating >= 7 && r.FYear == DateTime.Now.Year - 1).ToList();
				}
				if (betterScores.Any())
				{
					betterScores.Shuffle();
				}
				betterScores = betterScores.Take(4).ToList();
				var messageString = new StringBuilder();
				messageString.Append("Here are a few recommendations for you.\n");
				foreach (var piotroskiScore in betterScores)
				{
					var companyName = _dbconCompany.Get(r => r.SimId.Equals(piotroskiScore.SimId)).FirstOrDefault().Name;
					if (!string.IsNullOrWhiteSpace(companyName))
					{
						string targetPrice;
						if (betterScores.First() != piotroskiScore)
						{
							var a = await Task.Delay(100).ContinueWith(t => GetTargetPriceAsync(piotroskiScore.Ticker));
							targetPrice = a.Result;
						}
						else
						{
							targetPrice = await GetTargetPriceAsync(piotroskiScore.Ticker);
						}
						messageString.Append($"{companyName} with ticker {piotroskiScore.Ticker} scores {piotroskiScore.Rating}.\n");
						if (!targetPrice.IsNullOrWhiteSpace())
						{
							messageString.Append(targetPrice);
						}
					}
				}
				messageString.Append($"\n Note: Recommendations are based on SEC filings. Market conditions will affect the company's performance.\n");
				messageString.Append($"\n Further research needs to be done before you place your order.\n");
				var returnResponse = new WebhookResponse
				{
					FulfillmentText = messageString.ToString()
				};
				return returnResponse;
			}
			catch (Exception ex)
			{
				_log.LogCritical($"Error while getting data from database;\n{ex.Message}");
				return new WebhookResponse();
			}
		}

		private async Task<string> GetTargetPriceAsync(string ticker)
		{
			var urlToUseForTarget = iexTargetPrice.Replace(@"{ticker}", ticker)
				.Replace(@"{api-key}", _envHandler.GetApiKey(iexTradingProvider));
			var urlToUseForPrice = iexLastTradePrice.Replace(@"{ticker}", ticker)
				.Replace(@"{api-key}", _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					string lastTradePrice = "";
					data = await wc.DownloadStringTaskAsync(urlToUseForTarget);
					lastTradePrice = await wc.DownloadStringTaskAsync(urlToUseForPrice);
					var targetPrice = JsonConvert.DeserializeObject<TargetPrice>(data);
					if (targetPrice != null && !targetPrice.Symbol.IsNullOrWhiteSpace())
					{
						var returnString = $"\n As of {targetPrice.UpdatedDate.ToString("MMMM dd")} {targetPrice.NumberOfAnalysts} Analysts project {targetPrice.PriceTargetAverage.ToString("c2")} as target price \n";
						returnString += $"The target ranges between {targetPrice.PriceTargetLow.ToString("c2")} to {targetPrice.PriceTargetHigh.ToString("c2")} \n";
						if (!lastTradePrice.IsNullOrWhiteSpace())
						{
							returnString += $" It last traded at ${lastTradePrice}\n\n ";
						}
						return returnString;
					}
					return "";
				}
			}
			catch (Exception ex)
			{
				_log.LogError("Error while getting data from IEX Trading");
				_log.LogError(ex.Message);
				if (ex.InnerException != null)
				{
					_log.LogError(ex.InnerException.Message);
				}
				return "";
			}
		}
	}
}