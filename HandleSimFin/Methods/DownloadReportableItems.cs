﻿using HandleSimFin.Utils;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
	public class DownloadReportableItems
	{

		#region Private Fields

		private const string UrlTemplate = @"https://simfin.com/api/v1/companies/id/{companyId}/statements/standardised?api-key={API-KEY}&stype={statementType}&ptype={periodType}&fyear={financialYear}";
		private readonly ILogger _logger;

		#endregion Private Fields


		#region Public Constructors

		public DownloadReportableItems(ILogger logger)
		{
			_logger = logger;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<List<CompanyFinancials>> DownloadFinancialsAsync(StatementList statementList)
		{
			var cfl = new List<CompanyFinancials>();
			string apiKey = HandleSimFinUtils.GetApiKey(_logger);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Did not find API key; calls will fail");
				return null;
			}
			var companyId = statementList.CompanyId;

			//get balance sheets
			var statementType = "bs";
			var reportingList = statementList.Bs.OrderByDescending(b => b.Fyear).Take(5);
			foreach (var bs in reportingList)
			{
				var bsToAdd = await ObtainReportedNumbers(companyId, apiKey, statementType, bs);
				if (bsToAdd != null)
				{
					AddAdditionalDetailstoFinancials(companyId, bs, bsToAdd, StatementType.BalanceSheet);
					cfl.Add(bsToAdd);
				}
			}

			// get profit and loss
			statementType = "pl";
			reportingList = statementList.Pl.OrderByDescending(b => b.Fyear).Take(5);
			foreach (var pl in reportingList)
			{
				var plToAdd = await ObtainReportedNumbers(companyId, apiKey, statementType, pl);
				if (plToAdd != null)
				{
					AddAdditionalDetailstoFinancials(companyId, pl, plToAdd, StatementType.ProfitLoss);
					cfl.Add(plToAdd);
				}				
			}

			//get cash flow
			statementType = "cf";
			reportingList = statementList.Cf.OrderByDescending(b => b.Fyear).Take(5);
			foreach (var cf in reportingList)
			{
				var cfToAdd = await ObtainReportedNumbers(companyId, apiKey, statementType, cf);
				if (cfToAdd != null)
				{
					AddAdditionalDetailstoFinancials(companyId, cf, cfToAdd, StatementType.CashFlow);
					cfl.Add(cfToAdd);
				}
				
			}
			return cfl;
		}

		#endregion Public Methods


		#region Private Methods

		private  void AddAdditionalDetailstoFinancials(string companyId, StatementDetails bs, CompanyFinancials cfToAdd, StatementType st)
		{
			cfToAdd.Statement = st;
			cfToAdd.FYear = bs.Fyear;
			cfToAdd.CompanyId = companyId;
		}

		private  async Task<CompanyFinancials> ObtainReportedNumbers(string companyId, string apiKey, string statementType, StatementDetails bs)
		{
			CompanyFinancials cfToAdd;
			try
			{
				using (var wc = new WebClient())
				{
					var urlToUse = UrlTemplate.Replace(@"{API-KEY}", apiKey)
						.Replace(@"{companyId}", companyId)
						.Replace(@"{statementType}", statementType)
						.Replace(@"{periodType}", bs.Period)
						.Replace(@"{financialYear}", bs.Fyear.ToString());
					string data = "";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					cfToAdd = JsonConvert.DeserializeObject<CompanyFinancials>(data);
				}
				return cfToAdd;
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Exception in DownloadReportableItems::ObtainReportedNumbers\n{ex.Message}");
				if (ex.InnerException != null)
				{
					_logger.LogCritical(ex.InnerException.Message);
				}
				return null;
				
			}
		}

		#endregion Private Methods
	}
}