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
		private readonly HandleSharesOutStanding _hos;
		private readonly ILogger<AnalyzeFinancial> _logger;

		public AnalyzeFinancial(IDBConnectionHandler<CompanyFinancialsMd> dbconCompany,
			ILogger<AnalyzeFinancial> logger,
			HandleCompanyList hcl,
			HandleSharesOutStanding hos)
		{
			_dbconCompany = dbconCompany;
			_logger = logger;
			_hcl = hcl;
			_hos = hos;
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
			var returnValue = FlattenData(financial);
			var ebitdaLst = new Dictionary<int, long>();
			var pietroskiScores = new Dictionary<int, int>();
			switch (companyBasicInfo.IndustryTemplate)
			{
				case "general":
					ComputeEBITDA(returnValue, ebitdaLst);
					ComputePietroskiScore(returnValue, pietroskiScores, simId);
					break;

				default:
					returnValue = null;
					break;
			}
			foreach (var ebitd in ebitdaLst.OrderByDescending(a=>a.Key))
			{
				Console.WriteLine($"EBITDA for {companyBasicInfo.Name} for year {ebitd.Key} is {ebitd.Value}");
			}
			foreach (var pietroskiScore in pietroskiScores.OrderByDescending(a=>a.Key))
			{
				Console.WriteLine($"Piotroski score for {companyBasicInfo.Name} for year {pietroskiScore.Key} is {pietroskiScore.Value}");
			}
			return returnValue;
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

		private int Accruals(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var cashFromOperatingActivitiesCY = (float)normalizedFinForYear[@"Cash from Operating Activities"];
			var totalAssetsCY = (float)normalizedFinForYear[@"Total Assets"];
			var ratio1 = cashFromOperatingActivitiesCY / totalAssetsCY;
			//PY prior year
			var cashFromOperatingActivitiesPY = (float)normalizedFinForPrevYear[@"Cash from Operating Activities"];
			var totalAssetsPY = (float)normalizedFinForPrevYear[@"Total Assets"];
			var ratio2 = cashFromOperatingActivitiesPY / totalAssetsPY;
			return ratio1 - ratio2 > 0 ? 1 : 0;
		}

		private int ChangeInAssetTurnoverRatio(Dictionary<string, long> normalizedFinForYear,
			Dictionary<string, long> normalizedFinForPrevYear,
			Dictionary<string, long> normalizedFinForTwoPrevYear)
		{
			var revenueCY = (float)normalizedFinForYear[@"Revenue"];
			var revenuePY = (float)normalizedFinForPrevYear[@"Revenue"];
			var totalAssetsCur = (float)normalizedFinForYear[@"Total Assets"];
			var totalAssetsPrev = (float)normalizedFinForPrevYear[@"Total Assets"];
			var totalAssetsTwoPrev = (float)normalizedFinForTwoPrevYear[@"Total Assets"];

			//calculations.
			var averageAssetsCV = (totalAssetsCur + totalAssetsPrev) / 2;
			var averageAssetsPV = (totalAssetsPrev + totalAssetsTwoPrev) / 2;
			var ratio1 = revenueCY / averageAssetsCV;
			var ratio2 = revenuePY / averageAssetsPV;
			return ratio1 - ratio2 >= 0 ? 1 : 0;
		}

		private int ChangeInCurrentRatio(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var totalCurrentAssetsCY = (float)normalizedFinForYear[@"Total Current Assets"];
			var totalCurrentLiabilitiesCY = (float)normalizedFinForYear[@"Total Current Liabilities"];
			var ratio1 = totalCurrentAssetsCY / totalCurrentLiabilitiesCY;

			//PY prior year
			var totalCurrentAssetsPY = (float)normalizedFinForPrevYear[@"Total Current Assets"];
			var totalCurrentLiabilitiesPY = (float)normalizedFinForPrevYear[@"Total Current Liabilities"];
			var ratio2 = totalCurrentAssetsPY / totalCurrentLiabilitiesPY;
			return ratio1 - ratio2 > 0 ? 1 : 0;
		}

		private int ChangeInGrossMargin(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var grossProfitCY = (float)normalizedFinForYear[@"Gross Profit"];
			var revenueCY = (float)normalizedFinForYear[@"Revenue"];
			var ratio1 = grossProfitCY / revenueCY;

			//PY prior year
			var grossProfitPY = (float)normalizedFinForPrevYear[@"Gross Profit"];
			var revenuePY = (float)normalizedFinForPrevYear[@"Revenue"];
			var ratio2 = grossProfitPY / revenuePY;
			return ratio1 - ratio2 > 0 ? 1 : 0;
		}

		private int ChangeInLeverage(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var longTermDebtCY = (float)normalizedFinForYear[@"Long Term Debt"];
			var totalAssetsCY = (float)normalizedFinForYear[@"Total Assets"];
			var ratio1 = longTermDebtCY / totalAssetsCY;

			//PY prior year
			var longTermDebtPY = (float)normalizedFinForPrevYear[@"Long Term Debt"];
			var totalAssetsPY = (float)normalizedFinForPrevYear[@"Total Assets"];
			var ratio2 = longTermDebtPY / totalAssetsPY;
			return ratio1 - ratio2 < 0 ? 1 : 0;
		}

		private int ChangeInNumberOfShares(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear, string simId, int year, int prevYear)
		{
			var outstandingShares = _hos.GetOutStandingShares(simId).Result;
			if (outstandingShares == null ||
				!outstandingShares.OutstandingValues.Any() ||
				outstandingShares.OutstandingValues.Where(o => o.Fyear == prevYear).FirstOrDefault() == null)
			{
				return 1;
			}

			var prevYearValue = outstandingShares.OutstandingValues.Where(o => o.Fyear == prevYear).FirstOrDefault().Value;
			long? currentYearValue =
			outstandingShares.OutstandingValues.Where(o => o.Fyear == year).FirstOrDefault() != null ?
				outstandingShares.OutstandingValues.Where(o => o.Fyear == year).FirstOrDefault().Value : prevYearValue;
			
			if (currentYearValue == null || prevYearValue == null)
			{
				return 0;
			}
			return currentYearValue - prevYearValue <= 0 ? 1 : 0;
		}

		private int ChangeInReturnOfAssets(Dictionary<string, long> normalizedFinForYear, Dictionary<string, long> normalizedFinForPrevYear)
		{
			//CY current year
			var incomeCY = (float)normalizedFinForYear[@"Income (Loss) Including Minority Interest"];
			var totalAssetsCY = (float)normalizedFinForYear[@"Total Assets"];
			var ratio1 = incomeCY / totalAssetsCY;
			//PY prior year
			var incomePY = (float)normalizedFinForPrevYear[@"Income (Loss) Including Minority Interest"];
			var totalAssetsPY = (float)normalizedFinForPrevYear[@"Total Assets"];
			var ratio2 = incomePY / totalAssetsPY;

			return ratio1 - ratio2 > 0 ? 1 : 0;
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

		private void ComputePietroskiScore(Dictionary<int, Dictionary<string, long>> normalizedFin, Dictionary<int, int> pietroskiScores, string simId)
		{
			foreach (var year in normalizedFin.Keys)
			{
				var totalScore = 0;
				var normalizedFinForYear = normalizedFin[year];
				var doWeHavePreviousYear = normalizedFin.ContainsKey(year - 1);
				var normalizedFinForPrevYear = doWeHavePreviousYear ? normalizedFin[year - 1] : normalizedFin[year];
				var normalizedFinForTwoPrevYear = normalizedFin.ContainsKey(year - 2) ? normalizedFin[year - 2] : normalizedFinForPrevYear;
				totalScore += RetrunOnAssets(normalizedFinForYear);
				totalScore += OperatingCashFlow(normalizedFinForYear);
				totalScore += ChangeInReturnOfAssets(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += Accruals(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInLeverage(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInCurrentRatio(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInNumberOfShares(normalizedFinForYear, normalizedFinForPrevYear, simId: simId, year: year, prevYear: doWeHavePreviousYear ? year - 1 : year);
				totalScore += ChangeInGrossMargin(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInAssetTurnoverRatio(normalizedFinForYear, normalizedFinForPrevYear, normalizedFinForTwoPrevYear);
				pietroskiScores.Add(year, totalScore);
			}			
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

		private int OperatingCashFlow(Dictionary<string, long> normalizedFinForYear)
		{
			var cashFromOperatingActivities = (float)normalizedFinForYear[@"Cash from Operating Activities"];
			return cashFromOperatingActivities > 0 ? 1 : 0;
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

		private int RetrunOnAssets(Dictionary<string, long> normalizedFinForYear)
		{
			var income = (float)normalizedFinForYear[@"Income (Loss) Including Minority Interest"];
			var totalAssets = (float)normalizedFinForYear[@"Total Assets"];
			return income / totalAssets > 0 ? 1 : 0;
		}

	}
}