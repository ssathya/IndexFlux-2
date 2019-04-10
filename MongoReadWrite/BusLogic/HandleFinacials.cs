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
		private readonly HandleCompanyList _hcl;
		private readonly ListOfStatements _los;
		private readonly DownloadReportableItems _dri;

		/// <summary>
		/// Get statements from data provider and insert it to database.
		/// </summary>
		/// <param name="simId">The sim identifier.</param>
		/// <returns></returns>
		public HandleFinacials(IDBConnectionHandler<CompanyFinancialsMd> dbconCompany, 
			ILogger<HandleFinacials> logger, 
			HandleCompanyList hcl, 
			ListOfStatements los, 
			DownloadReportableItems dri)
		{
			_dbconCompany = dbconCompany;
			_statementConnection = _dbconCompany.ConnectToDatabase("CompanyFinancials");
			_logger = logger;
			_hcl = hcl;
			_los = los;
			_dri = dri;
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
			
			var cd = _hcl.GetCompanyDetails(simId);
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

				returnValue = await _hcl.UpdateCompanyDetailAsync(simId, cfMdl.First().IndustryTemplate);
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
			StatementList statementList = await _los.FetchStatementList(simId, IdentifyerType.SimFinId);
			statementList = _los.ExtractYearEndReports(statementList);
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
			
			var companyFinancials = await _dri.DownloadFinancialsAsync(statementList);
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