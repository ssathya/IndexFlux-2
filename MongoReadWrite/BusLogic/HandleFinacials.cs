using AutoMapper;
using HandleSimFin.Methods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using MongoDB.Driver;
using MongoReadWrite.BusLogic;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite.BusLogic
{
	public class HandleFinacials
	{
		private readonly IDBConnectionHandler<CompanyFinancialsMd> _dbconCompany;
		private readonly IMongoCollection<CompanyFinancialsMd> _statementConnection;
		private readonly ILogger<HandleFinacials> _logger;

		/// <summary>
		/// Get statements from data provider and insert it to database.
		/// </summary>
		/// <param name="simId">The sim identifier.</param>
		/// <returns></returns>
		public HandleFinacials(IDBConnectionHandler<CompanyFinancialsMd> dbconCompany, ILogger<HandleFinacials> logger)
		{
			_dbconCompany = dbconCompany;
			_statementConnection = _dbconCompany.ConnectToDatabase("CompanyFinancials");
			_logger = logger;
		}

		internal List<CompanyFinancials> ReadFinanceValues(string simId, string industryTemplate)
		{
			//var financial = _dbconCompany.Get().Where(cf => cf.CompanyId.Equals(simId));
			var financials = _dbconCompany.Get(cf => cf.CompanyId.Equals(simId));

			var cfLst = Mapper.Map<IEnumerable<CompanyFinancialsMd>, IEnumerable<CompanyFinancials>>(financials).ToList();
			Dictionary<int, Dictionary<string, long>> valueToUse;
			switch (industryTemplate)
			{
				case "general":
					valueToUse = GeneralFlattenData(cfLst);
					break;
				default:
					break;
			}
						
			return cfLst;
		}

		private static Dictionary<int, Dictionary<string, long>> GeneralFlattenData(List<CompanyFinancials> cfLst)
		{
			var valueRef = new Dictionary<int, Dictionary<string, long>>();
			var years = cfLst.Select(x => x.FYear).Distinct();
			foreach (var year in years)
			{
				valueRef.Add(year, new Dictionary<string, long>());
				var bsKeys = new string[] { "Total Assets", "Total Current Assets", "Total Current Liabilities", "Total Equity", "Total Liabilities", "Total Liabilities & Equity", "Total Noncurrent Assets", "Total Noncurrent Liabilities" };
				foreach (var bsKey in bsKeys)
				{										
					valueRef[year].Add(bsKey, (from bs in cfLst.Where(i => i.Statement == StatementType.BalanceSheet && i.FYear == year)
											   select (long)(bs.Values.Find(a => a.StandardisedName.Equals(bsKey)).ValueChosen)).FirstOrDefault());
				}
				var cfKeys = new string[] { "Cash from Financing Activities", "Cash from Investing Activities", "Cash from Operating Activities", "Net Changes in Cash" };
				foreach (var cfKey in cfKeys)
				{
					valueRef[year].Add(cfKey, (from cf in cfLst.Where(i => i.Statement == StatementType.CashFlow && i.FYear == year)
											   select (long)(cf.Values.Find(a => a.StandardisedName.Equals(cfKey)).ValueChosen)).FirstOrDefault());
				}
				var plKeys = new string[] { "Gross Profit", "Net Income Available to Common Shareholders", "Operating Income (Loss)", "Pretax Income (Loss)" };
				foreach (var plKey in plKeys)
				{
					valueRef[year].Add(plKey, (from pl in cfLst.Where(i => i.Statement == StatementType.ProfitLoss && i.FYear == year)
											   select (long)(pl.Values.Find(a => a.StandardisedName.Equals(plKey)).ValueChosen)).FirstOrDefault());
				}								
			}
			foreach (var key in valueRef.Keys)
			{
				Console.WriteLine($"Year : {key}");
				var format = "#,##0";
				foreach (var values in valueRef[key])
				{
					Console.WriteLine($"{values.Key} => {values.Value.ToString(format)}");
				}
			}

			return valueRef;
		}

		/// <summary>
		/// Get statements from data provider and insert it to database.
		/// </summary>
		/// <param name="simId">The sim identifier.</param>
		/// <returns></returns>
		public async Task<bool> UpdateStatements(string simId)
		{
			if (simId.IsNullOrWhiteSpace())
			{
				return false;
			}
			var hcl = Program.Provider.GetService<HandleCompanyList>();
			var cd = hcl.GetCompanyDetails(simId);
			if (cd.LastUpdate != null && ((TimeSpan)(DateTime.Now - cd.LastUpdate)).Days < 30)
			{
				return true;
			}

			var companyFinancials = await ObtainCompanyFinancilasAsync(simId);
			if (companyFinancials == null || companyFinancials.Count == 0)
			{
				return false;
			}
			var cfMdl = new List<CompanyFinancialsMd>();
			var oldcfML = _dbconCompany.Get(o => o.CompanyId.Equals(simId)).ToList();
			/*
			 * Not sure what the wizard did was the right thing. Original code was
			 * foreach (var companyFinancial in companyFinancials)
					{
					var oldcf = oldcfML.Where(o => o.CompanyId.Equals(companyFinancial.CompanyId)
						&& o.FYear == companyFinancial.FYear
						&& o.Statement == companyFinancial.Statement).FirstOrDefault();
					cfMdl.Add(new CompanyFinancialsMd(companyFinancial));
					if (oldcf != null)
					{
						cfMdl.Last().Id = oldcf.Id;
					}
			}
			 */
			foreach (var (companyFinancial, oldcf) in from companyFinancial in companyFinancials
													  let oldcf = oldcfML.Where(o => o.CompanyId.Equals(companyFinancial.CompanyId)
															&& o.FYear == companyFinancial.FYear
															&& o.Statement == companyFinancial.Statement).FirstOrDefault()
													  select (companyFinancial, oldcf))
			{
				cfMdl.Add(new CompanyFinancialsMd(companyFinancial));
				if (oldcf != null)
				{
					cfMdl.Last().Id = oldcf.Id;
				}
			}

			try
			{
				var returnValue = await _dbconCompany.UpdateMultiple(cfMdl);
				if (returnValue == false)
				{
					return false;
				}
				await RemoveUnwantedRecords(cfMdl, oldcfML);

				returnValue = await hcl.UpdateCompanyDetailAsync(simId, cfMdl.First().IndustryTemplate);
				return returnValue;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in method HandleFinacials:UpdateStatments\n{ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Obtains the company financial asynchronous.
		/// </summary>
		/// <param name="simId">The SIM identifier.</param>
		/// <returns></returns>
		private async Task<List<CompanyFinancials>> ObtainCompanyFinancilasAsync(string simId)
		{
			var loS = Program.Provider.GetService<ListOfStatements>();
			StatementList statementList = await loS.FetchStatementList(simId, IdentifyerType.SimFinId);
			statementList = loS.ExtractYearEndReports(statementList);
			if (statementList.Bs.Count == 0 || statementList.Cf.Count == 0
				|| statementList.Pl.Count == 0)
			{
				return null;
			}
			statementList.Bs = statementList.Bs.OrderByDescending(b => b.Fyear).Take(4).ToList();
			statementList.Cf = statementList.Cf.OrderByDescending(c => c.Fyear).Take(4).ToList();
			statementList.Pl = statementList.Pl.OrderByDescending(p => p.Fyear).Take(4).ToList();
			if (statementList.Bs.Count() != 4
				|| statementList.Cf.Count() != 4
				|| statementList.Pl.Count() != 4)
			{
				Console.WriteLine("Something is wrong");
			}
			var dri = Program.Provider.GetService<DownloadReportableItems>();
			var companyFinancials = await dri.DownloadFinancialsAsync(statementList);
			return companyFinancials;
		}

		/// <summary>
		/// Removes stale unwanted records.
		/// </summary>
		/// <param name="cfMdl">Statements obtained from external data source</param>
		/// <param name="oldcfML">Statements in database.</param>
		/// <returns></returns>
		private async Task RemoveUnwantedRecords(List<CompanyFinancialsMd> cfMdl, List<CompanyFinancialsMd> oldcfML)
		{
			CompanyFinancialsMd recordsToBeDeleted;
			//Do statement is sufficient here. 99% of the time there will not be more than one
			//record to delete. Why create a list that will contain 0 or 1 elements always.
			do
			{
				recordsToBeDeleted = (from o in oldcfML
									  where !cfMdl.Any(c => c.FYear == o.FYear)
									  select o).FirstOrDefault();
				if (recordsToBeDeleted != null)
				{
					await _dbconCompany.Remove(recordsToBeDeleted.Id);
					oldcfML.Remove(recordsToBeDeleted);
				}
			} while (recordsToBeDeleted != null);
		}
	}
}