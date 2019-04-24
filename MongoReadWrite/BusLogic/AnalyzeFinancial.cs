using AutoMapper;
using Microsoft.Extensions.Logging;
using Models;
using MongoDB.Driver;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite.BusLogic
{
	public class AnalyzeFinancial
	{

		#region Private Fields

		private readonly IDBConnectionHandler<CompanyFinancialsMd> _dbconCompany;
		private readonly IDBConnectionHandler<PiotroskiScoreMd> _dbpiScore;
		private readonly IMongoCollection<PiotroskiScoreMd> _dbpiScoreConnection;
		private readonly HandleCompanyList _hcl;
		private readonly HandleSharesOutStanding _hos;
		private readonly ILogger<AnalyzeFinancial> _logger;
		private readonly IMongoCollection<CompanyFinancialsMd> _statementConnection;

		#endregion Private Fields


		#region Public Constructors

		public AnalyzeFinancial(IDBConnectionHandler<CompanyFinancialsMd> dbconCompany,
			IDBConnectionHandler<PiotroskiScoreMd> dbpiScore,
			ILogger<AnalyzeFinancial> logger,
			HandleCompanyList hcl,
			HandleSharesOutStanding hos)
		{
			_dbconCompany = dbconCompany;
			_statementConnection = _dbconCompany.ConnectToDatabase("CompanyFinancials");

			_dbpiScore = dbpiScore;
			_dbpiScoreConnection = _dbpiScore.ConnectToDatabase("PiotroskiScore");

			_logger = logger;
			_hcl = hcl;
			_hos = hos;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<bool> ComputeScoresAsync(string simId)
		{
			var companyBasicInfo = _hcl.GetCompanyDetails(simId);
			if (companyBasicInfo == null)
			{
				_logger.LogError("Basic information about the requested firm is not available.");
				return false;
			}
			if (companyBasicInfo.LastUpdate > DateTime.Now)
			{
				_logger.LogError($"Data for {companyBasicInfo.SimId} cannot be evaluated now");
				return false;

			}
			var oldComputeValues = _dbpiScore.Get(r => r.SimId.Equals(simId)).ToList();
			if (oldComputeValues.Any())
			{
				var firstOldCompute = oldComputeValues.OrderByDescending(a => a.LastUpdate).FirstOrDefault();
				if (firstOldCompute.LastUpdate > companyBasicInfo.LastUpdate)
				{
					_logger.LogInformation($"{simId} already evaluated");
					return true;
				}
			}
			var industryTemplate = companyBasicInfo.IndustryTemplate;
			var financial = _dbconCompany.Get(cf => cf.SimId.Equals(simId)).ToList();
			if (financial == null || !financial.Any())
			{
				_logger.LogWarning($"Financial values for {companyBasicInfo.Name} is not stored in database yet ");
				return false;
			}
			if (EvaluateStoredValues(companyBasicInfo, financial) == false)
			{
				await DeleteFinancialAsync(financial);
				return false;
			}
			var flattendData = FlattenData(financial);
			if (flattendData == null)
			{
				return false;
			}
			var ebitdaLst = new Dictionary<int, long>();
			var pietroskiScores = new Dictionary<int, int>();
			var profitablityRatios = new Dictionary<string, decimal>();
			switch (companyBasicInfo.IndustryTemplate)
			{
				case "general":
					ComputeEBITDA(flattendData, ebitdaLst);
					ComputePietroskiScore(flattendData, pietroskiScores, simId);
					ComputeProfitablity(flattendData, profitablityRatios);
					var v = await StoreComputedValuesAsync(simId, ebitdaLst, pietroskiScores, profitablityRatios);
					break;

				default:
					flattendData = null;
					break;
			}
			Console.WriteLine($"Computed EBITDA for {companyBasicInfo.Name} is as follows:");
			foreach (var ebitd in ebitdaLst.OrderByDescending(a => a.Key))
			{
				Console.WriteLine($"{ebitd.Key} => {((decimal)ebitd.Value).ToKMB()}");
			}
			Console.WriteLine();
			Console.WriteLine("Piotroski f scores");
			foreach (var pietroskiScore in pietroskiScores.OrderByDescending(a => a.Key))
			{
				Console.WriteLine($"{pietroskiScore.Key} => {pietroskiScore.Value}");
			}
			Console.WriteLine($"Profitability Ratios for {companyBasicInfo.Name}");
			foreach (var key in profitablityRatios)
			{
				Console.WriteLine($"{key.Key} => {key.Value}%");
			}
			return true;
		}

		#endregion Public Methods


		#region Private Methods

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
			//sorted Normalized Fin by year
			normalizedFin = normalizedFin.OrderByDescending(a1 => a1.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
			foreach (var year in normalizedFin.Keys)
			{
				var totalScore = 0;
				var normalizedFinForYear = normalizedFin[year];
				var doWeHavePreviousYear = normalizedFin.ContainsKey(year - 1);
				var normalizedFinForPrevYear = doWeHavePreviousYear ? normalizedFin[year - 1] : normalizedFin[year];
				var normalizedFinForTwoPrevYear = normalizedFin.ContainsKey(year - 2) ? normalizedFin[year - 2] : normalizedFinForPrevYear;
				//Check profitability
				totalScore += RetrunOnAssets(normalizedFinForYear);
				totalScore += OperatingCashFlow(normalizedFinForYear);
				totalScore += ChangeInReturnOfAssets(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += Accruals(normalizedFinForYear, normalizedFinForPrevYear);
				// Test on Leverage, Liquidity and source of finds
				totalScore += ChangeInLeverage(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInCurrentRatio(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInNumberOfShares(normalizedFinForYear, normalizedFinForPrevYear, simId: simId, year: year, prevYear: doWeHavePreviousYear ? year - 1 : year);
				// Operating Efficiency
				totalScore += ChangeInGrossMargin(normalizedFinForYear, normalizedFinForPrevYear);
				totalScore += ChangeInAssetTurnoverRatio(normalizedFinForYear, normalizedFinForPrevYear, normalizedFinForTwoPrevYear);
				pietroskiScores.Add(year, totalScore);
			}
		}

		private void ComputeProfitablity(Dictionary<int, Dictionary<string, long>> normalizedFin, Dictionary<string, decimal> profitablityRatios)
		{
			var normalizedFinForYear = normalizedFin[normalizedFin.Keys.Max()];
			var grossProfit = (decimal)normalizedFinForYear[@"Gross Profit"];
			var operatingIncome = (decimal)normalizedFinForYear[@"Operating Income (Loss)"];
			var income = (decimal)normalizedFinForYear[@"Income (Loss) Including Minority Interest"];

			var revenue = (decimal)normalizedFinForYear[@"Revenue"];
			var totalEquity = (decimal)normalizedFinForYear[@"Total Equity"];
			var totalAssets = (decimal)normalizedFinForYear[@"Total Assets"];

			var grossMargin = revenue == 0 ? 0 : Math.Round(100 * grossProfit / revenue, 2);
			var operatingMargin = revenue == 0 ? 0 : Math.Round(100 * operatingIncome / revenue, 2);
			var netProfitMargin = revenue == 0 ? 0 : Math.Round(100 * income / revenue, 2);
			var returnOnEquity = totalEquity == 0 ? 0 : Math.Round(100 * income / totalEquity, 2);
			var returnOnAssets = totalAssets == 0 ? 0 : Math.Round(100 * income / totalAssets, 2);
			profitablityRatios.Add("Gross Margin", grossMargin);
			profitablityRatios.Add("Operating Margin", operatingMargin);
			profitablityRatios.Add("Net Profit Margin", netProfitMargin);
			profitablityRatios.Add("Return on Equity", returnOnEquity);
			profitablityRatios.Add("Return on Assets", returnOnAssets);

			return;
		}

		private async Task DeleteFinancialAsync(List<CompanyFinancialsMd> financial)
		{
			foreach (var financialsMd in financial)
			{
				await _dbconCompany.Remove(financialsMd.Id);
			}
		}

		private bool EvaluateStoredValues(CompanyDetail companyBasicInfo, List<CompanyFinancialsMd> financial)
		{
			if (companyBasicInfo == null)
			{
				_logger.LogError("Basic information about the requested firm is not available.");

				return false;
			}
			var latestYear = financial.Select(x => x.FYear).Max();
			var simId = financial.First().SimId;
			var currentYear = DateTime.Now.Year;
			if (currentYear - 1 > latestYear)
			{
				_logger.LogError($"Data not updated too long for {simId}");
				return false;
			}
			

			var latestBS = financial.Where(x => x.Statement == StatementType.BalanceSheet).Select(x => x.FYear).Max();
			var latestCF = financial.Where(x => x.Statement == StatementType.CashFlow).Select(x => x.FYear).Max();
			var latestPL = financial.Where(x => x.Statement == StatementType.ProfitLoss).Select(x => x.FYear).Max();
			if (latestYear != latestBS
				|| latestYear != latestCF
				|| latestYear != latestPL)
			{
				_logger.LogError($"Inconstant last reporting date  for {simId}");
				return false;
			}
			var earliestBS = financial.Where(x => x.Statement == StatementType.BalanceSheet).Select(x => x.FYear).Min();
			var earliestCF = financial.Where(x => x.Statement == StatementType.CashFlow).Select(x => x.FYear).Min();
			var earliestPL = financial.Where(x => x.Statement == StatementType.ProfitLoss).Select(x => x.FYear).Min();
			var earliestYear = financial.Select(x => x.FYear).Min();
			if (earliestYear != earliestBS
				|| earliestYear != earliestCF
				|| earliestYear != earliestPL)
			{
				_logger.LogError($"Inconstant early reporting data  for {simId}");
				return false;
			}
			return true;
		}

		private string[] ExtractKeys(IEnumerable<CompanyFinancialsMd> firstYearData, StatementType statementType)
		{
			try
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
			catch (Exception ex)
			{
				_logger.LogError($"Error in extracting keys for SimId:{firstYearData.First().SimId}");
				_logger.LogError(ex.Message);
				return null;
			}
		}

		private Dictionary<int, Dictionary<string, long>> FlattenData(List<CompanyFinancialsMd> financial)
		{
			var valueRef = new Dictionary<int, Dictionary<string, long>>();
			var years = financial.Select(x => x.FYear).Distinct();
			var firstYearData = financial.Where(x => x.FYear == years.First());
			string[] bsKeys = ExtractKeys(firstYearData, StatementType.BalanceSheet);
			string[] cfKeys = ExtractKeys(firstYearData, StatementType.CashFlow);
			string[] plKeys = ExtractKeys(firstYearData, StatementType.ProfitLoss);
			if (bsKeys == null || cfKeys == null || plKeys == null)
			{
				return null;
			}
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

		private async Task<bool> StoreComputedValuesAsync(string simId, Dictionary<int, long> ebitdaLst, Dictionary<int, int> pietroskiScores, Dictionary<string, decimal> profitablityRatios)
		{
			var oldValues = _dbpiScore.Get(r => r.SimId == simId).ToList();
			if (oldValues == null)
			{
				oldValues = new List<PiotroskiScoreMd>();
			}
			//compDetailList = Mapper.Map<List<CompanyDetailMd>, List<CompanyDetail>>(savedValue);
			var newValues = (from fYear in pietroskiScores.Keys
							 select new PiotroskiScore
							 {
								 SimId = simId,
								 FYear = fYear,
								 Rating = pietroskiScores[fYear],
								 ProfitablityRatios = profitablityRatios,
								 EBITDA = ebitdaLst[fYear],
								 LastUpdate = DateTime.Now
							 }).ToList();
			var valuesToStoreInDb = new List<PiotroskiScoreMd>();
			foreach (var (piotroskiScore, oldValue) in from piotroskiScore in newValues
													   let oldValue = oldValues.Find(a => a.SimId == piotroskiScore.SimId && a.FYear == piotroskiScore.FYear)
													   select (piotroskiScore, oldValue))
			{
				if (oldValue == null)
				{
					valuesToStoreInDb.Add(Mapper.Map<PiotroskiScoreMd>(piotroskiScore));
				}
				else
				{
					var oldId = oldValue.Id;
					valuesToStoreInDb.Add(Mapper.Map<PiotroskiScoreMd>(piotroskiScore));
					valuesToStoreInDb.Last().Id = oldId;
				}
			}
			try
			{
				foreach (var oldValue in from oldValue in oldValues
										 let updated = valuesToStoreInDb.Find(o => o.Id == oldValue.Id)
										 where updated == null
										 select oldValue)
				{
					await _dbconCompany.Remove(oldValue.Id);
				}
				var returnValue = await _dbpiScore.UpdateMultiple(valuesToStoreInDb);

				return returnValue;
			}
			catch (Exception ex)
			{
				_logger.LogError("Could not update scores");
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
			}
			return false;
		}

		#endregion Private Methods
	}
}