using Microsoft.Extensions.Logging;
using Models;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoReadWrite.BusLogic
{
	public class AnalyzeFinancial
	{
		private readonly IDBConnectionHandler<CompanyFinancialsMd> _dbconCompany;

		private readonly ILogger<AnalyzeFinancial> _logger;

		private readonly HandleCompanyList _hcl;

		public AnalyzeFinancial(IDBConnectionHandler<CompanyFinancialsMd> dbconCompany,
			ILogger<AnalyzeFinancial> logger,
			HandleCompanyList hcl)
		{
			_dbconCompany = dbconCompany;
			_logger = logger;
			_hcl = hcl;
		}
		public Dictionary<int, Dictionary<string, long>> ReadFinanceValues(string simId)
		{
			var companyBasicInfo = _hcl.GetCompanyDetails(simId);
			if (companyBasicInfo == null)
			{
				_logger.LogError("Basic information about the requested firm is not available.");
				return null;
			}
			var industryTemplate = companyBasicInfo.IndustryTemplate;
			var financial = _dbconCompany.Get(cf => cf.CompanyId.Equals(simId)).ToList();
			if (financial == null || !financial.Any())
			{
				_logger.LogWarning($"Financial values for {companyBasicInfo.Name} is not stored in database yet ");
				return null;
			}
			Dictionary<int, Dictionary<string, long>> returnValue;
			switch (companyBasicInfo.IndustryTemplate)
			{
				case "general":
					returnValue = GeneralFlattenData(financial);
					break;
				default:
					returnValue = null;
					break;
			}
			return returnValue;
		}

		private Dictionary<int, Dictionary<string, long>> GeneralFlattenData(List<CompanyFinancialsMd> financial)
		{
			var valueRef = new Dictionary<int, Dictionary<string, long>>();
			var years = financial.Select(x => x.FYear).Distinct();
			var firstYearData = financial.Where(x => x.FYear == years.First());
			string[] bsKeys = ExtractKeys(firstYearData, StatementType.BalanceSheet);
			string[] cfKeys = ExtractKeys(firstYearData, StatementType.CashFlow);
			string[] plKeys = ExtractKeys(firstYearData, StatementType.ProfitLoss);
			foreach (var year in years)
			{
				valueRef.Add(year, new Dictionary<string, long>());
				foreach (var bsKey in bsKeys)
				{
					PopulateFlattendData(financial, valueRef, year, bsKey, StatementType.BalanceSheet);
				}
				foreach (var cfKey in cfKeys)
				{
					PopulateFlattendData(financial, valueRef, year, cfKey, StatementType.CashFlow);

				}
				foreach (var plKey in plKeys)
				{
					PopulateFlattendData(financial, valueRef, year, plKey, StatementType.ProfitLoss);
				}
			}
			return valueRef;
		}

		private void PopulateFlattendData(List<CompanyFinancialsMd> financial, Dictionary<int, Dictionary<string, long>> valueRef, int year, string key, StatementType statementType)
		{
			var extractedValue = ExtractChosenValue(financial, year, key, statementType);			
			if (valueRef[year].ContainsKey(key))
			{
				var cfmd = financial.Where(f => f.FYear == year && f.Statement == statementType).FirstOrDefault();
				var selectedValues = cfmd.Values.Where(v => v.StandardisedName == key);
				string newKey = key;
				Value selecteVal = selectedValues.First();
				foreach (var selectedValue in selectedValues)
				{
					newKey = $"{key}-{selectedValue.Tid}";
					if (!valueRef[year].ContainsKey(newKey))
					{
						extractedValue = ExtractChosenValue(financial, year, key, statementType, selectedValue.Tid);
						break;
					}
					else
					{
						newKey = "";
					}
				}
				if (!newKey.IsNullOrWhiteSpace())
				{
					valueRef[year].Add(newKey, extractedValue);
				}
				
			}
			else
			{
				valueRef[year].Add(key, extractedValue);
			}
		}

		private static long ExtractChosenValue(List<CompanyFinancialsMd> financial, int year, string key, StatementType statementType, int tid=0)
		{
			var valueChosen = (from rcd in financial.Where(i => i.Statement == statementType && i.FYear == year)
							   select (rcd.Values.Find(a => a.StandardisedName.Equals(key)).ValueChosen)).FirstOrDefault();
			if (tid != 0)
			{
				valueChosen = (from rcd in financial.Where(i => i.Statement == statementType && i.FYear == year)
							   select (rcd.Values.Find(a => a.StandardisedName.Equals(key) && a.Tid== tid).ValueChosen)).FirstOrDefault();
			}
			
			return valueChosen == null ? 0 : (long)valueChosen;

		}

		private string[] ExtractKeys(IEnumerable<CompanyFinancialsMd> firstYearData, StatementType statementType)
		{
			var records = firstYearData.Where(st => st.Statement == statementType).FirstOrDefault().Values;
			if (records == null || !records.Any())
			{
				_logger.LogCritical("Extract Keys is not able to identify keys; Reload database");
				return null;
			}
			var returnStrArray = (from record in records
								  select record.StandardisedName);
			return returnStrArray.ToArray();
		}
	}
}
