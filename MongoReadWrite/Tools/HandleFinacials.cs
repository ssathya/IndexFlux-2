using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using MongoDB.Driver;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoReadWrite.Tools
{
    public class HandleFinacials
    {
		private readonly DBConnectionHandler<CompanyFinancialsMd> _dbconCompany;
		private readonly IMongoCollection<CompanyFinancialsMd> _statementConnection;

		public HandleFinacials()
		{
			_dbconCompany = new DBConnectionHandler<CompanyFinancialsMd>();
			_statementConnection = _dbconCompany.ConnectToDatabase("CompanyFinancials");
		}

		public async Task<bool> UpdateStatements(string simId)
		{

			if (string.IsNullOrWhiteSpace(simId))
			{
				return false;
			}
			var hcl = new HandleCompanyList();
			var cd = hcl.GetCompanyDetails(simId);
			if (cd.LastUpdate != null && ((TimeSpan)(DateTime.Now - cd.LastUpdate)).Days < 7)
			{
				return true;
			}

			var companyFinancials = await ObtainCompanyFinancilasAsync(simId);
			if (companyFinancials == null || companyFinancials.Count == 0)
			{
				return false;
			}
			var cfMdl = new List<CompanyFinancialsMd>();
			foreach (var companyFinancial in companyFinancials)
			{
				cfMdl.Add(new CompanyFinancialsMd(companyFinancial));
			}
			try
			{
				var returnValue = await _dbconCompany.Create(cfMdl);
				if (returnValue == false)
				{
					return false;
				}
				
				returnValue = await hcl.UpdateCompanyDetailAsync(simId, cfMdl.First().IndustryTemplate);
				return returnValue;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in method HandleFinacials:UpdateStatments\n{ex.Message}");
				return false;
			}
		}
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
	}
}
