using AutoMapper;
using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using MongoDB.Driver;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;


namespace MongoReadWrite.Tools
{
	public class HandleCompanyList
	{

		#region Private Fields

		private readonly IMongoCollection<CompanyDetailMd> _companyConnection;
		private readonly IDBConnectionHandler<CompanyDetailMd> _dbconCompany;
		private List<CompanyDetailMd> allCompanies;

		#endregion Private Fields


		#region Public Constructors

		public HandleCompanyList(ILogger<HandleCompanyList> logger, IDBConnectionHandler<CompanyDetailMd> dbconCompany)
		{			
			_dbconCompany = dbconCompany;
			_companyConnection = _dbconCompany.ConnectToDatabase("CompanyDetail");
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<List<CompanyDetail>> GetAllCompaniesAsync()
		{

			var dLF = Program.Provider.GetService<DownloadListedFirms>();
			var tmpList = await dLF.GetCompanyList();
			allCompanies = new List<CompanyDetailMd>();
			var dbCompanies = _dbconCompany.Get().ToList();
			if (dbCompanies.Count() < 100 || dbCompanies.Where(x => string.IsNullOrWhiteSpace(x.IndustryTemplate)).Count() < 20)
			{
				var deleteStatus = await _dbconCompany.RemoveAll();
				if (deleteStatus == false)
				{
					return null;
				}
			}
			dbCompanies = _dbconCompany.Get().ToList();
			foreach (var company in tmpList)
			{
				var dbCompany = dbCompanies.Where(x => x.SimId == company.SimId).FirstOrDefault();
				if (dbCompany != null)
				{
					allCompanies.Add(dbCompany);
				}
				else
				{
					allCompanies.Add(new CompanyDetailMd(company));
				}
			}
			var insertStatus = await _dbconCompany.UpdateMultipe(allCompanies);
			if (insertStatus)
			{
				var listOfAllCompanies = (Mapper.Map<List<CompanyDetailMd>, List<CompanyDetail>>(allCompanies));
				return listOfAllCompanies;
			}
			return null;
		}

		public async Task<List<CompanyDetail>> GetAllCompaniesFromDbAsync()
		{
			List<CompanyDetail> compDetailList;
			var savedValue = _dbconCompany.Get().ToList();
			if (savedValue.Count <= 10)
			{
				compDetailList = await GetAllCompaniesAsync();
				return compDetailList;
			}
			compDetailList = (Mapper.Map<List<CompanyDetailMd>, List<CompanyDetail>>(savedValue));
			return compDetailList;
		}

		public CompanyDetail GetCompanyDetails(string simId)
		{
			var selectedRecord = _dbconCompany.Get().Where(cd => cd.SimId.Equals(simId)).FirstOrDefault();
			if (selectedRecord == null)
			{
				return null;
			}
			var returnValue = new CompanyDetail
			{
				IndustryTemplate = selectedRecord.IndustryTemplate,
				LastUpdate = selectedRecord.LastUpdate,
				Name = selectedRecord.Name,
				SimId = selectedRecord.SimId,
				Ticker = selectedRecord.Ticker
			};
			return returnValue;
		}

		public async Task<bool> UpdateCompanyDetailAsync(string simId, string industryTemplate, DateTime? updateTime = null)
		{
			var selectedRecord = _dbconCompany.Get().Where(cd => cd.SimId.Equals(simId)).FirstOrDefault();
			if (selectedRecord == null)
			{
				return false;
			}
			selectedRecord.IndustryTemplate = industryTemplate;
			selectedRecord.LastUpdate = updateTime == null ? DateTime.Now : updateTime;
			return await _dbconCompany.Update(selectedRecord.Id, selectedRecord);
		}

		#endregion Public Methods
	}
}