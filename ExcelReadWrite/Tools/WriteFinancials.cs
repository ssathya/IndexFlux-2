using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelReadWrite.Tools
{
	public class WriteFinancials
	{


		#region Private Fields

		private const string BankBsWorkSheetName = "Bank-Balance-Sheets";
		private const string BankCfWorkSheetName = "Bank-Cash-Flow";
		private const string BankPlWorkSheetName = "Bank-Profit-Loss";
		private const string GeneralBsWorkSheetName = "General-Balance-Sheets";
		private const string GeneralCfWorkSheetName = "General-Cash-Flow";
		private const string GeneralPlWorkSheetName = "General-Profit-Loss";
		private const string InsuranceBsWorkSheetName = "Insurance-Balance-Sheets";
		private const string InsuranceCfWorkSheetName = "Insurance-Cash-Flow";
		private const string InsurancePlWorkSheetName = "Insurance-Profit-Loss";
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
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			var companyFinancials = await ObtainCompanyFinancilasAsync(SimId);
			stopWatch.Stop();
			var msg = "Time to fetch data";
			DisplayTimeTaken(stopWatch, msg);
			if (companyFinancials == null || companyFinancials.Count == 0)
			{
				return false;
			}
			var balanceSheets = companyFinancials.Where(cf => cf.Statement == StatementType.BalanceSheet).ToList();
			var cashFlows = companyFinancials.Where(cf => cf.Statement == StatementType.CashFlow).ToList();
			var profitAndLosss = companyFinancials.Where(cf => cf.Statement == StatementType.ProfitLoss).ToList();

			msg = "Time to update excel";
			stopWatch.Reset();
			stopWatch.Start();
			await UpdateIndustryTemplate(balanceSheets.First().IndustryTemplate, outFile, SimId);

			await WriteBalanceSheetDataAsync(balanceSheets, outFile);
			await WriteCashFlowDataAsync(cashFlows, outFile);
			await WriteProfitAndLossAsync(profitAndLosss, outFile);
			Task.WaitAll();
			stopWatch.Stop();
			DisplayTimeTaken(stopWatch, msg);
			return true;
		}

		#endregion Public Methods


		#region Private Methods

		private static BankBalanceSheet BuildBankBalanceSheet(CompanyFinancials balanceSheet)
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
			return bbs;
		}

		private static GeneralBalanceSheet BuildGeneralBalanceSheet(CompanyFinancials balanceSheet)
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
			return fbc;
		}

		private static InsuranceBalanceSheet BuildInsuranceBalanceSheet(CompanyFinancials balanceSheet)
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
			return ibs;
		}

		private static InsuranceCashFlow BuildInsurnaceCashFlow(CompanyFinancials cashFlow)
		{
			var icf = new InsuranceCashFlow
			{
				Calculated = cashFlow.Calculated,
				CompanyId = cashFlow.CompanyId,
				FYear = cashFlow.FYear,
				IndustryTemplate = cashFlow.IndustryTemplate,
				Statement = cashFlow.Statement
			};
			icf.NetIncome_StartingLine_1 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income/Starting Line" && a.Tid == 1).ValueAssigned;
			icf.NetIncome_2 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income" && a.Tid == 2).ValueAssigned;
			icf.NetIncomeFromDiscontinuedOperations_3 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income From Discontinued Operations" && a.Tid == 3).ValueAssigned;
			icf.OtherAdjustments_4 = cashFlow.Values.Find(a => a.StandardisedName == "Other Adjustments" && a.Tid == 4).ValueAssigned;
			icf.DepreciationAmortization_5 = cashFlow.Values.Find(a => a.StandardisedName == "Depreciation & Amortization" && a.Tid == 5).ValueAssigned;
			icf.NonCashItems_6 = cashFlow.Values.Find(a => a.StandardisedName == "Non-Cash Items" && a.Tid == 6).ValueAssigned;
			icf.StockBasedCompensation_7 = cashFlow.Values.Find(a => a.StandardisedName == "Stock-Based Compensation" && a.Tid == 7).ValueAssigned;
			icf.DeferredIncomeTaxes_8 = cashFlow.Values.Find(a => a.StandardisedName == "Deferred Income Taxes" && a.Tid == 8).ValueAssigned;
			icf.OtherNonCashAdjustments_9 = cashFlow.Values.Find(a => a.StandardisedName == "Other Non-Cash Adjustments" && a.Tid == 9).ValueAssigned;
			icf.NetChangeinOperatingCapital_10 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Operating Capital" && a.Tid == 10).ValueAssigned;
			icf.NetCashFromDiscontinuedOperations_operating__11 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (operating)" && a.Tid == 11).ValueAssigned;
			icf.CashfromOperatingActivities_12 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Operating Activities" && a.Tid == 12).ValueAssigned;
			icf.ChangeinFixedAssetsandIntangibles_37 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Fixed Assets and Intangibles" && a.Tid == 37).ValueAssigned;
			icf.DispositionofFixedAssetsIntangibles_13 = cashFlow.Values.Find(a => a.StandardisedName == "Disposition of Fixed Assets & Intangibles" && a.Tid == 13).ValueAssigned;
			icf.AcquisitionofFixedAssetsIntangibles_14 = cashFlow.Values.Find(a => a.StandardisedName == "Acquisition of Fixed Assets & Intangibles" && a.Tid == 14).ValueAssigned;
			icf.NetChangeinInvestments_36 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Investments" && a.Tid == 36).ValueAssigned;
			icf.IncreaseinInvestments_15 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in Investments" && a.Tid == 15).ValueAssigned;
			icf.DecreaseinInvestments_16 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in Investments" && a.Tid == 16).ValueAssigned;
			icf.OtherInvestingActivities_17 = cashFlow.Values.Find(a => a.StandardisedName == "Other Investing Activities" && a.Tid == 17).ValueAssigned;
			icf.NetCashFromDiscontinuedOperations_investing__18 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (investing)" && a.Tid == 18).ValueAssigned;
			icf.CashfromInvestingActivities_19 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Investing Activities" && a.Tid == 19).ValueAssigned;
			icf.DividendsPaid_20 = cashFlow.Values.Find(a => a.StandardisedName == "Dividends Paid" && a.Tid == 20).ValueAssigned;
			icf.CashFrom_Repaymentof_Debt_21 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Debt" && a.Tid == 21).ValueAssigned;
			icf.CashFrom_Repaymentof_ShortTermDebtnet_22 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Short Term Debt, net" && a.Tid == 22).ValueAssigned;
			icf.CashFrom_Repaymentof_LongTermDebtnet_23 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Long Term Debt, net" && a.Tid == 23).ValueAssigned;
			icf.RepaymentsofLongTermDebt_24 = cashFlow.Values.Find(a => a.StandardisedName == "Repayments of Long Term Debt" && a.Tid == 24).ValueAssigned;
			icf.CashFromLongTermDebt_25 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From Long Term Debt" && a.Tid == 25).ValueAssigned;
			icf.Cash_Repurchase_ofEquity_26 = cashFlow.Values.Find(a => a.StandardisedName == "Cash (Repurchase) of Equity" && a.Tid == 26).ValueAssigned;
			icf.IncreaseinCapitalStock_27 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in Capital Stock" && a.Tid == 27).ValueAssigned;
			icf.DecreaseinCapitalStock_28 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in Capital Stock" && a.Tid == 28).ValueAssigned;
			icf.ChangeinInsuranceReserves_29 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Insurance Reserves" && a.Tid == 29).ValueAssigned;
			icf.OtherFinancingActivities_30 = cashFlow.Values.Find(a => a.StandardisedName == "Other Financing Activities" && a.Tid == 30).ValueAssigned;
			icf.NetCashFromDiscontinuedOperations_financing__31 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (financing)" && a.Tid == 31).ValueAssigned;
			icf.CashfromFinancingActivities_32 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Financing Activities" && a.Tid == 32).ValueAssigned;
			icf.NetCashBeforeDiscOperationsandFX_45 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash Before Disc. Operations and FX" && a.Tid == 45).ValueAssigned;
			icf.ChangeinCashfromDiscOperationsandOther_34 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Cash from Disc. Operations and Other" && a.Tid == 34).ValueAssigned;
			icf.NetCashBeforeExchangeRates_44 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash Before Exchange Rates" && a.Tid == 44).ValueAssigned;
			icf.EffectofForeignExchangeRates_33 = cashFlow.Values.Find(a => a.StandardisedName == "Effect of Foreign Exchange Rates" && a.Tid == 33).ValueAssigned;
			icf.NetChangesinCash_35 = cashFlow.Values.Find(a => a.StandardisedName == "Net Changes in Cash" && a.Tid == 35).ValueAssigned;
			return icf;
		}

		private static void DisplayTimeTaken(Stopwatch stopWatch, string msg)
		{
			var ts = stopWatch.Elapsed;
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
				ts.Hours, ts.Minutes, ts.Seconds,
				ts.Milliseconds / 10);
			Console.WriteLine($"\n{msg} {elapsedTime}");
		}

		private BankCashFlow BuildBankCashFlow(CompanyFinancials cashFlow)
		{
			var bcf = new BankCashFlow
			{
				Calculated = cashFlow.Calculated,
				CompanyId = cashFlow.CompanyId,
				FYear = cashFlow.FYear,
				IndustryTemplate = cashFlow.IndustryTemplate,
				Statement = cashFlow.Statement
			};
			bcf.NetIncome_StartingLine_1 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income/Starting Line" && a.Tid == 1).ValueAssigned;
			bcf.NetIncome_2 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income" && a.Tid == 2).ValueAssigned;
			bcf.NetIncomeFromDiscontinuedOperations_3 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income From Discontinued Operations" && a.Tid == 3).ValueAssigned;
			bcf.OtherAdjustments_4 = cashFlow.Values.Find(a => a.StandardisedName == "Other Adjustments" && a.Tid == 4).ValueAssigned;
			bcf.DepreciationAmortization_5 = cashFlow.Values.Find(a => a.StandardisedName == "Depreciation & Amortization" && a.Tid == 5).ValueAssigned;
			bcf.ProvisionforLoanLosses_6 = cashFlow.Values.Find(a => a.StandardisedName == "Provision for Loan Losses" && a.Tid == 6).ValueAssigned;
			bcf.NonCashItems_7 = cashFlow.Values.Find(a => a.StandardisedName == "Non-Cash Items" && a.Tid == 7).ValueAssigned;
			bcf.GainonSaleofSecuritiesLoans_8 = cashFlow.Values.Find(a => a.StandardisedName == "Gain on Sale of Securities & Loans" && a.Tid == 8).ValueAssigned;
			bcf.DeferredIncomeTaxes_9 = cashFlow.Values.Find(a => a.StandardisedName == "Deferred Income Taxes" && a.Tid == 9).ValueAssigned;
			bcf.StockBasedCompensation_10 = cashFlow.Values.Find(a => a.StandardisedName == "Stock-Based Compensation" && a.Tid == 10).ValueAssigned;
			bcf.OtherNonCashAdjustments_11 = cashFlow.Values.Find(a => a.StandardisedName == "Other Non-Cash Adjustments" && a.Tid == 11).ValueAssigned;
			bcf.NetChangeinOperatingCapital_12 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Operating Capital" && a.Tid == 12).ValueAssigned;
			bcf.TradingAssetsLiabilities_13 = cashFlow.Values.Find(a => a.StandardisedName == "Trading Assets & Liabilities" && a.Tid == 13).ValueAssigned;
			bcf.NetChangeofInvestments_14 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change of Investments" && a.Tid == 14).ValueAssigned;
			bcf.NetChangeofInterbankAssets_15 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change of Interbank Assets" && a.Tid == 15).ValueAssigned;
			bcf.NetChangeofInterbankLiabilities_16 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change of Interbank Liabilities" && a.Tid == 16).ValueAssigned;
			bcf.NetChangeinOperatingLoans_17 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Operating Loans" && a.Tid == 17).ValueAssigned;
			bcf.AccruedInterestReceivable_18 = cashFlow.Values.Find(a => a.StandardisedName == "Accrued Interest Receivable" && a.Tid == 18).ValueAssigned;
			bcf.AccruedInterestPayable_19 = cashFlow.Values.Find(a => a.StandardisedName == "Accrued Interest Payable" && a.Tid == 19).ValueAssigned;
			bcf.OtherOperatingAssets_Liabilities_20 = cashFlow.Values.Find(a => a.StandardisedName == "Other Operating Assets/Liabilities" && a.Tid == 20).ValueAssigned;
			bcf.NetCashFromDiscontinuedOperations_operating__21 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (operating)" && a.Tid == 21).ValueAssigned;
			bcf.CashfromOperatingActivities_22 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Operating Activities" && a.Tid == 22).ValueAssigned;
			bcf.ChangeinFixedAssetsIntangibles_23 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Fixed Assets & Intangibles" && a.Tid == 23).ValueAssigned;
			bcf.DisposalofFixedAssetsIntangibles_24 = cashFlow.Values.Find(a => a.StandardisedName == "Disposal of Fixed Assets & Intangibles" && a.Tid == 24).ValueAssigned;
			bcf.CapitalExpenditures_25 = cashFlow.Values.Find(a => a.StandardisedName == "Capital Expenditures" && a.Tid == 25).ValueAssigned;
			bcf.NetChangeinInvestments_26 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Investments" && a.Tid == 26).ValueAssigned;
			bcf.DecreaseinInvestments_27 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in Investments" && a.Tid == 27).ValueAssigned;
			bcf.DecreaseinHTMInvestments_28 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in HTM Investments" && a.Tid == 28).ValueAssigned;
			bcf.DecreaseinAFSInvestments_29 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in AFS Investments" && a.Tid == 29).ValueAssigned;
			bcf.IncreaseinInvestments_30 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in Investments" && a.Tid == 30).ValueAssigned;
			bcf.IncreaseinHTMInvestments_31 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in HTM Investments" && a.Tid == 31).ValueAssigned;
			bcf.IncreaseinAFSInvestments_32 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in AFS Investments" && a.Tid == 32).ValueAssigned;
			bcf.NetChangeinOtherInvestments_33 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Other Investments" && a.Tid == 33).ValueAssigned;
			bcf.NetChangeinLoansInterbank_34 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Loans & Interbank" && a.Tid == 34).ValueAssigned;
			bcf.NetChangeinCustomerLoans_35 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Customer Loans" && a.Tid == 35).ValueAssigned;
			bcf.NetChangeinInterbankAssets_36 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Interbank Assets" && a.Tid == 36).ValueAssigned;
			bcf.NetChangeinOtherLoans_37 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Other Loans" && a.Tid == 37).ValueAssigned;
			bcf.NetCashFromAcquisitionsDivestitures_38 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Acquisitions & Divestitures" && a.Tid == 38).ValueAssigned;
			bcf.NetCashfromDivestitures_39 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash from Divestitures" && a.Tid == 39).ValueAssigned;
			bcf.CashforAcqusitionofSubsidiaries_40 = cashFlow.Values.Find(a => a.StandardisedName == "Cash for Acqusition of Subsidiaries" && a.Tid == 40).ValueAssigned;
			bcf.CashforJointVentures_41 = cashFlow.Values.Find(a => a.StandardisedName == "Cash for Joint Ventures" && a.Tid == 41).ValueAssigned;
			bcf.NetCashfromOtherAcquisitions_42 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash from Other Acquisitions" && a.Tid == 42).ValueAssigned;
			bcf.OtherInvestingActivities_43 = cashFlow.Values.Find(a => a.StandardisedName == "Other Investing Activities" && a.Tid == 43).ValueAssigned;
			bcf.NetCashFromDiscontinuedOperations_investing__44 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (investing)" && a.Tid == 44).ValueAssigned;
			bcf.CashfromInvestingActivities_45 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Investing Activities" && a.Tid == 45).ValueAssigned;
			bcf.DividendsPaid_46 = cashFlow.Values.Find(a => a.StandardisedName == "Dividends Paid" && a.Tid == 46).ValueAssigned;
			bcf.CashFrom_Repaymentof_Debt_47 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Debt" && a.Tid == 47).ValueAssigned;
			bcf.CashFrom_Repaymentof_ShortTermDebtnet_48 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Short Term Debt, net" && a.Tid == 48).ValueAssigned;
			bcf.NetChangeinInterbankTransfers_49 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Interbank Transfers" && a.Tid == 49).ValueAssigned;
			bcf.CashFrom_Repaymentof_LongTermDebtnet_50 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Long Term Debt, net" && a.Tid == 50).ValueAssigned;
			bcf.RepaymentsofLongTermDebt_51 = cashFlow.Values.Find(a => a.StandardisedName == "Repayments of Long Term Debt" && a.Tid == 51).ValueAssigned;
			bcf.CashFromLongTermDebt_52 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From Long Term Debt" && a.Tid == 52).ValueAssigned;
			bcf.CashFrom_Repurchaseof_Equity_53 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repurchase of) Equity" && a.Tid == 53).ValueAssigned;
			bcf.IncreaseinCapitalStock_54 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in Capital Stock" && a.Tid == 54).ValueAssigned;
			bcf.DecreaseinCapitalStock_55 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in Capital Stock" && a.Tid == 55).ValueAssigned;
			bcf.NetChangeInDeposits_56 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change In Deposits" && a.Tid == 56).ValueAssigned;
			bcf.OtherFinancingActivities_57 = cashFlow.Values.Find(a => a.StandardisedName == "Other Financing Activities" && a.Tid == 57).ValueAssigned;
			bcf.NetCashFromDiscontinuedOperations_financing__58 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (financing)" && a.Tid == 58).ValueAssigned;
			bcf.CashfromFinancingActivities_59 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Financing Activities" && a.Tid == 59).ValueAssigned;
			bcf.NetCashBeforeDiscOperationsandFX_71 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash Before Disc. Operations and FX" && a.Tid == 71).ValueAssigned;
			bcf.ChangeinCashfromDiscOperationsandOther_61 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Cash from Disc. Operations and Other" && a.Tid == 61).ValueAssigned;
			bcf.NetCashBeforeFX_70 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash Before FX" && a.Tid == 70).ValueAssigned;
			bcf.EffectofForeignExchangeRates_60 = cashFlow.Values.Find(a => a.StandardisedName == "Effect of Foreign Exchange Rates" && a.Tid == 60).ValueAssigned;
			bcf.NetChangesinCash_62 = cashFlow.Values.Find(a => a.StandardisedName == "Net Changes in Cash" && a.Tid == 62).ValueAssigned;
			return bcf;
		}

		private BankProfitAndLoss BuildBankProfitLoss(CompanyFinancials profitAndLoss)
		{
			var bpl = new BankProfitAndLoss
			{
				Calculated = profitAndLoss.Calculated,
				CompanyId = profitAndLoss.CompanyId,
				FYear = profitAndLoss.FYear,
				IndustryTemplate = profitAndLoss.IndustryTemplate,
				Statement = profitAndLoss.Statement
			};
			bpl.NetRevenue_1 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Revenue" && a.Tid == 1).ValueAssigned;
			bpl.Netinterestincome_2 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net interest income" && a.Tid == 2).ValueAssigned;
			bpl.TotalInterestIncome_3 = profitAndLoss.Values.Find(a => a.StandardisedName == "Total Interest Income" && a.Tid == 3).ValueAssigned;
			bpl.TotalInterestExpense_6 = profitAndLoss.Values.Find(a => a.StandardisedName == "Total Interest Expense" && a.Tid == 6).ValueAssigned;
			bpl.TotalNonInterestIncome_7 = profitAndLoss.Values.Find(a => a.StandardisedName == "Total Non-Interest Income" && a.Tid == 7).ValueAssigned;
			bpl.TradingAccountProfits_Losses_8 = profitAndLoss.Values.Find(a => a.StandardisedName == "Trading Account Profits/Losses" && a.Tid == 8).ValueAssigned;
			bpl.InvestmentIncome_Loss__10 = profitAndLoss.Values.Find(a => a.StandardisedName == "Investment Income (Loss)" && a.Tid == 10).ValueAssigned;
			bpl.SaleofLoanIncome_Loss__11 = profitAndLoss.Values.Find(a => a.StandardisedName == "Sale of Loan Income (Loss)" && a.Tid == 11).ValueAssigned;
			bpl.CommissionsFeesEarned_12 = profitAndLoss.Values.Find(a => a.StandardisedName == "Commissions & Fees Earned" && a.Tid == 12).ValueAssigned;
			bpl.NetOTTIlossesrecognisedinearnings_52 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net OTTI losses recognised in earnings" && a.Tid == 52).ValueAssigned;
			bpl.OtherNonInterestIncome_13 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Non-Interest Income" && a.Tid == 13).ValueAssigned;
			bpl.ProvisionforLoanLosses_14 = profitAndLoss.Values.Find(a => a.StandardisedName == "Provision for Loan Losses" && a.Tid == 14).ValueAssigned;
			bpl.NetRevenueafterProvisions_15 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Revenue after Provisions" && a.Tid == 15).ValueAssigned;
			bpl.TotalNonInterestExpense_16 = profitAndLoss.Values.Find(a => a.StandardisedName == "Total Non-Interest Expense" && a.Tid == 16).ValueAssigned;
			bpl.CommissionsFeesPaid_17 = profitAndLoss.Values.Find(a => a.StandardisedName == "Commissions & Fees Paid" && a.Tid == 17).ValueAssigned;
			bpl.OtherOperatingExpense_18 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Operating Expense" && a.Tid == 18).ValueAssigned;
			bpl.OperatingIncome_Loss__19 = profitAndLoss.Values.Find(a => a.StandardisedName == "Operating Income (Loss)" && a.Tid == 19).ValueAssigned;
			bpl.NonOperatingIncome_Loss__20 = profitAndLoss.Values.Find(a => a.StandardisedName == "Non-Operating Income (Loss)" && a.Tid == 20).ValueAssigned;
			bpl.Income_Loss_fromAffiliates_21 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Affiliates" && a.Tid == 21).ValueAssigned;
			bpl.OtherNonOperatingIncome_Loss__22 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Non-Operating Income (Loss)" && a.Tid == 22).ValueAssigned;
			bpl.PretaxIncome_Loss_Adjusted_23 = profitAndLoss.Values.Find(a => a.StandardisedName == "Pretax Income (Loss), Adjusted" && a.Tid == 23).ValueAssigned;
			bpl.AbnormalGains_Losses__24 = profitAndLoss.Values.Find(a => a.StandardisedName == "Abnormal Gains (Losses)" && a.Tid == 24).ValueAssigned;
			bpl.DebtValuationAdjustment_25 = profitAndLoss.Values.Find(a => a.StandardisedName == "Debt Valuation Adjustment" && a.Tid == 25).ValueAssigned;
			bpl.CreditValuationAdjustment_26 = profitAndLoss.Values.Find(a => a.StandardisedName == "Credit Valuation Adjustment" && a.Tid == 26).ValueAssigned;
			bpl.Merger_AcquisitionExpense_27 = profitAndLoss.Values.Find(a => a.StandardisedName == "Merger / Acquisition Expense" && a.Tid == 27).ValueAssigned;
			bpl.DisposalofAssets_28 = profitAndLoss.Values.Find(a => a.StandardisedName == "Disposal of Assets" && a.Tid == 28).ValueAssigned;
			bpl.EarlyextinguishmentofDebt_29 = profitAndLoss.Values.Find(a => a.StandardisedName == "Early extinguishment of Debt" && a.Tid == 29).ValueAssigned;
			bpl.AssetWriteDown_30 = profitAndLoss.Values.Find(a => a.StandardisedName == "Asset Write-Down" && a.Tid == 30).ValueAssigned;
			bpl.ImpairmentofGoodwillIntangibles_31 = profitAndLoss.Values.Find(a => a.StandardisedName == "Impairment of Goodwill & Intangibles" && a.Tid == 31).ValueAssigned;
			bpl.SaleofBusiness_32 = profitAndLoss.Values.Find(a => a.StandardisedName == "Sale of Business" && a.Tid == 32).ValueAssigned;
			bpl.LegalSettlement_33 = profitAndLoss.Values.Find(a => a.StandardisedName == "Legal Settlement" && a.Tid == 33).ValueAssigned;
			bpl.RestructuringCharges_34 = profitAndLoss.Values.Find(a => a.StandardisedName == "Restructuring Charges" && a.Tid == 34).ValueAssigned;
			bpl.OtherAbnormalItems_35 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Abnormal Items" && a.Tid == 35).ValueAssigned;
			bpl.PretaxIncome_Loss__36 = profitAndLoss.Values.Find(a => a.StandardisedName == "Pretax Income (Loss)" && a.Tid == 36).ValueAssigned;
			bpl.IncomeTax_Expense_Benefitnet_37 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income Tax (Expense) Benefit, net" && a.Tid == 37).ValueAssigned;
			bpl.CurrentIncomeTax_38 = profitAndLoss.Values.Find(a => a.StandardisedName == "Current Income Tax" && a.Tid == 38).ValueAssigned;
			bpl.DeferredIncomeTax_39 = profitAndLoss.Values.Find(a => a.StandardisedName == "Deferred Income Tax" && a.Tid == 39).ValueAssigned;
			bpl.TaxAllowance_Credit_40 = profitAndLoss.Values.Find(a => a.StandardisedName == "Tax Allowance/Credit" && a.Tid == 40).ValueAssigned;
			bpl.Income_Loss_fromAffiliatesnetoftaxes_41 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Affiliates, net of taxes" && a.Tid == 41).ValueAssigned;
			bpl.Income_Loss_fromContinuingOperations_42 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Continuing Operations" && a.Tid == 42).ValueAssigned;
			bpl.NetExtraordinaryGains_Losses__43 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Extraordinary Gains (Losses)" && a.Tid == 43).ValueAssigned;
			bpl.DiscontinuedOperations_44 = profitAndLoss.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 44).ValueAssigned;
			bpl.XOAccountingChargesOther_45 = profitAndLoss.Values.Find(a => a.StandardisedName == "XO & Accounting Charges & Other" && a.Tid == 45).ValueAssigned;
			bpl.Income_Loss_IncludingMinorityInterest_46 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) Including Minority Interest" && a.Tid == 46).ValueAssigned;
			bpl.MinorityInterest_47 = profitAndLoss.Values.Find(a => a.StandardisedName == "Minority Interest" && a.Tid == 47).ValueAssigned;
			bpl.NetIncome_48 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Income" && a.Tid == 48).ValueAssigned;
			bpl.PreferredDividends_49 = profitAndLoss.Values.Find(a => a.StandardisedName == "Preferred Dividends" && a.Tid == 49).ValueAssigned;
			bpl.OtherAdjustments_50 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Adjustments" && a.Tid == 50).ValueAssigned;
			bpl.NetIncomeAvailabletoCommonShareholders_51 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Income Available to Common Shareholders" && a.Tid == 51).ValueAssigned;

			return bpl;
		}

		private GeneralCashFlow BuildGeneralCashFlow(CompanyFinancials cashFlow)
		{
			var gcf = new GeneralCashFlow
			{
				Calculated = cashFlow.Calculated,
				CompanyId = cashFlow.CompanyId,
				FYear = cashFlow.FYear,
				IndustryTemplate = cashFlow.IndustryTemplate,
				Statement = cashFlow.Statement
			};
			gcf.NetIncome_StartingLine_1 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income/Starting Line" && a.Tid == 1).ValueAssigned;
			gcf.NetIncome_47 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income" && a.Tid == 47).ValueAssigned;
			gcf.NetIncomeFromDiscontinuedOperations_48 = cashFlow.Values.Find(a => a.StandardisedName == "Net Income From Discontinued Operations" && a.Tid == 48).ValueAssigned;
			gcf.OtherAdjustments_49 = cashFlow.Values.Find(a => a.StandardisedName == "Other Adjustments" && a.Tid == 49).ValueAssigned;
			gcf.DepreciationAmortization_2 = cashFlow.Values.Find(a => a.StandardisedName == "Depreciation & Amortization" && a.Tid == 2).ValueAssigned;
			gcf.NonCashItems_3 = cashFlow.Values.Find(a => a.StandardisedName == "Non-Cash Items" && a.Tid == 3).ValueAssigned;
			gcf.StockBasedCompensation_4 = cashFlow.Values.Find(a => a.StandardisedName == "Stock-Based Compensation" && a.Tid == 4).ValueAssigned;
			gcf.DeferredIncomeTaxes_5 = cashFlow.Values.Find(a => a.StandardisedName == "Deferred Income Taxes" && a.Tid == 5).ValueAssigned;
			gcf.OtherNonCashAdjustments_6 = cashFlow.Values.Find(a => a.StandardisedName == "Other Non-Cash Adjustments" && a.Tid == 6).ValueAssigned;
			gcf.ChangeinWorkingCapital_7 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Working Capital" && a.Tid == 7).ValueAssigned;
			gcf._Increase_DecreaseinAccountsReceivable_8 = cashFlow.Values.Find(a => a.StandardisedName == "(Increase) Decrease in Accounts Receivable" && a.Tid == 8).ValueAssigned;
			gcf._Increase_DecreaseinInventories_9 = cashFlow.Values.Find(a => a.StandardisedName == "(Increase) Decrease in Inventories" && a.Tid == 9).ValueAssigned;
			gcf.Increase_Decrease_inAccountsPayable_10 = cashFlow.Values.Find(a => a.StandardisedName == "Increase (Decrease) in Accounts Payable" && a.Tid == 10).ValueAssigned;
			gcf.Increase_Decrease_inOther_11 = cashFlow.Values.Find(a => a.StandardisedName == "Increase (Decrease) in Other" && a.Tid == 11).ValueAssigned;
			gcf.NetCashFromDiscontinuedOperations_operating__12 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (operating)" && a.Tid == 12).ValueAssigned;
			gcf.CashfromOperatingActivities_13 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Operating Activities" && a.Tid == 13).ValueAssigned;
			gcf.ChangeinFixedAssetsIntangibles_14 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Fixed Assets & Intangibles" && a.Tid == 14).ValueAssigned;
			gcf.DispositionofFixedAssetsIntangibles_15 = cashFlow.Values.Find(a => a.StandardisedName == "Disposition of Fixed Assets & Intangibles" && a.Tid == 15).ValueAssigned;
			gcf.DispositionofFixedAssets_16 = cashFlow.Values.Find(a => a.StandardisedName == "Disposition of Fixed Assets" && a.Tid == 16).ValueAssigned;
			gcf.DispositionofIntangibleAssets_17 = cashFlow.Values.Find(a => a.StandardisedName == "Disposition of Intangible Assets" && a.Tid == 17).ValueAssigned;
			gcf.AcquisitionofFixedAssetsIntangibles_18 = cashFlow.Values.Find(a => a.StandardisedName == "Acquisition of Fixed Assets & Intangibles" && a.Tid == 18).ValueAssigned;
			gcf.PurchaseofFixedAssets_19 = cashFlow.Values.Find(a => a.StandardisedName == "Purchase of Fixed Assets" && a.Tid == 19).ValueAssigned;
			gcf.AcquisitionofIntangibleAssets_20 = cashFlow.Values.Find(a => a.StandardisedName == "Acquisition of Intangible Assets" && a.Tid == 20).ValueAssigned;
			gcf.OtherChangeinFixedAssetsIntangibles_21 = cashFlow.Values.Find(a => a.StandardisedName == "Other Change in Fixed Assets & Intangibles" && a.Tid == 21).ValueAssigned;
			gcf.NetChangeinLongTermInvestment_22 = cashFlow.Values.Find(a => a.StandardisedName == "Net Change in Long Term Investment" && a.Tid == 22).ValueAssigned;
			gcf.DecreaseinLongTermInvestment_23 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in Long Term Investment" && a.Tid == 23).ValueAssigned;
			gcf.IncreaseinLongTermInvestment_24 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in Long Term Investment" && a.Tid == 24).ValueAssigned;
			gcf.NetCashFromAcquisitionsDivestitures_25 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Acquisitions & Divestitures" && a.Tid == 25).ValueAssigned;
			gcf.NetCashfromDivestitures_26 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash from Divestitures" && a.Tid == 26).ValueAssigned;
			gcf.CashforAcqusitionofSubsidiaries_27 = cashFlow.Values.Find(a => a.StandardisedName == "Cash for Acqusition of Subsidiaries" && a.Tid == 27).ValueAssigned;
			gcf.CashforJointVentures_28 = cashFlow.Values.Find(a => a.StandardisedName == "Cash for Joint Ventures" && a.Tid == 28).ValueAssigned;
			gcf.NetCashfromOtherAcquisitions_50 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash from Other Acquisitions" && a.Tid == 50).ValueAssigned;
			gcf.OtherInvestingActivities_29 = cashFlow.Values.Find(a => a.StandardisedName == "Other Investing Activities" && a.Tid == 29).ValueAssigned;
			gcf.NetCashFromDiscontinuedOperations_investing__30 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (investing)" && a.Tid == 30).ValueAssigned;
			gcf.CashfromInvestingActivities_31 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Investing Activities" && a.Tid == 31).ValueAssigned;
			gcf.DividendsPaid_32 = cashFlow.Values.Find(a => a.StandardisedName == "Dividends Paid" && a.Tid == 32).ValueAssigned;
			gcf.CashFrom_Repaymentof_Debt_33 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Debt" && a.Tid == 33).ValueAssigned;
			gcf.CashFrom_Repaymentof_ShortTermDebtnet_34 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Short Term Debt, net" && a.Tid == 34).ValueAssigned;
			gcf.CashFrom_Repaymentof_LongTermDebtnet_35 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repayment of) Long Term Debt, net" && a.Tid == 35).ValueAssigned;
			gcf.RepaymentsofLongTermDebt_36 = cashFlow.Values.Find(a => a.StandardisedName == "Repayments of Long Term Debt" && a.Tid == 36).ValueAssigned;
			gcf.CashFromLongTermDebt_37 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From Long Term Debt" && a.Tid == 37).ValueAssigned;
			gcf.CashFrom_Repurchaseof_Equity_38 = cashFlow.Values.Find(a => a.StandardisedName == "Cash From (Repurchase of) Equity" && a.Tid == 38).ValueAssigned;
			gcf.IncreaseinCapitalStock_39 = cashFlow.Values.Find(a => a.StandardisedName == "Increase in Capital Stock" && a.Tid == 39).ValueAssigned;
			gcf.DecreaseinCapitalStock_40 = cashFlow.Values.Find(a => a.StandardisedName == "Decrease in Capital Stock" && a.Tid == 40).ValueAssigned;
			gcf.OtherFinancingActivities_41 = cashFlow.Values.Find(a => a.StandardisedName == "Other Financing Activities" && a.Tid == 41).ValueAssigned;
			gcf.NetCashFromDiscontinuedOperations_financing__42 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash From Discontinued Operations (financing)" && a.Tid == 42).ValueAssigned;
			gcf.CashfromFinancingActivities_43 = cashFlow.Values.Find(a => a.StandardisedName == "Cash from Financing Activities" && a.Tid == 43).ValueAssigned;
			gcf.NetCashBeforeDiscOperationsandFX_56 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash Before Disc. Operations and FX" && a.Tid == 56).ValueAssigned;
			gcf.ChangeinCashfromDiscOperationsandOther_45 = cashFlow.Values.Find(a => a.StandardisedName == "Change in Cash from Disc. Operations and Other" && a.Tid == 45).ValueAssigned;
			gcf.NetCashBeforeFX_55 = cashFlow.Values.Find(a => a.StandardisedName == "Net Cash Before FX" && a.Tid == 55).ValueAssigned;
			gcf.EffectofForeignExchangeRates_44 = cashFlow.Values.Find(a => a.StandardisedName == "Effect of Foreign Exchange Rates" && a.Tid == 44).ValueAssigned;
			gcf.NetChangesinCash_46 = cashFlow.Values.Find(a => a.StandardisedName == "Net Changes in Cash" && a.Tid == 46).ValueAssigned;

			return gcf;
		}

		private InsuranceProfitAndLoss BuildInsurancePrfitAndLoss(CompanyFinancials profitAndLoss)
		{
			var ipl = new InsuranceProfitAndLoss
			{
				Calculated = profitAndLoss.Calculated,
				CompanyId = profitAndLoss.CompanyId,
				FYear = profitAndLoss.FYear,
				IndustryTemplate = profitAndLoss.IndustryTemplate,
				Statement = profitAndLoss.Statement
			};
			ipl.NetRevenue_1 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Revenue" && a.Tid == 1).ValueAssigned;
			ipl.NetPremiumsEarned_2 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Premiums Earned" && a.Tid == 2).ValueAssigned;
			ipl.InvestmentIncome_Loss__6 = profitAndLoss.Values.Find(a => a.StandardisedName == "Investment Income (Loss)" && a.Tid == 6).ValueAssigned;
			ipl.IncomefromRealEstate_7 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income from Real Estate" && a.Tid == 7).ValueAssigned;
			ipl.OtherOperatingIncome_8 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Operating Income" && a.Tid == 8).ValueAssigned;
			ipl.PolicyChargesFees_9 = profitAndLoss.Values.Find(a => a.StandardisedName == "Policy Charges & Fees" && a.Tid == 9).ValueAssigned;
			ipl.TotalRealizedInvestmentGains_62 = profitAndLoss.Values.Find(a => a.StandardisedName == "Total Realized Investment Gains" && a.Tid == 62).ValueAssigned;
			ipl.TotalOTTIRealized_63 = profitAndLoss.Values.Find(a => a.StandardisedName == "Total OTTI Realized" && a.Tid == 63).ValueAssigned;
			ipl.OtherRealizedInvestmentGains_64 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Realized Investment Gains" && a.Tid == 64).ValueAssigned;
			ipl.OtherIncome_10 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Income" && a.Tid == 10).ValueAssigned;
			ipl.ClaimsLosses_11 = profitAndLoss.Values.Find(a => a.StandardisedName == "Claims & Losses" && a.Tid == 11).ValueAssigned;
			ipl.ClaimsLosses_12 = profitAndLoss.Values.Find(a => a.StandardisedName == "Claims & Losses" && a.Tid == 12).ValueAssigned;
			ipl.LongTermCharges_14 = profitAndLoss.Values.Find(a => a.StandardisedName == "Long Term Charges" && a.Tid == 14).ValueAssigned;
			ipl.OtherClaimsLosses_15 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Claims & Losses" && a.Tid == 15).ValueAssigned;
			ipl.UnderwritingExpenseAcquisitionCost_16 = profitAndLoss.Values.Find(a => a.StandardisedName == "Underwriting Expense & Acquisition Cost" && a.Tid == 16).ValueAssigned;
			ipl.OtherOperatingExpense_26 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Operating Expense" && a.Tid == 26).ValueAssigned;
			ipl.OperatingIncome_Loss__27 = profitAndLoss.Values.Find(a => a.StandardisedName == "Operating Income (Loss)" && a.Tid == 27).ValueAssigned;
			ipl.NonOperatingIncome_Loss__28 = profitAndLoss.Values.Find(a => a.StandardisedName == "Non-Operating Income (Loss)" && a.Tid == 28).ValueAssigned;
			ipl.Income_Loss_fromAffiliates_29 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Affiliates" && a.Tid == 29).ValueAssigned;
			ipl.InterestExpense_30 = profitAndLoss.Values.Find(a => a.StandardisedName == "Interest Expense" && a.Tid == 30).ValueAssigned;
			ipl.OtherNonOperatingIncome_Loss__31 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Non-Operating Income (Loss)" && a.Tid == 31).ValueAssigned;
			ipl.PretaxIncome_Loss_Adjusted_32 = profitAndLoss.Values.Find(a => a.StandardisedName == "Pretax Income (Loss), Adjusted" && a.Tid == 32).ValueAssigned;
			ipl.AbnormalGains_Losses__33 = profitAndLoss.Values.Find(a => a.StandardisedName == "Abnormal Gains (Losses)" && a.Tid == 33).ValueAssigned;
			ipl.Merger_AcquisitionExpense_34 = profitAndLoss.Values.Find(a => a.StandardisedName == "Merger / Acquisition Expense" && a.Tid == 34).ValueAssigned;
			ipl.AbnormalDerivatives_35 = profitAndLoss.Values.Find(a => a.StandardisedName == "Abnormal Derivatives" && a.Tid == 35).ValueAssigned;
			ipl.DisposalofAssets_36 = profitAndLoss.Values.Find(a => a.StandardisedName == "Disposal of Assets" && a.Tid == 36).ValueAssigned;
			ipl.EarlyextinguishmentofDebt_37 = profitAndLoss.Values.Find(a => a.StandardisedName == "Early extinguishment of Debt" && a.Tid == 37).ValueAssigned;
			ipl.AssetWriteDown_38 = profitAndLoss.Values.Find(a => a.StandardisedName == "Asset Write-Down" && a.Tid == 38).ValueAssigned;
			ipl.ImpairmentofGoodwillIntangibles_39 = profitAndLoss.Values.Find(a => a.StandardisedName == "Impairment of Goodwill & Intangibles" && a.Tid == 39).ValueAssigned;
			ipl.SaleofBusiness_40 = profitAndLoss.Values.Find(a => a.StandardisedName == "Sale of Business" && a.Tid == 40).ValueAssigned;
			ipl.LegalSettlement_41 = profitAndLoss.Values.Find(a => a.StandardisedName == "Legal Settlement" && a.Tid == 41).ValueAssigned;
			ipl.RestructuringCharges_42 = profitAndLoss.Values.Find(a => a.StandardisedName == "Restructuring Charges" && a.Tid == 42).ValueAssigned;
			ipl.NetInvestmentLosses_43 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Investment Losses" && a.Tid == 43).ValueAssigned;
			ipl.ForeignExchange_44 = profitAndLoss.Values.Find(a => a.StandardisedName == "Foreign Exchange" && a.Tid == 44).ValueAssigned;
			ipl.OtherAbnormalItems_45 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Abnormal Items" && a.Tid == 45).ValueAssigned;
			ipl.PretaxIncome_Loss__46 = profitAndLoss.Values.Find(a => a.StandardisedName == "Pretax Income (Loss)" && a.Tid == 46).ValueAssigned;
			ipl.IncomeTax_Expense_Benefitnet_47 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income Tax (Expense) Benefit, net" && a.Tid == 47).ValueAssigned;
			ipl.CurrentIncomeTax_48 = profitAndLoss.Values.Find(a => a.StandardisedName == "Current Income Tax" && a.Tid == 48).ValueAssigned;
			ipl.DeferredIncomeTax_49 = profitAndLoss.Values.Find(a => a.StandardisedName == "Deferred Income Tax" && a.Tid == 49).ValueAssigned;
			ipl.TaxAllowance_Credit_50 = profitAndLoss.Values.Find(a => a.StandardisedName == "Tax Allowance/Credit" && a.Tid == 50).ValueAssigned;
			ipl.Income_Loss_fromAffiliatesnetoftaxes_51 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Affiliates, net of taxes" && a.Tid == 51).ValueAssigned;
			ipl.Income_Loss_fromContinuingOperations_52 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Continuing Operations" && a.Tid == 52).ValueAssigned;
			ipl.NetExtraordinaryGains_Losses__53 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Extraordinary Gains (Losses)" && a.Tid == 53).ValueAssigned;
			ipl.DiscontinuedOperations_54 = profitAndLoss.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 54).ValueAssigned;
			ipl.XOAccountingChargesOther_55 = profitAndLoss.Values.Find(a => a.StandardisedName == "XO & Accounting Charges & Other" && a.Tid == 55).ValueAssigned;
			ipl.Income_Loss_IncludingMinorityInterest_56 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) Including Minority Interest" && a.Tid == 56).ValueAssigned;
			ipl.MinorityInterest_57 = profitAndLoss.Values.Find(a => a.StandardisedName == "Minority Interest" && a.Tid == 57).ValueAssigned;
			ipl.NetIncome_58 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Income" && a.Tid == 58).ValueAssigned;
			ipl.PreferredDividends_59 = profitAndLoss.Values.Find(a => a.StandardisedName == "Preferred Dividends" && a.Tid == 59).ValueAssigned;
			ipl.OtherAdjustments_60 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Adjustments" && a.Tid == 60).ValueAssigned;
			ipl.NetIncomeAvailabletoCommonShareholders_61 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Income Available to Common Shareholders" && a.Tid == 61).ValueAssigned;

			return ipl;
		}

		private GeneralProfitAndLoss BuldGeneralProfitAndLoss(CompanyFinancials profitAndLoss)
		{
			var gpl = new GeneralProfitAndLoss
			{
				Calculated = profitAndLoss.Calculated,
				CompanyId = profitAndLoss.CompanyId,
				FYear = profitAndLoss.FYear,
				IndustryTemplate = profitAndLoss.IndustryTemplate,
				Statement = profitAndLoss.Statement
			};
			gpl.Revenue_1 = profitAndLoss.Values.Find(a => a.StandardisedName == "Revenue" && a.Tid == 1).ValueAssigned;
			gpl.SalesServicesRevenue_3 = profitAndLoss.Values.Find(a => a.StandardisedName == "Sales & Services Revenue" && a.Tid == 3).ValueAssigned;
			gpl.FinancingRevenue_5 = profitAndLoss.Values.Find(a => a.StandardisedName == "Financing Revenue" && a.Tid == 5).ValueAssigned;
			gpl.OtherRevenue_6 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Revenue" && a.Tid == 6).ValueAssigned;
			gpl.Costofrevenue_2 = profitAndLoss.Values.Find(a => a.StandardisedName == "Cost of revenue" && a.Tid == 2).ValueAssigned;
			gpl.CostofGoodsServices_7 = profitAndLoss.Values.Find(a => a.StandardisedName == "Cost of Goods & Services" && a.Tid == 7).ValueAssigned;
			gpl.CostofFinancingRevenue_8 = profitAndLoss.Values.Find(a => a.StandardisedName == "Cost of Financing Revenue" && a.Tid == 8).ValueAssigned;
			gpl.CostofOtherRevenue_9 = profitAndLoss.Values.Find(a => a.StandardisedName == "Cost of Other Revenue" && a.Tid == 9).ValueAssigned;
			gpl.GrossProfit_4 = profitAndLoss.Values.Find(a => a.StandardisedName == "Gross Profit" && a.Tid == 4).ValueAssigned;
			gpl.OtherOperatingIncome_10 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Operating Income" && a.Tid == 10).ValueAssigned;
			gpl.OperatingExpenses_11 = profitAndLoss.Values.Find(a => a.StandardisedName == "Operating Expenses" && a.Tid == 11).ValueAssigned;
			gpl.SellingGeneralAdministrative_12 = profitAndLoss.Values.Find(a => a.StandardisedName == "Selling, General & Administrative" && a.Tid == 12).ValueAssigned;
			gpl.SellingMarketing_13 = profitAndLoss.Values.Find(a => a.StandardisedName == "Selling & Marketing" && a.Tid == 13).ValueAssigned;
			gpl.GeneralAdministrative_14 = profitAndLoss.Values.Find(a => a.StandardisedName == "General & Administrative" && a.Tid == 14).ValueAssigned;
			gpl.ResearchDevelopment_15 = profitAndLoss.Values.Find(a => a.StandardisedName == "Research & Development" && a.Tid == 15).ValueAssigned;
			gpl.DepreciationAmortization_16 = profitAndLoss.Values.Find(a => a.StandardisedName == "Depreciation & Amortization" && a.Tid == 16).ValueAssigned;
			gpl.ProvisionForDoubtfulAccounts_17 = profitAndLoss.Values.Find(a => a.StandardisedName == "Provision For Doubtful Accounts" && a.Tid == 17).ValueAssigned;
			gpl.OtherOperatingExpense_18 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Operating Expense" && a.Tid == 18).ValueAssigned;
			gpl.OperatingIncome_Loss__19 = profitAndLoss.Values.Find(a => a.StandardisedName == "Operating Income (Loss)" && a.Tid == 19).ValueAssigned;
			gpl.NonOperatingIncome_Loss__20 = profitAndLoss.Values.Find(a => a.StandardisedName == "Non-Operating Income (Loss)" && a.Tid == 20).ValueAssigned;
			gpl.InterestExpensenet_21 = profitAndLoss.Values.Find(a => a.StandardisedName == "Interest Expense, net" && a.Tid == 21).ValueAssigned;
			gpl.InterestExpense_22 = profitAndLoss.Values.Find(a => a.StandardisedName == "Interest Expense" && a.Tid == 22).ValueAssigned;
			gpl.InterestIncome_23 = profitAndLoss.Values.Find(a => a.StandardisedName == "Interest Income" && a.Tid == 23).ValueAssigned;
			gpl.OtherInvestmentIncome_Loss__24 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Investment Income (Loss)" && a.Tid == 24).ValueAssigned;
			gpl.ForeignExchangeGain_Loss__25 = profitAndLoss.Values.Find(a => a.StandardisedName == "Foreign Exchange Gain (Loss)" && a.Tid == 25).ValueAssigned;
			gpl.Income_Loss_fromAffiliates_26 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Affiliates" && a.Tid == 26).ValueAssigned;
			gpl.OtherNonOperatingIncome_Loss__27 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Non-Operating Income (Loss)" && a.Tid == 27).ValueAssigned;
			gpl.PretaxIncome_Loss_Adjusted_28 = profitAndLoss.Values.Find(a => a.StandardisedName == "Pretax Income (Loss), Adjusted" && a.Tid == 28).ValueAssigned;
			gpl.AbnormalGains_Losses__29 = profitAndLoss.Values.Find(a => a.StandardisedName == "Abnormal Gains (Losses)" && a.Tid == 29).ValueAssigned;
			gpl.AcquiredInProcessRD_30 = profitAndLoss.Values.Find(a => a.StandardisedName == "Acquired In-Process R&D" && a.Tid == 30).ValueAssigned;
			gpl.Merger_AcquisitionExpense_31 = profitAndLoss.Values.Find(a => a.StandardisedName == "Merger / Acquisition Expense" && a.Tid == 31).ValueAssigned;
			gpl.AbnormalDerivatives_32 = profitAndLoss.Values.Find(a => a.StandardisedName == "Abnormal Derivatives" && a.Tid == 32).ValueAssigned;
			gpl.DisposalofAssets_33 = profitAndLoss.Values.Find(a => a.StandardisedName == "Disposal of Assets" && a.Tid == 33).ValueAssigned;
			gpl.EarlyextinguishmentofDebt_34 = profitAndLoss.Values.Find(a => a.StandardisedName == "Early extinguishment of Debt" && a.Tid == 34).ValueAssigned;
			gpl.AssetWriteDown_35 = profitAndLoss.Values.Find(a => a.StandardisedName == "Asset Write-Down" && a.Tid == 35).ValueAssigned;
			gpl.ImpairmentofGoodwillIntangibles_36 = profitAndLoss.Values.Find(a => a.StandardisedName == "Impairment of Goodwill & Intangibles" && a.Tid == 36).ValueAssigned;
			gpl.SaleofBusiness_37 = profitAndLoss.Values.Find(a => a.StandardisedName == "Sale of Business" && a.Tid == 37).ValueAssigned;
			gpl.LegalSettlement_38 = profitAndLoss.Values.Find(a => a.StandardisedName == "Legal Settlement" && a.Tid == 38).ValueAssigned;
			gpl.RestructuringCharges_39 = profitAndLoss.Values.Find(a => a.StandardisedName == "Restructuring Charges" && a.Tid == 39).ValueAssigned;
			gpl.SaleofandUnrealizedInvestments_40 = profitAndLoss.Values.Find(a => a.StandardisedName == "Sale of and Unrealized Investments" && a.Tid == 40).ValueAssigned;
			gpl.InsuranceSettlement_41 = profitAndLoss.Values.Find(a => a.StandardisedName == "Insurance Settlement" && a.Tid == 41).ValueAssigned;
			gpl.OtherAbnormalItems_42 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Abnormal Items" && a.Tid == 42).ValueAssigned;
			gpl.PretaxIncome_Loss__43 = profitAndLoss.Values.Find(a => a.StandardisedName == "Pretax Income (Loss)" && a.Tid == 43).ValueAssigned;
			gpl.IncomeTax_Expense_Benefitnet_44 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income Tax (Expense) Benefit, net" && a.Tid == 44).ValueAssigned;
			gpl.CurrentIncomeTax_45 = profitAndLoss.Values.Find(a => a.StandardisedName == "Current Income Tax" && a.Tid == 45).ValueAssigned;
			gpl.DeferredIncomeTax_46 = profitAndLoss.Values.Find(a => a.StandardisedName == "Deferred Income Tax" && a.Tid == 46).ValueAssigned;
			gpl.TaxAllowance_Credit_47 = profitAndLoss.Values.Find(a => a.StandardisedName == "Tax Allowance/Credit" && a.Tid == 47).ValueAssigned;
			gpl.Income_Loss_fromAffiliatesnetoftaxes_48 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Affiliates, net of taxes" && a.Tid == 48).ValueAssigned;
			gpl.Income_Loss_fromContinuingOperations_49 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) from Continuing Operations" && a.Tid == 49).ValueAssigned;
			gpl.NetExtraordinaryGains_Losses__50 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Extraordinary Gains (Losses)" && a.Tid == 50).ValueAssigned;
			gpl.DiscontinuedOperations_51 = profitAndLoss.Values.Find(a => a.StandardisedName == "Discontinued Operations" && a.Tid == 51).ValueAssigned;
			gpl.XOAccountingChargesOther_52 = profitAndLoss.Values.Find(a => a.StandardisedName == "XO & Accounting Charges & Other" && a.Tid == 52).ValueAssigned;
			gpl.Income_Loss_IncludingMinorityInterest_53 = profitAndLoss.Values.Find(a => a.StandardisedName == "Income (Loss) Including Minority Interest" && a.Tid == 53).ValueAssigned;
			gpl.MinorityInterest_54 = profitAndLoss.Values.Find(a => a.StandardisedName == "Minority Interest" && a.Tid == 54).ValueAssigned;
			gpl.NetIncome_55 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Income" && a.Tid == 55).ValueAssigned;
			gpl.PreferredDividends_56 = profitAndLoss.Values.Find(a => a.StandardisedName == "Preferred Dividends" && a.Tid == 56).ValueAssigned;
			gpl.OtherAdjustments_57 = profitAndLoss.Values.Find(a => a.StandardisedName == "Other Adjustments" && a.Tid == 57).ValueAssigned;
			gpl.NetIncomeAvailabletoCommonShareholders_58 = profitAndLoss.Values.Find(a => a.StandardisedName == "Net Income Available to Common Shareholders" && a.Tid == 58).ValueAssigned;
			return gpl;
		}

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

		private async Task PopulateBankBSAsync(List<CompanyFinancials> balanceSheets, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, BankBsWorkSheetName);
			var bbsl = new List<BankBalanceSheet>();
			foreach (var balanceSheet in balanceSheets)
			{
				BankBalanceSheet bbs = BuildBankBalanceSheet(balanceSheet);
				bbsl.Add(bbs);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(bbsl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateBankCFAsync(List<CompanyFinancials> cashFlows, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, BankCfWorkSheetName);
			var bcfl = new List<BankCashFlow>();
			foreach (var cashFlow in cashFlows)
			{
				BankCashFlow bbs = BuildBankCashFlow(cashFlow);
				bcfl.Add(bbs);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(bcfl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateBankPLAsync(List<CompanyFinancials> profitAndLosss, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, BankPlWorkSheetName);
			var bpll = new List<BankProfitAndLoss>();
			foreach (var profitAndLoss in profitAndLosss)
			{
				BankProfitAndLoss bpl = BuildBankProfitLoss(profitAndLoss);
				bpll.Add(bpl);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(bpll, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateGeneralBSAsync(List<CompanyFinancials> balanceSheets, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, GeneralBsWorkSheetName);
			var gbsl = new List<GeneralBalanceSheet>();
			foreach (var balanceSheet in balanceSheets)
			{
				GeneralBalanceSheet gbs = BuildGeneralBalanceSheet(balanceSheet);
				gbsl.Add(gbs);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(gbsl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateGeneralCFAsync(List<CompanyFinancials> cashFlows, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, GeneralCfWorkSheetName);
			var gcfl = new List<GeneralCashFlow>();
			foreach (var cashFlow in cashFlows)
			{
				GeneralCashFlow gcf = BuildGeneralCashFlow(cashFlow);
				gcfl.Add(gcf);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(gcfl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateGeneralPLAsync(List<CompanyFinancials> profitAndLosss, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, GeneralPlWorkSheetName);
			var gpll = new List<GeneralProfitAndLoss>();
			foreach (var profitAndLoss in profitAndLosss)
			{
				var gpl = BuldGeneralProfitAndLoss(profitAndLoss);
				gpll.Add(gpl);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(gpll, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateInsuranceBSAsync(List<CompanyFinancials> balanceSheets, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, InsuranceBsWorkSheetName);
			var ibsl = new List<InsuranceBalanceSheet>();
			foreach (var balanceSheet in balanceSheets)
			{
				InsuranceBalanceSheet ibs = BuildInsuranceBalanceSheet(balanceSheet);
				ibsl.Add(ibs);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(ibsl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateInsuranceCFAsync(List<CompanyFinancials> cashFlows, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, InsuranceCfWorkSheetName);
			var icfl = new List<InsuranceCashFlow>();
			foreach (var cashFlow in cashFlows)
			{
				InsuranceCashFlow icf = BuildInsurnaceCashFlow(cashFlow);
				icfl.Add(icf);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(icfl, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task PopulateInsurancePLAsync(List<CompanyFinancials> profitAndLosss, string outFile, ExcelPackage package)
		{
			ExcelWorksheet worksheet = CreateSheet(outFile, package, InsurancePlWorkSheetName);
			var iplls = new List<InsuranceProfitAndLoss>();
			foreach (var profitAndLoss in profitAndLosss)
			{
				InsuranceProfitAndLoss ipl = BuildInsurancePrfitAndLoss(profitAndLoss);
				iplls.Add(ipl);
			}
			var row = worksheet.Dimension == null ? 1 : worksheet.Dimension.End.Row;
			worksheet.Cells[row, 1].LoadFromCollection(iplls, (row == 1));

			var fullData = package.GetAsByteArray();
			await File.WriteAllBytesAsync(outFile, fullData);
		}

		private async Task UpdateIndustryTemplate(string industryTemplate, string outFile, string simId)
		{
			var wloc = new WriteListOfCompanies(_logger);
			await wloc.UpdateIndustryTemplateAsync(simId, industryTemplate, outFile);
		}

		private async Task WriteBalanceSheetDataAsync(List<CompanyFinancials> balanceSheets, string outFile)
		{
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

		private async Task WriteCashFlowDataAsync(List<CompanyFinancials> cashFlows, string outFile)
		{
			using (var package = new ExcelPackage())
			{
				if (cashFlows.First().IndustryTemplate.Equals("insurances"))
				{
					_logger.LogInformation("Processing Cash flow for Insurance");
					await PopulateInsuranceCFAsync(cashFlows, outFile, package);
				}
				else if (cashFlows.First().IndustryTemplate.Equals("general"))
				{
					_logger.LogInformation("Processing General");
					await PopulateGeneralCFAsync(cashFlows, outFile, package);
				}
				else if (cashFlows.First().IndustryTemplate.Equals("banks"))
				{
					_logger.LogInformation("Processing Bank");
					await PopulateBankCFAsync(cashFlows, outFile, package);
				}
				else
				{
					_logger.LogError($"Unknown Industry template {cashFlows.First().IndustryTemplate}");
				}
			}
		}

		private async Task WriteProfitAndLossAsync(List<CompanyFinancials> profitAndLosss, string outFile)
		{
			using (var package = new ExcelPackage())
			{
				if (profitAndLosss.First().IndustryTemplate.Equals("insurances"))
				{
					_logger.LogInformation("Processing Cash flow for Insurance");
					await PopulateInsurancePLAsync(profitAndLosss, outFile, package);
				}
				else if (profitAndLosss.First().IndustryTemplate.Equals("general"))
				{
					_logger.LogInformation("Processing General");
					await PopulateGeneralPLAsync(profitAndLosss, outFile, package);
				}
				else if (profitAndLosss.First().IndustryTemplate.Equals("banks"))
				{
					_logger.LogInformation("Processing Bank");
					await PopulateBankPLAsync(profitAndLosss, outFile, package);
				}
				else
				{
					_logger.LogError($"Unknown Industry template {profitAndLosss.First().IndustryTemplate}");
				}
			}
		}

		#endregion Private Methods

	}
}