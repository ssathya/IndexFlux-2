using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelReadWrite.Tools
{
	public class WriteFinancials
	{

		#region Private Fields

		private const string cfWorkSheetName = "Cash-Flow";
		private const string GeneralBsWorkSheetName = "General-Balance-Sheets";
		private const string InsuranceBsWorkSheetName = "Insurance-Balance-Sheets";
		private const string BankBsWorkSheetName = "Bank-Balance-Sheets";
		private const string plWorkSheetName = "Profit-Loss";
		private readonly ILogger _logger;

		#endregion Private Fields


		#region Public Constructors

		public WriteFinancials(ILogger logger)
		{
			_logger = logger;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<bool> UpdateWorkSheetForCompanyAsync(string SimId, string outFile)
		{
			if (string.IsNullOrWhiteSpace(SimId))
			{
				return false;
			}
			var companyFinancials = await ObtainCompanyFinancilasAsync(SimId);

			if (companyFinancials == null || companyFinancials.Count == 0)
			{
				return false;
			}
			var balanceSheets = companyFinancials.Where(cf => cf.Statement == StatementType.BalanceSheet).ToList();
			await WriteBalanceSheetDataAsync(balanceSheets, outFile);
			return true;
		}

		#endregion Public Methods


		#region Private Methods

		private ExcelWorksheet CreateSheet(string outFile, ExcelPackage package, string sheetName)
		{
			using (var outStream = new FileStream(outFile, FileMode.OpenOrCreate))
			{
				package.Load(outStream);
			}
			var worksheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name == sheetName);
			if (worksheet == null)
			{
				worksheet = package.Workbook.Worksheets.Add(sheetName);
			}

			return worksheet;
		}

		private async Task<List<CompanyFinancials>> ObtainCompanyFinancilasAsync(string SimId)
		{
			var loS = new ListOfStatements(_logger);
			StatementList statementList = await loS.FetchStatementList(SimId, IdentifyerType.SimFinId);
			statementList = loS.ExtractYearEndReports(statementList);
			if (statementList.Bs.Count == 0 || statementList.Cf.Count == 0
				|| statementList.Pl.Count == 0)
			{
				return null;
			}
			statementList.Bs = statementList.Bs.OrderByDescending(b => b.Fyear).Take(4).ToList();
			statementList.Cf = statementList.Cf.OrderByDescending(c => c.Fyear).Take(4).ToList();
			statementList.Pl = statementList.Pl.OrderByDescending(p => p.Fyear).Take(4).ToList();

			var dri = new DownloadReportableItems(_logger);
			var companyFinancials = await dri.DownloadFinancialsAsync(statementList);
			return companyFinancials;
		}

		private async Task PopulateGeneralBSAsync(List<CompanyFinancials> balanceSheets, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, GeneralBsWorkSheetName);
			var gbsl = new List<GeneralBalanceSheet>();
			foreach (var balanceSheet in balanceSheets)
			{
				var fbc = new GeneralBalanceSheet
				{
					Calculated = balanceSheet.Calculated,
					CompanyId = balanceSheet.CompanyId,
					FYear = balanceSheet.FYear,
					IndustryTemplate = balanceSheet.IndustryTemplate,
					Statement = balanceSheet.Statement,
				};
				fbc.CashCashEquivalentsShortTermInvestments_1 = balanceSheet.Values.Find(a => a.StandardisedName == "Cash, Cash Equivalents & Short Term Investments" && a.Tid == 1).ValueChosen;
				fbc.CashCashEquivalentsShortTermInvestments_1 = balanceSheet.Values.Find(a => a.StandardisedName == "Cash, Cash Equivalents & Short Term Investments" && a.Tid == 1).ValueChosen;
				fbc.CashCashEquivalents_2 = balanceSheet.Values.Find(a => a.StandardisedName == "Cash & Cash Equivalents" && a.Tid == 2).ValueChosen;
				fbc.ShortTermInvestments_3 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Investments" && a.Tid == 3).ValueChosen;
				fbc.AccountsNotesReceivable_4 = balanceSheet.Values.Find(a => a.StandardisedName == "Accounts & Notes Receivable" && a.Tid == 4).ValueChosen;
				fbc.AccountsReceivableNet_5 = balanceSheet.Values.Find(a => a.StandardisedName == "Accounts Receivable, Net" && a.Tid == 5).ValueChosen;
				fbc.NotesReceivableNet_6 = balanceSheet.Values.Find(a => a.StandardisedName == "Notes Receivable, Net" && a.Tid == 6).ValueChosen;
				fbc.UnbilledRevenues_7 = balanceSheet.Values.Find(a => a.StandardisedName == "Unbilled Revenues" && a.Tid == 7).ValueChosen;
				fbc.Inventories_8 = balanceSheet.Values.Find(a => a.StandardisedName == "Inventories" && a.Tid == 8).ValueChosen;
				fbc.RawMaterials_9 = balanceSheet.Values.Find(a => a.StandardisedName == "Raw Materials" && a.Tid == 9).ValueChosen;
				fbc.WorkInProcess_10 = balanceSheet.Values.Find(a => a.StandardisedName == "Work In Process" && a.Tid == 10).ValueChosen;
				fbc.FinishedGoods_11 = balanceSheet.Values.Find(a => a.StandardisedName == "Finished Goods" && a.Tid == 11).ValueChosen;
				fbc.OtherInventory_12 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Inventory" && a.Tid == 12).ValueChosen;
				fbc.OtherShortTermAssets_13 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Short Term Assets" && a.Tid == 13).ValueChosen;
				fbc.PrepaidExpenses_14 = balanceSheet.Values.Find(a => a.StandardisedName == "Prepaid Expenses" && a.Tid == 14).ValueChosen;
				fbc.DerivativeHedgingAssets_15 = balanceSheet.Values.Find(a => a.StandardisedName == "Derivative & Hedging Assets" && a.Tid == 15).ValueChosen;
				fbc.AssetsHeldforSale_16 = balanceSheet.Values.Find(a => a.StandardisedName == "Assets Held-for-Sale" && a.Tid == 16).ValueChosen;
				fbc.DeferredTaxAssets_17 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Tax Assets" && a.Tid == 17).ValueChosen;
				fbc.IncomeTaxesReceivable_18 = balanceSheet.Values.Find(a => a.StandardisedName == "Income Taxes Receivable" && a.Tid == 18).ValueChosen;
				fbc.DiscontinuedOperations_19 = balanceSheet.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 19).ValueChosen;
				fbc.MiscellaneousShortTermAssets_20 = balanceSheet.Values.Find(a => a.StandardisedName == "Miscellaneous Short Term Assets" && a.Tid == 20).ValueChosen;
				fbc.TotalCurrentAssets_21 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Current Assets" && a.Tid == 21).ValueChosen;
				fbc.PropertyPlantEquipmentNet_22 = balanceSheet.Values.Find(a => a.StandardisedName == "Property, Plant & Equipment, Net" && a.Tid == 22).ValueChosen;
				fbc.PropertyPlantEquipment_23 = balanceSheet.Values.Find(a => a.StandardisedName == "Property, Plant & Equipment" && a.Tid == 23).ValueChosen;
				fbc.AccumulatedDepreciation_24 = balanceSheet.Values.Find(a => a.StandardisedName == "Accumulated Depreciation" && a.Tid == 24).ValueChosen;
				fbc.LongTermInvestmentsReceivables_25 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Investments & Receivables" && a.Tid == 25).ValueChosen;
				fbc.LongTermInvestments_26 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Investments" && a.Tid == 26).ValueChosen;
				fbc.LongTermMarketableSecurities_27 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Marketable Securities" && a.Tid == 27).ValueChosen;
				fbc.LongTermReceivables_28 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Receivables" && a.Tid == 28).ValueChosen;
				fbc.OtherLongTermAssets_29 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Long Term Assets" && a.Tid == 29).ValueChosen;
				fbc.IntangibleAssets_30 = balanceSheet.Values.Find(a => a.StandardisedName == "Intangible Assets" && a.Tid == 30).ValueChosen;
				fbc.Goodwill_31 = balanceSheet.Values.Find(a => a.StandardisedName == "Goodwill" && a.Tid == 31).ValueChosen;
				fbc.OtherIntangibleAssets_32 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Intangible Assets" && a.Tid == 32).ValueChosen;
				fbc.PrepaidExpense_33 = balanceSheet.Values.Find(a => a.StandardisedName == "Prepaid Expense" && a.Tid == 33).ValueChosen;
				fbc.DeferredTaxAssets_34 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Tax Assets" && a.Tid == 34).ValueChosen;
				fbc.DerivativeHedgingAssets_35 = balanceSheet.Values.Find(a => a.StandardisedName == "Derivative & Hedging Assets" && a.Tid == 35).ValueChosen;
				fbc.PrepaidPensionCosts_36 = balanceSheet.Values.Find(a => a.StandardisedName == "Prepaid Pension Costs" && a.Tid == 36).ValueChosen;
				fbc.DiscontinuedOperations_37 = balanceSheet.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 37).ValueChosen;
				fbc.InvestmentsinAffiliates_38 = balanceSheet.Values.Find(a => a.StandardisedName == "Investments in Affiliates" && a.Tid == 38).ValueChosen;
				fbc.MiscellaneousLongTermAssets_39 = balanceSheet.Values.Find(a => a.StandardisedName == "Miscellaneous Long Term Assets" && a.Tid == 39).ValueChosen;
				fbc.TotalNoncurrentAssets_40 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Noncurrent Assets" && a.Tid == 40).ValueChosen;
				fbc.TotalAssets_41 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Assets" && a.Tid == 41).ValueChosen;
				fbc.PayablesAccruals_42 = balanceSheet.Values.Find(a => a.StandardisedName == "Payables & Accruals" && a.Tid == 42).ValueChosen;
				fbc.AccountsPayable_43 = balanceSheet.Values.Find(a => a.StandardisedName == "Accounts Payable" && a.Tid == 43).ValueChosen;
				fbc.AccruedTaxes_44 = balanceSheet.Values.Find(a => a.StandardisedName == "Accrued Taxes" && a.Tid == 44).ValueChosen;
				fbc.InterestDividendsPayable_45 = balanceSheet.Values.Find(a => a.StandardisedName == "Interest & Dividends Payable" && a.Tid == 45).ValueChosen;
				fbc.OtherPayablesAccruals_46 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Payables & Accruals" && a.Tid == 46).ValueChosen;
				fbc.ShortTermDebt_47 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Debt" && a.Tid == 47).ValueChosen;
				fbc.ShortTermBorrowings_48 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Borrowings" && a.Tid == 48).ValueChosen;
				fbc.ShortTermCapitalLeases_49 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Capital Leases" && a.Tid == 49).ValueChosen;
				fbc.CurrentPortionofLongTermDebt_50 = balanceSheet.Values.Find(a => a.StandardisedName == "Current Portion of Long Term Debt" && a.Tid == 50).ValueChosen;
				fbc.OtherShortTermLiabilities_51 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Short Term Liabilities" && a.Tid == 51).ValueChosen;
				fbc.DeferredRevenue_52 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Revenue" && a.Tid == 52).ValueChosen;
				fbc.DerivativesHedging_53 = balanceSheet.Values.Find(a => a.StandardisedName == "Derivatives & Hedging" && a.Tid == 53).ValueChosen;
				fbc.DeferredTaxLiabilities_54 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Tax Liabilities" && a.Tid == 54).ValueChosen;
				fbc.DiscontinuedOperations_55 = balanceSheet.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 55).ValueChosen;
				fbc.MiscellaneousShortTermLiabilities_56 = balanceSheet.Values.Find(a => a.StandardisedName == "Miscellaneous Short Term Liabilities" && a.Tid == 56).ValueChosen;
				fbc.TotalCurrentLiabilities_57 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Current Liabilities" && a.Tid == 57).ValueChosen;
				fbc.LongTermDebt_58 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Debt" && a.Tid == 58).ValueChosen;
				fbc.LongTermBorrowings_59 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Borrowings" && a.Tid == 59).ValueChosen;
				fbc.LongTermCapitalLeases_60 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Capital Leases" && a.Tid == 60).ValueChosen;
				fbc.OtherLongTermLiabilities_61 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Long Term Liabilities" && a.Tid == 61).ValueChosen;
				fbc.AccruedLiabilities_62 = balanceSheet.Values.Find(a => a.StandardisedName == "Accrued Liabilities" && a.Tid == 62).ValueChosen;
				fbc.PensionLiabilities_63 = balanceSheet.Values.Find(a => a.StandardisedName == "Pension Liabilities" && a.Tid == 63).ValueChosen;
				fbc.Pensions_64 = balanceSheet.Values.Find(a => a.StandardisedName == "Pensions" && a.Tid == 64).ValueChosen;
				fbc.OtherPostRetirementBenefits_65 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Post-Retirement Benefits" && a.Tid == 65).ValueChosen;
				fbc.DeferredCompensation_66 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Compensation" && a.Tid == 66).ValueChosen;
				fbc.DeferredRevenue_67 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Revenue" && a.Tid == 67).ValueChosen;
				fbc.DeferredTaxLiabilities_68 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Tax Liabilities" && a.Tid == 68).ValueChosen;
				fbc.DerivativesHedging_69 = balanceSheet.Values.Find(a => a.StandardisedName == "Derivatives & Hedging" && a.Tid == 69).ValueChosen;
				fbc.DiscontinuedOperations_70 = balanceSheet.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 70).ValueChosen;
				fbc.MiscellaneousLongTermLiabilities_71 = balanceSheet.Values.Find(a => a.StandardisedName == "Miscellaneous Long Term Liabilities" && a.Tid == 71).ValueChosen;
				fbc.TotalNoncurrentLiabilities_72 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Noncurrent Liabilities" && a.Tid == 72).ValueChosen;
				fbc.TotalLiabilities_73 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Liabilities" && a.Tid == 73).ValueChosen;
				fbc.PreferredEquity_74 = balanceSheet.Values.Find(a => a.StandardisedName == "Preferred Equity" && a.Tid == 74).ValueChosen;
				fbc.ShareCapitalAdditionalPaidInCapital_75 = balanceSheet.Values.Find(a => a.StandardisedName == "Share Capital & Additional Paid-In Capital" && a.Tid == 75).ValueChosen;
				fbc.CommonStock_76 = balanceSheet.Values.Find(a => a.StandardisedName == "Common Stock" && a.Tid == 76).ValueChosen;
				fbc.AdditionalPaidinCapital_77 = balanceSheet.Values.Find(a => a.StandardisedName == "Additional Paid in Capital" && a.Tid == 77).ValueChosen;
				fbc.OtherShareCapital_78 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Share Capital" && a.Tid == 78).ValueChosen;
				fbc.TreasuryStock_79 = balanceSheet.Values.Find(a => a.StandardisedName == "Treasury Stock" && a.Tid == 79).ValueChosen;
				fbc.RetainedEarnings_80 = balanceSheet.Values.Find(a => a.StandardisedName == "Retained Earnings" && a.Tid == 80).ValueChosen;
				fbc.OtherEquity_81 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Equity" && a.Tid == 81).ValueChosen;
				fbc.EquityBeforeMinorityInterest_82 = balanceSheet.Values.Find(a => a.StandardisedName == "Equity Before Minority Interest" && a.Tid == 82).ValueChosen;
				fbc.MinorityInterest_83 = balanceSheet.Values.Find(a => a.StandardisedName == "Minority Interest" && a.Tid == 83).ValueChosen;
				fbc.TotalEquity_84 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Equity" && a.Tid == 84).ValueChosen;
				fbc.TotalLiabilitiesEquity_85 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Liabilities & Equity" && a.Tid == 85).ValueChosen;
				gbsl.Add(fbc);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(gbsl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}
		private async Task PopulateBankBSAsync(List<CompanyFinancials> balanceSheets, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, BankBsWorkSheetName);
			var bbsl = new List<BankBalanceSheet>();
			foreach (var balanceSheet in balanceSheets)
			{
				var bbs = new BankBalanceSheet
				{
					Calculated = balanceSheet.Calculated,
					CompanyId = balanceSheet.CompanyId,
					FYear = balanceSheet.FYear,
					IndustryTemplate = balanceSheet.IndustryTemplate,
					Statement = balanceSheet.Statement
				};
				bbs.CashCashEquivalents_1 = balanceSheet.Values.Find(a => a.StandardisedName == "Cash & Cash Equivalents" && a.Tid == 1).ValueAssigned;
				bbs.Interbankassets_2 = balanceSheet.Values.Find(a => a.StandardisedName == "Interbank assets" && a.Tid == 2).ValueAssigned;
				bbs.FedFundsSoldRepos_3 = balanceSheet.Values.Find(a => a.StandardisedName == "Fed Funds Sold & Repos" && a.Tid == 3).ValueAssigned;
				bbs.OtherInterbankAssets_4 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Interbank Assets" && a.Tid == 4).ValueAssigned;
				bbs.ShortandLongTermInvestments_5 = balanceSheet.Values.Find(a => a.StandardisedName == "Short and Long Term Investments" && a.Tid == 5).ValueAssigned;
				bbs.TradingSecurities_6 = balanceSheet.Values.Find(a => a.StandardisedName == "Trading Securities" && a.Tid == 6).ValueAssigned;
				bbs.InvestmentSecuritiesAvailableforSale_7 = balanceSheet.Values.Find(a => a.StandardisedName == "Investment Securities Available for Sale" && a.Tid == 7).ValueAssigned;
				bbs.InvestmentSecuritiesHeldtoMaturity_8 = balanceSheet.Values.Find(a => a.StandardisedName == "Investment Securities Held to Maturity" && a.Tid == 8).ValueAssigned;
				bbs.RealEstateInvestments_9 = balanceSheet.Values.Find(a => a.StandardisedName == "Real Estate Investments" && a.Tid == 9).ValueAssigned;
				bbs.OtherInvestments_10 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Investments" && a.Tid == 10).ValueAssigned;
				bbs.NetReceivables_11 = balanceSheet.Values.Find(a => a.StandardisedName == "Net Receivables" && a.Tid == 11).ValueAssigned;
				bbs.NetLoans_25 = balanceSheet.Values.Find(a => a.StandardisedName == "Net Loans" && a.Tid == 25).ValueAssigned;
				bbs.ReserveforLoanLosses_24 = balanceSheet.Values.Find(a => a.StandardisedName == "Reserve for Loan Losses" && a.Tid == 24).ValueAssigned;
				bbs.TotalLoans_23 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Loans" && a.Tid == 23).ValueAssigned;
				bbs.TotalCommercialLoans_12 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Commercial Loans" && a.Tid == 12).ValueAssigned;
				bbs.CommercialRealEstateLoans_13 = balanceSheet.Values.Find(a => a.StandardisedName == "Commercial Real Estate Loans" && a.Tid == 13).ValueAssigned;
				bbs.OtherCommercialLoans_14 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Commercial Loans" && a.Tid == 14).ValueAssigned;
				bbs.TotalConsumerLoans_15 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Consumer Loans" && a.Tid == 15).ValueAssigned;
				bbs.CreditCardLoans_16 = balanceSheet.Values.Find(a => a.StandardisedName == "Credit Card Loans" && a.Tid == 16).ValueAssigned;
				bbs.HomeEquityLoans_17 = balanceSheet.Values.Find(a => a.StandardisedName == "Home Equity Loans" && a.Tid == 17).ValueAssigned;
				bbs.FamilyResidentialLoans_18 = balanceSheet.Values.Find(a => a.StandardisedName == "Family Residential Loans" && a.Tid == 18).ValueAssigned;
				bbs.AutoLoans_19 = balanceSheet.Values.Find(a => a.StandardisedName == "Auto Loans" && a.Tid == 19).ValueAssigned;
				bbs.StudentLoans_20 = balanceSheet.Values.Find(a => a.StandardisedName == "Student Loans" && a.Tid == 20).ValueAssigned;
				bbs.OtherConsumerLoans_21 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Consumer Loans" && a.Tid == 21).ValueAssigned;
				bbs.OtherLoans_22 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Loans" && a.Tid == 22).ValueAssigned;
				bbs.NetFixedAssets_26 = balanceSheet.Values.Find(a => a.StandardisedName == "Net Fixed Assets" && a.Tid == 26).ValueAssigned;
				bbs.PropertyPlantEquipmentNet_27 = balanceSheet.Values.Find(a => a.StandardisedName == "Property, Plant & Equipment, Net" && a.Tid == 27).ValueAssigned;
				bbs.OperatingLeaseAssets_28 = balanceSheet.Values.Find(a => a.StandardisedName == "Operating Lease Assets" && a.Tid == 28).ValueAssigned;
				bbs.Otherfixedassets_75 = balanceSheet.Values.Find(a => a.StandardisedName == "Other fixed assets" && a.Tid == 75).ValueAssigned;
				bbs.IntangibleAssets_29 = balanceSheet.Values.Find(a => a.StandardisedName == "Intangible Assets" && a.Tid == 29).ValueAssigned;
				bbs.Goodwill_30 = balanceSheet.Values.Find(a => a.StandardisedName == "Goodwill" && a.Tid == 30).ValueAssigned;
				bbs.OtherIntangibleAssets_31 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Intangible Assets" && a.Tid == 31).ValueAssigned;
				bbs.InvestmentsinAssociates_32 = balanceSheet.Values.Find(a => a.StandardisedName == "Investments in Associates" && a.Tid == 32).ValueAssigned;
				bbs.DeferredTaxAssets_33 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Tax Assets" && a.Tid == 33).ValueAssigned;
				bbs.DerivativesHedging_34 = balanceSheet.Values.Find(a => a.StandardisedName == "Derivatives & Hedging" && a.Tid == 34).ValueAssigned;
				bbs.DiscontinuedOperations_35 = balanceSheet.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 35).ValueAssigned;
				bbs.CustomerAcceptancesLiabilities_36 = balanceSheet.Values.Find(a => a.StandardisedName == "Customer Acceptances & Liabilities" && a.Tid == 36).ValueAssigned;
				bbs.OtherAssets_37 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Assets" && a.Tid == 37).ValueAssigned;
				bbs.TotalAssets_38 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Assets" && a.Tid == 38).ValueAssigned;
				bbs.TotalDeposits_44 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Deposits" && a.Tid == 44).ValueAssigned;
				bbs.DemandDeposits_39 = balanceSheet.Values.Find(a => a.StandardisedName == "Demand Deposits" && a.Tid == 39).ValueAssigned;
				bbs.InterestBearingDeposits_40 = balanceSheet.Values.Find(a => a.StandardisedName == "Interest Bearing Deposits" && a.Tid == 40).ValueAssigned;
				bbs.SavingDeposits_41 = balanceSheet.Values.Find(a => a.StandardisedName == "Saving Deposits" && a.Tid == 41).ValueAssigned;
				bbs.TimeDeposits_42 = balanceSheet.Values.Find(a => a.StandardisedName == "Time Deposits" && a.Tid == 42).ValueAssigned;
				bbs.OtherDeposits_43 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Deposits" && a.Tid == 43).ValueAssigned;
				bbs.ShortTermBorrowingsRepos_45 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Borrowings & Repos" && a.Tid == 45).ValueAssigned;
				bbs.SecuritiesSoldUnderRepo_46 = balanceSheet.Values.Find(a => a.StandardisedName == "Securities Sold Under Repo" && a.Tid == 46).ValueAssigned;
				bbs.TradingAccountLiabilities_47 = balanceSheet.Values.Find(a => a.StandardisedName == "Trading Account Liabilities" && a.Tid == 47).ValueAssigned;
				bbs.ShortTermCapitalLeases_48 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Capital Leases" && a.Tid == 48).ValueAssigned;
				bbs.CurrentPortionofLongTermDebt_49 = balanceSheet.Values.Find(a => a.StandardisedName == "Current Portion of Long Term Debt" && a.Tid == 49).ValueAssigned;
				bbs.ShortTermBorrowings_50 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Borrowings" && a.Tid == 50).ValueAssigned;
				bbs.PayablesBrokerDealers_51 = balanceSheet.Values.Find(a => a.StandardisedName == "Payables Broker Dealers" && a.Tid == 51).ValueAssigned;
				bbs.LongTermDebt_52 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Debt" && a.Tid == 52).ValueAssigned;
				bbs.LongTermCapitalLeases_53 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Capital Leases" && a.Tid == 53).ValueAssigned;
				bbs.LongTermBorrowings_54 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Borrowings" && a.Tid == 54).ValueAssigned;
				bbs.PensionLiabilities_55 = balanceSheet.Values.Find(a => a.StandardisedName == "Pension Liabilities" && a.Tid == 55).ValueAssigned;
				bbs.Pensions_56 = balanceSheet.Values.Find(a => a.StandardisedName == "Pensions" && a.Tid == 56).ValueAssigned;
				bbs.OtherPostRetirementBenefits_57 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Post-Retirement Benefits" && a.Tid == 57).ValueAssigned;
				bbs.DeferredTaxLiabilities_58 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Tax Liabilities" && a.Tid == 58).ValueAssigned;
				bbs.DerivativesHedging_59 = balanceSheet.Values.Find(a => a.StandardisedName == "Derivatives & Hedging" && a.Tid == 59).ValueAssigned;
				bbs.DiscontinuedOperations_60 = balanceSheet.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 60).ValueAssigned;
				bbs.OtherLiabilities_61 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Liabilities" && a.Tid == 61).ValueAssigned;
				bbs.TotalLiabilities_62 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Liabilities" && a.Tid == 62).ValueAssigned;
				bbs.PreferredEquity_63 = balanceSheet.Values.Find(a => a.StandardisedName == "Preferred Equity" && a.Tid == 63).ValueAssigned;
				bbs.ShareCapitalAdditionalPaidInCapital_64 = balanceSheet.Values.Find(a => a.StandardisedName == "Share Capital & Additional Paid-In Capital" && a.Tid == 64).ValueAssigned;
				bbs.CommonStock_65 = balanceSheet.Values.Find(a => a.StandardisedName == "Common Stock" && a.Tid == 65).ValueAssigned;
				bbs.AdditionalPaidinCapital_66 = balanceSheet.Values.Find(a => a.StandardisedName == "Additional Paid in Capital" && a.Tid == 66).ValueAssigned;
				bbs.OtherShareCapital_67 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Share Capital" && a.Tid == 67).ValueAssigned;
				bbs.TreasuryStock_68 = balanceSheet.Values.Find(a => a.StandardisedName == "Treasury Stock" && a.Tid == 68).ValueAssigned;
				bbs.RetainedEarnings_69 = balanceSheet.Values.Find(a => a.StandardisedName == "Retained Earnings" && a.Tid == 69).ValueAssigned;
				bbs.OtherEquity_70 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Equity" && a.Tid == 70).ValueAssigned;
				bbs.EquityBeforeMinorityInterest_71 = balanceSheet.Values.Find(a => a.StandardisedName == "Equity Before Minority Interest" && a.Tid == 71).ValueAssigned;
				bbs.MinorityInterest_72 = balanceSheet.Values.Find(a => a.StandardisedName == "Minority Interest" && a.Tid == 72).ValueAssigned;
				bbs.TotalEquity_73 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Equity" && a.Tid == 73).ValueAssigned;
				bbs.TotalLiabilitiesEquity_74 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Liabilities & Equity" && a.Tid == 74).ValueAssigned;
				bbsl.Add(bbs);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(bbsl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}
		private async Task PopulateInsuranceBSAsync(List<CompanyFinancials> balanceSheets, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, InsuranceBsWorkSheetName);
			var ibsl = new List<InsuranceBalanceSheet>();
			foreach (var balanceSheet in balanceSheets)
			{
				var ibs = new InsuranceBalanceSheet
				{
					Calculated = balanceSheet.Calculated,
					CompanyId = balanceSheet.CompanyId,
					FYear = balanceSheet.FYear,
					IndustryTemplate = balanceSheet.IndustryTemplate,
					Statement = balanceSheet.Statement
				};
				ibs.TotalInvestments_1 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Investments" && a.Tid == 1).ValueAssigned;
				ibs.FixedIncomeTrading_AFSShortTermInv_2 = balanceSheet.Values.Find(a => a.StandardisedName == "Fixed Income-Trading/AFS & Short Term Inv." && a.Tid == 2).ValueAssigned;
				ibs.LoansMortgages_3 = balanceSheet.Values.Find(a => a.StandardisedName == "Loans & Mortgages" && a.Tid == 3).ValueAssigned;
				ibs.FixedIncomeSecuritiesHTM_4 = balanceSheet.Values.Find(a => a.StandardisedName == "Fixed Income Securities-HTM" && a.Tid == 4).ValueAssigned;
				ibs.EquitySecurities_5 = balanceSheet.Values.Find(a => a.StandardisedName == "Equity Securities" && a.Tid == 5).ValueAssigned;
				ibs.RealEstateInvestments_6 = balanceSheet.Values.Find(a => a.StandardisedName == "Real Estate Investments" && a.Tid == 6).ValueAssigned;
				ibs.OtherInvestments_7 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Investments" && a.Tid == 7).ValueAssigned;
				ibs.CashCashEquivalents_8 = balanceSheet.Values.Find(a => a.StandardisedName == "Cash & Cash Equivalents" && a.Tid == 8).ValueAssigned;
				ibs.AccountsNotesReceivable_9 = balanceSheet.Values.Find(a => a.StandardisedName == "Accounts & Notes Receivable" && a.Tid == 9).ValueAssigned;
				ibs.NetFixedAssets_10 = balanceSheet.Values.Find(a => a.StandardisedName == "Net Fixed Assets" && a.Tid == 10).ValueAssigned;
				ibs.DeferredPolicyAcquisitionCosts_11 = balanceSheet.Values.Find(a => a.StandardisedName == "Deferred Policy Acquisition Costs" && a.Tid == 11).ValueAssigned;
				ibs.OtherAssets_12 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Assets" && a.Tid == 12).ValueAssigned;
				ibs.TotalAssets_13 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Assets" && a.Tid == 13).ValueAssigned;
				ibs.InsuranceReserves_14 = balanceSheet.Values.Find(a => a.StandardisedName == "Insurance Reserves" && a.Tid == 14).ValueAssigned;
				ibs.ReserveforOutstandingClaimsLosses_15 = balanceSheet.Values.Find(a => a.StandardisedName == "Reserve for Outstanding Claims & Losses" && a.Tid == 15).ValueAssigned;
				ibs.PremiumReserve_Unearned__16 = balanceSheet.Values.Find(a => a.StandardisedName == "Premium Reserve (Unearned)" && a.Tid == 16).ValueAssigned;
				ibs.LifePolicyBenefits_17 = balanceSheet.Values.Find(a => a.StandardisedName == "Life Policy Benefits" && a.Tid == 17).ValueAssigned;
				ibs.OtherInsuranceReserves_18 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Insurance Reserves" && a.Tid == 18).ValueAssigned;
				ibs.ShortTermDebt_19 = balanceSheet.Values.Find(a => a.StandardisedName == "Short Term Debt" && a.Tid == 19).ValueAssigned;
				ibs.OtherShortTermLiabilities_20 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Short Term Liabilities" && a.Tid == 20).ValueAssigned;
				ibs.LongTermDebt_21 = balanceSheet.Values.Find(a => a.StandardisedName == "Long Term Debt" && a.Tid == 21).ValueAssigned;
				ibs.PensionLiabilities_55 = balanceSheet.Values.Find(a => a.StandardisedName == "Pension Liabilities" && a.Tid == 55).ValueAssigned;
				ibs.Pensions_56 = balanceSheet.Values.Find(a => a.StandardisedName == "Pensions" && a.Tid == 56).ValueAssigned;
				ibs.OtherPostRetirementBenefits_57 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Post-Retirement Benefits" && a.Tid == 57).ValueAssigned;
				ibs.OtherLongTermLiabilities_22 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Long Term Liabilities" && a.Tid == 22).ValueAssigned;
				ibs.FundsforFutureAppropriations_23 = balanceSheet.Values.Find(a => a.StandardisedName == "Funds for Future Appropriations" && a.Tid == 23).ValueAssigned;
				ibs.TotalLiabilities_24 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Liabilities" && a.Tid == 24).ValueAssigned;
				ibs.PreferredEquity_25 = balanceSheet.Values.Find(a => a.StandardisedName == "Preferred Equity" && a.Tid == 25).ValueAssigned;
				ibs.PolicyholdersEquity_26 = balanceSheet.Values.Find(a => a.StandardisedName == "Policyholders' Equity" && a.Tid == 26).ValueAssigned;
				ibs.ShareCapitalAdditionalPaidInCapital_27 = balanceSheet.Values.Find(a => a.StandardisedName == "Share Capital & Additional Paid-In Capital" && a.Tid == 27).ValueAssigned;
				ibs.CommonStock_28 = balanceSheet.Values.Find(a => a.StandardisedName == "Common Stock" && a.Tid == 28).ValueAssigned;
				ibs.AdditionalPaidinCapital_29 = balanceSheet.Values.Find(a => a.StandardisedName == "Additional Paid in Capital" && a.Tid == 29).ValueAssigned;
				ibs.OtherShareCapital_30 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Share Capital" && a.Tid == 30).ValueAssigned;
				ibs.TreasuryStock_31 = balanceSheet.Values.Find(a => a.StandardisedName == "Treasury Stock" && a.Tid == 31).ValueAssigned;
				ibs.RetainedEarnings_32 = balanceSheet.Values.Find(a => a.StandardisedName == "Retained Earnings" && a.Tid == 32).ValueAssigned;
				ibs.OtherEquity_33 = balanceSheet.Values.Find(a => a.StandardisedName == "Other Equity" && a.Tid == 33).ValueAssigned;
				ibs.EquityBeforeMinorityInterest_34 = balanceSheet.Values.Find(a => a.StandardisedName == "Equity Before Minority Interest" && a.Tid == 34).ValueAssigned;
				ibs.MinorityInterest_35 = balanceSheet.Values.Find(a => a.StandardisedName == "Minority Interest" && a.Tid == 35).ValueAssigned;
				ibs.TotalEquity_36 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Equity" && a.Tid == 36).ValueAssigned;
				ibs.TotalLiabilitiesEquity_37 = balanceSheet.Values.Find(a => a.StandardisedName == "Total Liabilities & Equity" && a.Tid == 37).ValueAssigned;
				ibsl.Add(ibs);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(ibsl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task WriteBalanceSheetDataAsync(List<CompanyFinancials> balanceSheets, string outFile)
		{
			var flattendData = new Dictionary<string, string>();

			using (var package = new ExcelPackage())
			{
				if (balanceSheets.First().IndustryTemplate.Equals("general"))
				{
					_logger.LogInformation("Processing General");
					await PopulateGeneralBSAsync(balanceSheets, outFile, package);
				}
				else if (balanceSheets.First().IndustryTemplate.Equals("insurances"))
				{
					_logger.LogInformation("Processing Insurance");
					await PopulateInsuranceBSAsync(balanceSheets, outFile, package);
				}
				else if (balanceSheets.First().IndustryTemplate.Equals("banks"))
				{
					_logger.LogInformation("Processing Bank");
					await PopulateBankBSAsync(balanceSheets, outFile, package);
				}
				else
				{
					_logger.LogError($"Unknown Industry template {balanceSheets.First().IndustryTemplate}");
				}
			}
			return;
		}

		

		#endregion Private Methods
	}
}