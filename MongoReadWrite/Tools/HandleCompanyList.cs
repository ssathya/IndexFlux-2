﻿using HandleSimFin.Methods;
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




			//var insertStatus = await _dbconCompany.Create(allCompanies);
			var insertStatus = await _dbconCompany.UpdateMultipe(allCompanies);
			if (insertStatus)
			{
				return allCompanies;
			}			
			return null;
		}
		public async Task<List<CompanyDetailMd>> GetAllCompaniesFromDbAsync()
		{
			var returnValue = _dbconCompany.Get().ToList();
			if (returnValue.Count <= 10)
			{
				returnValue = await GetAllCompaniesAsync();
			}
			return returnValue;
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

	}
}