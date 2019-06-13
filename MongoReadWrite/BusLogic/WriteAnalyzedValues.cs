using AutoMapper;
using Microsoft.Extensions.Logging;
using Models;
using MongoDB.Driver;
using MongoHandler.Utils;
using MongoReadWrite.Extensions;
using SpreadSheetReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite.BusLogic
{
	public class WriteAnalyzedValues
	{
		#region Private Fields

		private readonly IDBConnectionHandler<CompanyFinancialsMd> _dbconCompany;
		private readonly IDBConnectionHandler<PiotroskiScoreMd> _dbpiScore;
		private readonly IMongoCollection<PiotroskiScoreMd> _dbpiScoreConnection;
		private readonly DataFileReader _dfr;
		private readonly HandleCompanyList _hcl;
		private readonly ILogger<WriteAnalyzedValues> _logger;
		private readonly IMongoCollection<CompanyFinancialsMd> _statementConnection;

		#endregion Private Fields

		#region Public Constructors

		public WriteAnalyzedValues(IDBConnectionHandler<CompanyFinancialsMd> dbconCompany,
			IDBConnectionHandler<PiotroskiScoreMd> dbpiScore,
			ILogger<WriteAnalyzedValues> logger,
			HandleCompanyList hcl,
			DataFileReader dfr)
		{
			_dbconCompany = dbconCompany;
			_statementConnection = _dbconCompany.ConnectToDatabase("CompanyFinancials");

			_dbpiScore = dbpiScore;
			_dbpiScoreConnection = _dbpiScore.ConnectToDatabase("PiotroskiScore");

			_logger = logger;
			_hcl = hcl;
			_dfr = dfr;
		}

		#endregion Public Constructors

		#region Public Methods

		public async Task UpdateAnalysis()
		{
			var ac = await _hcl.GetAllCompaniesFromDbAsync();

			try
			{
				await _dfr.ParseKeyFinanceFromS3(ServiceExtensions.BucketName, ServiceExtensions.Region, "1. Key Ratios.xlsx");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error while reading excel file\n{ex.Message}");
				return;
			}
			var result = await _dbpiScore.RemoveAll();
			_logger.LogDebug($"Excel file contains {_dfr.dataCollection.Count}");
			List<DataCollection> dcl = _dfr.dataCollection;
			List<CompanyDetail> allCompanies = await _hcl.GetAllCompaniesFromDbAsync();
			var counter = 0;
			var newValues = new List<PiotroskiScoreMd>();
			var updateTime = DateTime.Now;
			foreach (var dc in dcl)
			{
				if (string.IsNullOrWhiteSpace(dc.CompanyName) || string.IsNullOrWhiteSpace(dc.Ticker))
				{
					_logger.LogDebug($"Skipping {dc.CompanyName} => {dc.Ticker} due to missing details");
					continue;
				}
				var selected = (from comp in allCompanies
								where (comp.Ticker == dc.Ticker && comp.Name == dc.CompanyName)
								select comp).FirstOrDefault();
				if (selected == null)
				{
					_logger.LogDebug("Referential integrity error");
					_logger.LogDebug($"Did not find {dc.CompanyName} => {dc.Ticker} in Company Details");
					continue;
				}

				var ProfitablityRatios = new Dictionary<string, decimal>
				{
					{ "Gross Margin", (decimal)dc.GrossMargin },
					{ "Operating Margin", (decimal)dc.OperatingMargin },
					{ "Net Profit Margin", (decimal)dc.NetMargin },
					{ "Return on Equity", (decimal)dc.ReturnOnEquity },
					{ "Return on Assets", (decimal)dc.ReturnOnAssets }
				};
				var newValue = new PiotroskiScore
				{
					SimId = selected.SimId,
					FYear = DateTime.Now.Year,
					Rating = dc.PiotroskiScoreCurrent,
					EBITDA = (long)dc.EbitdaCurrent,
					LastUpdate = DateTime.Now,
					ProfitablityRatios = ProfitablityRatios,
					Revenue = dc.Revenue,
					Ticker = dc.Ticker
				};
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				newValue.ProfitablityRatios.Clear();
				UpdateAnalysis(newValue, DateTime.Now.Year - 1, dc.PiotroskiScore1YrAgo, dc.Ebitda1YrAgo);
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				UpdateAnalysis(newValue, DateTime.Now.Year - 2, dc.PiotroskiScore2YrAgo, dc.Ebitda2YrAgo);
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				UpdateAnalysis(newValue, DateTime.Now.Year - 3, dc.PiotroskiScore3YrAgo, dc.Ebitda3YrAgo);
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				selected.LastUpdate = updateTime;
				await _hcl.UpdateCompanyDetailAsync(selected.SimId, dc.Sector, updateTime);
				if (++counter % 500 == 0)
				{
					Console.WriteLine($"Updated {counter} firms {DateTime.Now}");
					_logger.LogDebug($"Updated {counter} firms");
					await _dbpiScore.Create(newValues);
					newValues.Clear();
				}
			}
			if (newValues.Any())
			{
				Console.WriteLine($"Updated {counter} firms");
				_logger.LogDebug($"Updated {counter} firms");
				await _dbpiScore.Create(newValues);
			}
		}

		#endregion Public Methods

		#region Private Methods

		private static void UpdateAnalysis(PiotroskiScore newValue, int year, int rating, float ebitda)
		{
			newValue.FYear = year;
			newValue.Rating = rating;
			newValue.EBITDA = (long)ebitda;
			newValue.Revenue = null;
		}

		#endregion Private Methods
	}
}