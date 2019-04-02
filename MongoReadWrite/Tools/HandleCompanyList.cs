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
	
	public class HandleCompanyList
    {
		private List<CompanyDetailMd> allCompanies;

		private readonly DBConnectionHandler<CompanyDetailMd> _dbconCompany;
		private readonly IMongoCollection<CompanyDetailMd> _companyConnection;

		public HandleCompanyList()
		{
			_dbconCompany = new DBConnectionHandler<CompanyDetailMd>();
			_companyConnection = _dbconCompany.ConnectToDatabase("CompanyDetail");
		}
		public async Task<List<CompanyDetailMd>> GetAllCompaniesAsync()
		{
			var lf = new LoggerFactory();
			var localLogger = LoggerFactoryExtensions.CreateLogger(lf, typeof(Logger<DownloadListedFirms>));			
			var dLF = new DownloadListedFirms(localLogger);
			var tmpList = await dLF.GetCompanyList();
			allCompanies = new List<CompanyDetailMd>();
			
			foreach (var company in tmpList)
			{
				allCompanies.Add(new CompanyDetailMd(company));
			}
			var savedList = _dbconCompany.Get();
			foreach (var company in allCompanies)
			{
				var oldEntry = savedList.FirstOrDefault(r => r.Ticker == company.Ticker);
				if (oldEntry == null)
				{
					await _dbconCompany.Create(company);
				}
				else
				{
					company.Id = oldEntry.Id;
					await _dbconCompany.Update(oldEntry.Id, company);
				}
			}
			return allCompanies;
		}


	}
}
