using Microsoft.Extensions.Logging;
using Models;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoReadWrite.BusLogic
{
	public class AnalyzeFinancial
	{

		private readonly IDBConnectionHandler<CompanyFinancialsMd> _dbconCompany;

		private readonly HandleCompanyList _hcl;
		private readonly ILogger<AnalyzeFinancial> _logger;

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
			var ebitdaLst = new Dictionary<int, long>();
			var pietroskiScores = new Dictionary<int, int>();
			switch (companyBasicInfo.IndustryTemplate)
			{
				case "general":
					returnValue = FlattenData(financial);
					ComputeEBITDA(returnValue, ebitdaLst);
					ComputePietroskiScore(returnValue, pietroskiScores);
					break;

				default:
					returnValue = null;
					break;
			}
			return returnValue;
		}

		private void ComputePietroskiScore(Dictionary<int, Dictionary<string, long>> normalizedFin, Dictionary<int, int> pietroskiScores)
		{
			foreach (var year in normalizedFin.Keys)
			{
				var totalScore = 0;
				var normalizedFinForYear = normalizedFin[year];
				var normalizedFinForPrevYear = normalizedFin.ContainsKey(year - 1) ? normalizedFin[year - 1] : normalizedFin[year];
				totalScore += RetrunOnAssets(normalizedFinForYear);
				totalScore += OperatingCashFlow(normalizedFinForYear);
				totalScore += ChangeInReturnOfAssets(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += Accruals(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInLeverage(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInCurrentRatio(normalizedFinForYear, normalizedFinForPrevYear);
			}
			//throw new NotImplementedException();
		}

		private int ChangeInCurrentRatio(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var totalCurrentAssetsCY = normalizedFinForYear[@"Total Current Assets"];
			var totalCurrentLiabilitiesCY = normalizedFinForYear[@"Total Current Liabilities"];
			var ratio1 = totalCurrentAssetsCY / totalCurrentLiabilitiesCY;

			//PY prior year
			var totalCurrentAssetsPY = normalizedFinForPrevYear[@"Total Current Assets"];
			var totalCurrentLiabilitiesPY = normalizedFinForPrevYear[@"Total Current Liabilities"];
			var ratio2 = totalCurrentAssetsPY / totalCurrentLiabilitiesPY;
			return ratio1 - ratio2 > 0 ? 1 : 0;
			
		}

		private int ChangeInLeverage(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var longTermDebtCY = normalizedFinForYear[@"Long Term Debt"];
			var totalAssetsCY = normalizedFinForYear[@"Total Assets"];
			var ratio1 = longTermDebtCY / totalAssetsCY;

			//PY prior year
			var longTermDebtPY = normalizedFinForPrevYear[@"Long Term Debt"];
			var totalAssetsPY = normalizedFinForPrevYear[@"Total Assets"];
			var ratio2 = longTermDebtPY / totalAssetsPY;
			return ratio1 - ratio2 < 0 ? 1 : 0;
		}

		private int Accruals(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var cashFromOperatingActivitiesCY = normalizedFinForYear[@"Cash from Operating Activities"];
			var totalAssetsCY = normalizedFinForYear[@"Total Assets"];
			var ratio1 = cashFromOperatingActivitiesCY / totalAssetsCY;
			//PY prior year
			var cashFromOperatingActivitiesPY = normalizedFinForPrevYear[@"Cash from Operating Activities"];
			var totalAssetsPY = normalizedFinForPrevYear[@"Total Assets"];
			var ratio2 = cashFromOperatingActivitiesPY / totalAssetsPY;
			return ratio1 - ratio2 > 0 ? 1 : 0;

		}

		private int ChangeInReturnOfAssets(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var incomeCY = normalizedFinForYear[@"Income (Loss) Including Minority Interest"];
			var totalAssetsCY = normalizedFinForYear[@"Total Assets"];
			var ratio1 = incomeCY / totalAssetsCY;
			//PY prior year
			var incomePY = normalizedFinForPrevYear[@"Income (Loss) Including Minority Interest"];
			var totalAssetsPY = normalizedFinForPrevYear[@"Total Assets"];			
			var ratio2 = incomePY / totalAssetsPY;

			return ratio1 - ratio2 > 0 ? 1 : 0;
		}

		private int OperatingCashFlow(Dictionary<string, long> normalizedFinForYear)
		{
			var cashFromOperatingActivities = normalizedFinForYear[@"Cash from Operating Activities"];
			return cashFromOperatingActivities > 0 ? 1 : 0;
		}

		private int RetrunOnAssets(Dictionary<string, long> normalizedFinForYear)
		{
			var income = normalizedFinForYear[@"Income (Loss) Including Minority Interest"];
			var totalAssets = normalizedFinForYear[@"Total Assets"];
			return income / totalAssets > 0 ? 1 : 0;
		}

		private static long ExtractChosenValue(List<CompanyFinancialsMd> financial, int year, string key, StatementType statementType, int tid = 0)
		{
			var valueChosen = (from rcd in financial.Where(i => i.Statement == statementType && i.FYear == year)
							   select (rcd.Values.Find(a => a.StandardisedName.Equals(key)).ValueChosen)).FirstOrDefault();
			if (tid != 0)
			{
				valueChosen = (from rcd in financial.Where(i => i.Statement == statementType && i.FYear == year)
							   select (rcd.Values.Find(a => a.StandardisedName.Equals(key) && a.Tid == tid).ValueChosen)).FirstOrDefault();
			}

			return valueChosen == null ? 0 : (long)valueChosen;
		}

		private void ComputeEBITDA(Dictionary<int, Dictionary<string, long>> normalizedFin, Dictionary<int, long> ebitdaLst)
		{
			foreach (var year in normalizedFin.Keys)
			{
				var operatingIncome = normalizedFin[year][@"Operating Income (Loss)"];
				var depreciationAmortization = normalizedFin[year][@"Depreciation & Amortization"];
				ebitdaLst.Add(year, operatingIncome + depreciationAmortization);
			}
			return;
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

		private Dictionary<int, Dictionary<string, long>> FlattenData(List<CompanyFinancialsMd> financial)
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

	}
}