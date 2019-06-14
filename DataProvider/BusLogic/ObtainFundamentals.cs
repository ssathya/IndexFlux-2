﻿using AutoMapper;
using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using HandleSimFin.Helpers;
using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using MongoHandler.Utils;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
	public class ObtainFundamentals
	{
		#region Private Fields

		private readonly IDBConnectionHandler<PiotroskiScoreMd> _connectionHandlerCF;
		private readonly ObtainCompanyDetails _obtainCompanyDetails;
		private readonly ILogger<ObtainFundamentals> _log;
		private readonly string companyName = "CompanyName";

		#endregion Private Fields

		#region Public Constructors

		public ObtainFundamentals(ILogger<ObtainFundamentals> log,
			ObtainCompanyDetails obtainCompanyDetails,
			IDBConnectionHandler<PiotroskiScoreMd> connectionHandlerCF)
		{
			_log = log;
			_obtainCompanyDetails = obtainCompanyDetails;
			_connectionHandlerCF = connectionHandlerCF;
			_connectionHandlerCF.ConnectToDatabase("PiotroskiScore");
		}

		#endregion Public Constructors

		#region Public Methods

		public async Task<WebhookResponse> GetCompanyRatings(GoogleCloudDialogflowV2WebhookRequest ratingsParameters)
		{
			_log.LogTrace("Start obtain fundamentals");
			var companyNameToResolve = ratingsParameters.QueryResult.Parameters[companyName].ToString();
			var tickersToUse = await _obtainCompanyDetails.ResolveCompanyNameOrTicker(companyNameToResolve);
			if (string.IsNullOrWhiteSpace(tickersToUse))
			{
				return new WebhookResponse
				{
					FulfillmentText = $"Could not resolve {companyNameToResolve}"
				};
			}

			var fulfillmentText = new StringBuilder();
			try
			{
				int counter = 0;
				foreach (var ticker in tickersToUse.Split(','))
				{
					var computedRatingMd = _connectionHandlerCF.Get().Where(x => x.Ticker.ToLower().Equals(ticker.ToLower()) &&
						x.FYear == DateTime.Now.Year).FirstOrDefault();
					if (computedRatingMd == null)
					{
						computedRatingMd = _connectionHandlerCF.Get().Where(x => x.Ticker.ToLower().Equals(ticker.ToLower()) &&
						x.FYear == DateTime.Now.Year - 1).FirstOrDefault();
					}
					PiotroskiScore computedRating = null;
					if (computedRatingMd != null)
					{
						computedRating = Mapper.Map<PiotroskiScore>(computedRatingMd);
					}
					fulfillmentText.Append(await BuildCompanyProfile(ticker, computedRating));
					if (++counter >= 2)
					{
						break;
					}
				}
				if (counter >= 2)
				{
					fulfillmentText.Append($"Limiting result set as the search term {companyNameToResolve} resolved to too many results.\n");
				}
			}
			catch (Exception ex)
			{
				_log.LogCritical($"Error while processing Getting Company Ratings; \n{ex.Message}");
				return new WebhookResponse
				{
					FulfillmentText = Utilities.ErrorReturnMsg() + Utilities.EndOfCurrentRequest()
				};
			}
			if (fulfillmentText.ToString().Contains("Piotroski"))
			{
				fulfillmentText.Append("Note: Piotroski F-Score is based on company fundamentals; " +
					"a rating greater than 6 indicates strong fundamentals");
			}
			var webhookResponse = new WebhookResponse
			{
				FulfillmentText = fulfillmentText.ToString()
			};
			_log.LogTrace("End obtain fundamentals");
			return webhookResponse;
		}

		#endregion Public Methods

		#region Private Methods

		private async Task<string> BuildCompanyProfile(string ticker, PiotroskiScore computedRating)
		{
			var companyOverview = await _obtainCompanyDetails.ObtainCompanyOverview(ticker);
			var returnText = new StringBuilder();
			if (companyOverview == null || string.IsNullOrWhiteSpace(companyOverview.Symbol))
			{
				returnText.Append($" No basic information about {ticker} found at this time\n");
			}
			else
			{
				returnText.Append($"Basic information about {companyOverview.CompanyName} trading with symbol {companyOverview.Symbol}.\n");
				returnText.Append($" {(companyOverview.Description.IsNullOrWhiteSpace() ? "This information is not available now" : companyOverview.Description.TruncateAtWord(200))}\n");
				if (companyOverview.Description.Length >= 201)
				{
					returnText.Append("\n\n..... more removed. ....\n\n");
				}
				returnText.Append($" Its industry sector is {companyOverview.Sector} and falls under {companyOverview.Industry}\n\n ");
			}
			switch (companyOverview.IssueType.ToLower())
			{
				case "cs":
					returnText.Append(BuildCommonStockMessage(ticker, computedRating));
					break;
				case "ad":
					returnText.Append($" {ticker} is an American Depositary Receipt: ADR");
					break;
				case "re":
					returnText.Append($" {ticker} is a Real estate investment trust: REIT");
					break;
				case "ce":
				case "cef":
					returnText.Append($" {ticker} is a closed end fund");
					break;
				case "si":
					returnText.Append($" {ticker} is a secondary issue");
					break;
				case "et":
				case "etf":
					returnText.Append($" {ticker} is a exchange traded fund; ETF");
					break;
				case "ps":
				case "wt":
					returnText.Append($" {ticker} is a preferred stock or warrant; individuals do not trade these!");
					break;
				default:
					returnText.Append($" No additional information is available for {ticker}");
					break;
			}
			return returnText.ToString();
		}

		private string BuildCommonStockMessage(string ticker, PiotroskiScore computedRating)
		{
			var returnText = new StringBuilder();
			if (computedRating == null)
			{
				returnText.Append($" We do not have any additional details about {ticker} at this time.\n");
				return returnText.ToString();
			}
			returnText.Append($"Additional details about {ticker} using its SEC filings.\n");
			returnText.Append($" Revenues {StringTools.ToKMB((decimal)computedRating.Revenue)}\n");
			returnText.Append($" EBITDA {StringTools.ToKMB(computedRating.EBITDA)} \n");
			returnText.Append($" Gross Margin {computedRating.ProfitablityRatios["Gross Margin"]}%\n");
			returnText.Append($" Operating Margin {computedRating.ProfitablityRatios["Operating Margin"]}%\n");
			returnText.Append($" Net Profit Margin {computedRating.ProfitablityRatios["Net Profit Margin"]}%\n");
			returnText.Append($" Return on Equity {computedRating.ProfitablityRatios["Return on Equity"]}%\n");
			returnText.Append($" Return on Assets {computedRating.ProfitablityRatios["Return on Assets"]}%\n");
			if (computedRating.Rating != 0)
			{
				returnText.Append($" Piotroski F-Score ratings for {ticker} is {computedRating.Rating}.  \n");
			}
			return returnText.ToString();
		}

		#endregion Private Methods
	}
}