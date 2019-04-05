using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using MongoDB.Driver;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite.Tools
{
	public class HandleFinacials
	{

		#region Private Fields

		private readonly DBConnectionHandler<CompanyFinancialsMd> _dbconCompany;
		private readonly IMongoCollection<CompanyFinancialsMd> _statementConnection;

		/// <summary>
		/// Get statements from data provider and insert it to database.
		/// </summary>
		/// <param name="simId">The sim identifier.</param>
		/// <returns></returns>
		public HandleFinacials()
		{
			_dbconCompany = new DBConnectionHandler<CompanyFinancialsMd>();
			_statementConnection = _dbconCompany.ConnectToDatabase("CompanyFinancials");
		}

		#endregion Public Constructors

		#region Public Methods

		/// <summary>
		/// Get statements from data provider and insert it to database.
		/// </summary>
		/// <param name="simId">The sim identifier.</param>
		/// <returns></returns>
		public async Task<bool> UpdateStatements(string simId)
		{
			if (string.IsNullOrWhiteSpace(simId))
			{
				return false;
			}
			var hcl = new HandleCompanyList();
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
			var oldcfML = _dbconCompany.Get().Where(o => o.CompanyId.Equals(simId)).ToList();
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
				var returnValue = await _dbconCompany.UpdateMultipe(cfMdl);
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

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Obtains the company financial asynchronous.
		/// </summary>
		/// <param name="simId">The SIM identifier.</param>
		/// <returns></returns>
		private async Task<List<CompanyFinancials>> ObtainCompanyFinancilasAsync(string simId)
		{
			var lf = new LoggerFactory();
			var loggerLos = LoggerFactoryExtensions.CreateLogger(lf, typeof(Logger<ListOfStatements>));
			var loS = new ListOfStatements(loggerLos);
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
			var loggerRI = LoggerFactoryExtensions.CreateLogger(lf, typeof(Logger<DownloadReportableItems>));
			var dri = new DownloadReportableItems(loggerRI);
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
									  where !(cfMdl.Any(c => c.FYear == o.FYear))
									  select o).FirstOrDefault();
				if (recordsToBeDeleted != null)
				{
					await _dbconCompany.Remove(recordsToBeDeleted.Id);
					oldcfML.Remove(recordsToBeDeleted);
				}
			} while (recordsToBeDeleted != null);
		}

		#endregion Private Methods
	}
}