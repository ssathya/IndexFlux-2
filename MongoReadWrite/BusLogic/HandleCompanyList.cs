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
using MongoReadWrite.Extensions;
using Microsoft.Extensions.Configuration;
using HandleSimFin.Helpers;

namespace MongoReadWrite.BusLogic
{
	public class HandleCompanyList
	{


		#region Private Fields

		private readonly IMongoCollection<CompanyDetailMd> _companyConnection;
		private readonly IDBConnectionHandler<CompanyDetailMd> _dbconCompany;
		private readonly DownloadListedFirms _dlf;
		private readonly ILogger<HandleCompanyList> _logger;
		private List<CompanyDetailMd> allCompanies;

		#endregion Private Fields


		#region Public Constructors

		public HandleCompanyList(ILogger<HandleCompanyList> logger, 
			IDBConnectionHandler<CompanyDetailMd> 
			dbconCompany, DownloadListedFirms dlf)
		{
			_dbconCompany = dbconCompany;
			_companyConnection = _dbconCompany.ConnectToDatabase("CompanyDetail");
			_dlf = dlf;
			_logger = logger;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<List<CompanyDetail>> GetAllCompaniesAsync()
		{
						
			var tmpList = await _dlf.GetCompanyList();
			allCompanies = new List<CompanyDetailMd>();
			var dbCompanies = _dbconCompany.Get().ToList();
			if (dbCompanies.Count() < 100 || dbCompanies.Where(x => x.IndustryTemplate.IsNullOrWhiteSpace()).Count() < 20)
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
					dbCompany.Name = company.Name;
					dbCompany.Ticker = company.Ticker;
					allCompanies.Add(dbCompany);
				}
				else
				{
					allCompanies.Add(new CompanyDetailMd(company));
				}
			}
			var insertStatus = await _dbconCompany.UpdateMultiple(allCompanies);
			if (insertStatus)
			{
				var listOfAllCompanies = Mapper.Map<List<CompanyDetailMd>, List<CompanyDetail>>(allCompanies);
				return listOfAllCompanies;
			}
			return null;
		}

		public async Task<List<CompanyDetail>> GetAllCompaniesFromDbAsync()
		{
			_logger.LogTrace("Starting GetAllCompaniesFromDbAsync ");
			List<CompanyDetail> compDetailList;
			var savedValue = _dbconCompany.Get().ToList();
			//refresh list if it has too little data or at least once a month
			if (savedValue.Count <= 1000 || TodayIsFirstSaturday())
			{
				compDetailList = await GetAllCompaniesAsync();
				return compDetailList;
			}
			compDetailList = Mapper.Map<List<CompanyDetailMd>, List<CompanyDetail>>(savedValue);
			_logger.LogTrace("Exiting GetAllCompaniesFromDbAsync");
			return compDetailList;
		}

		public CompanyDetail GetCompanyDetails(string simId)
		{
			var selectedRecord = _dbconCompany.Get(cd => cd.SimId.Equals(simId)).FirstOrDefault();
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
			var selectedRecord = _dbconCompany.Get(cd => cd.SimId.Equals(simId)).FirstOrDefault();
			if (selectedRecord == null)
			{
				return false;
			}
			selectedRecord.IndustryTemplate = industryTemplate;
			selectedRecord.LastUpdate = updateTime == null ? DateTime.Now : updateTime;
			return await _dbconCompany.Update(selectedRecord.Id, selectedRecord);
		}

		#endregion Public Methods


		#region Private Methods

		private bool TodayIsFirstSaturday()
		{
			var today = DateTime.Today;
			if (today.DayOfWeek == DayOfWeek.Saturday && today.Day <= 7)
			{
				return true;
			}
			return false;
		}

		#endregion Private Methods
	}
}