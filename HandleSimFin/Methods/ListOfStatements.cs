﻿using HandleSimFin.Utils;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
	public enum IdentifyerType
	{
		CompanyName,
		Ticker,
		SimFinId
	};

	public class ListOfStatements
	{


		#region Private Fields

		private const string urlForStatmentList = @"https://simfin.com/api/v1/companies/id/{companyId}/statements/list?api-key={API-KEY}";
		private readonly ILogger _logger;
		private StatementList statementList;

		#endregion Private Fields


		#region Public Constructors

		public ListOfStatements(ILogger logger)
		{
			_logger = logger;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<StatementList> FetchStatementList(string identifyer, IdentifyerType identifyerType = IdentifyerType.SimFinId)
		{
			string companyId = "";
			companyId = await IdentifyerToSimFinId(identifyer, identifyerType, companyId);
			if (string.IsNullOrEmpty(companyId))
			{
				return null;
			}
			return await GetListOfStatements(companyId);
		}

		public StatementList RemovePastTTMs(StatementList statement = null)
		{
			if (statement == null)
			{
				statement = statementList;
			}
			var returnValue = new StatementList
			{
				Bs = statement.Bs.Where(bs => !bs.Period.Contains("TTM-")).ToList(),
				Cf = statement.Cf.Where(cf => !cf.Period.Contains("TTM-")).ToList(),
				Pl = statement.Pl.Where(pl => !pl.Period.Contains("TTM-")).ToList()
			};
			return returnValue;
		}
		public StatementList ExtractYearEndReports(StatementList statement = null)
		{
			if (statement == null)
			{
				statement = statementList;
			}
			var returnValue = new StatementList
			{
				Bs = statement.Bs.Where(bs => bs.Period.Equals("TTM") || bs.Period.Contains("FY") || (bs.Period.Equals("Q4") && bs.Calculated==false)).ToList(),
				Cf = statement.Cf.Where(cf => cf.Period.Equals("TTM") || cf.Period.Contains("FY")).ToList(),
				Pl = statement.Pl.Where(pl => pl.Period.Equals("TTM") || pl.Period.Contains("FY")).ToList()
			};
			foreach (var bs in returnValue.Bs)
			{
				if (bs.Period.Equals("Q4"))
				{
					bs.Period = "FY";
				}
			}
			return returnValue;
		}
		#endregion Public Methods


		#region Private Methods

		private async Task<StatementList> GetListOfStatements(string companyId)
		{
			var urlToUse = urlForStatmentList.Replace(@"{companyId}", companyId);
			string apiKey = HandleSimFinUtils.GetApiKey(_logger);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Did not find API key; calls will fail");
				return null;
			}
			urlToUse = urlToUse.Replace(@"{API-KEY}", apiKey);
			string data = "[]";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				statementList = JsonConvert.DeserializeObject<StatementList>(data);
				return statementList;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in ListOfStatements:FetchStatementList\n{ex.Message}");
				if (ex.InnerException != null)
				{
					_logger.LogError($"\n{ex.InnerException.Message}");
				}
				return null;
			}
		}

		private async Task<string> IdentifyerToSimFinId(string identifyer, IdentifyerType identifyerType, string companyId)
		{
			var getSimId = new GetSimId(_logger);
			switch (identifyerType)
			{
				case IdentifyerType.CompanyName:
					companyId = await getSimId.GetSimIdByCompanyName(identifyer);
					break;

				case IdentifyerType.Ticker:
					companyId = await getSimId.GetSimIdByTicker(identifyer);
					break;

				case IdentifyerType.SimFinId:
					companyId = identifyer;
					break;

				default:
					break;
			}

			return companyId;
		}

		#endregion Private Methods

	}
}