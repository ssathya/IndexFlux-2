using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Logging;
using Models;
using MongoHandler.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoHandler.Extensions;

namespace DataProvider.BusLogic
{
    public class ObtainGoodInvestments
    {
		private readonly ILogger<ObtainGoodInvestments> _log;
		private readonly IDBConnectionHandler<PiotroskiScoreMd> _ratingsConnectionHandler;
		private readonly IDBConnectionHandler<CompanyDetailMd> _dbconCompany;

		public ObtainGoodInvestments(ILogger<ObtainGoodInvestments> log,
			IDBConnectionHandler<PiotroskiScoreMd> connectionHandlerCF,
			IDBConnectionHandler<CompanyDetailMd> dbconCompany)
		{
			_log = log;
			_ratingsConnectionHandler = connectionHandlerCF;
			_dbconCompany = dbconCompany;
			_ratingsConnectionHandler.ConnectToDatabase("PiotroskiScore");
			_dbconCompany.ConnectToDatabase("CompanyDetail");
		}
		public WebhookResponse SelectRandomGoodFirms()
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
						messageString.Append($"{companyName} with ticker {piotroskiScore.Ticker} scores {piotroskiScore.Rating}.\n");
					}
				}
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
    }
}
