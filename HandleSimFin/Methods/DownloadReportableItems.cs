using HandleSimFin.Utils;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
    public class DownloadReportableItems
    {
		private readonly ILogger _logger;
		private const string UrlTemplate = @"https://simfin.com/api/v1/companies/id/{companyId}/statements/standardised?api-key={API-KEY}&stype={statementType}&ptype={periodType}&fyear={financialYear}";
		public DownloadReportableItems(ILogger logger)
		{
			_logger = logger;
		}
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
				var cfToAdd = await ObtainReportedNumbers(companyId, apiKey, statementType, bs);				
				AddAdditionalDetailstoFinancials(companyId, bs, cfToAdd, StatementType.BalanceSheet);
				cfl.Add(cfToAdd);
			}

			// get profit and loss
			statementType = "pl";
			reportingList = statementList.Pl.OrderByDescending(b => b.Fyear).Take(5);
			foreach (var pl in reportingList)
			{
				var cfToAdd = await ObtainReportedNumbers(companyId, apiKey, statementType, pl);
				AddAdditionalDetailstoFinancials(companyId, pl, cfToAdd, StatementType.ProfitLoss);
				cfl.Add(cfToAdd);
			}

			//get cash flow
			statementType = "cf";
			reportingList = statementList.Cf.OrderByDescending(b => b.Fyear).Take(5);
			foreach (var cf in reportingList)
			{
				var cfToAdd = await ObtainReportedNumbers(companyId, apiKey, statementType, cf);
				AddAdditionalDetailstoFinancials(companyId, cf, cfToAdd, StatementType.CashFlow);
				cfl.Add(cfToAdd);
			}
			return cfl;
		}

		private static void AddAdditionalDetailstoFinancials(string companyId, StatementDetails bs, CompanyFinancials cfToAdd, StatementType st)
		{
			cfToAdd.Statement = st;
			cfToAdd.FYear = bs.Fyear;
			cfToAdd.CompanyId = companyId;
		}

		private static async Task<CompanyFinancials> ObtainReportedNumbers(string companyId, string apiKey, string statementType, StatementDetails bs)
		{
			CompanyFinancials cfToAdd;
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
	}
}
