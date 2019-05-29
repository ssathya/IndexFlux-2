using AutoMapper;
using Microsoft.Extensions.Logging;
using Models;
using MongoDB.Driver;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
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
		private readonly ILogger<AnalyzeFinancial> _logger;
		private readonly IMongoCollection<CompanyFinancialsMd> _statementConnection;

		#endregion Private Fields


		#region Public Constructors

		public WriteAnalyzedValues(IDBConnectionHandler<CompanyFinancialsMd> dbconCompany,
			IDBConnectionHandler<PiotroskiScoreMd> dbpiScore,
			ILogger<AnalyzeFinancial> logger,
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
			foreach (var dc in dcl)
			{
				if (string.IsNullOrWhiteSpace(dc.CompanyName) || string.IsNullOrWhiteSpace(dc.Ticker))
				{
					continue;
				}
				var selected = (from comp in allCompanies
								where (comp.Ticker == dc.Ticker && comp.Name == dc.CompanyName)
								select comp).FirstOrDefault();
				if (selected == null)
				{
					continue;
				}
				
				var ProfitablityRatios = new Dictionary<string, decimal>();
				ProfitablityRatios.Add("Gross Margin", (decimal)dc.GrossMargin);
				ProfitablityRatios.Add("Operating Margin", (decimal)dc.OperatingMargin);
				ProfitablityRatios.Add("Net Profit Margin", (decimal)dc.NetMargin);
				ProfitablityRatios.Add("Return on Equity", (decimal)dc.ReturnOnEquity);
				ProfitablityRatios.Add("Return on Assets", (decimal)dc.ReturnOnAssets);
				var newValue = new PiotroskiScore
				{
					SimId = selected.SimId,
					FYear = DateTime.Now.Year,
					Rating = dc.PiotroskiScoreCurrent,
					EBITDA = (long)dc.EbitdaCurrent,
					LastUpdate = DateTime.Now,
					ProfitablityRatios = ProfitablityRatios
				};
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				newValue.ProfitablityRatios.Clear();
				UpdateAnalysis(newValue, DateTime.Now.Year - 1, dc.PiotroskiScore1YrAgo, dc.Ebitda1YrAgo);
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				UpdateAnalysis(newValue, DateTime.Now.Year - 2, dc.PiotroskiScore2YrAgo, dc.Ebitda2YrAgo);
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				UpdateAnalysis(newValue, DateTime.Now.Year - 3, dc.PiotroskiScore3YrAgo, dc.Ebitda3YrAgo);
				newValues.Add(Mapper.Map<PiotroskiScoreMd>(newValue));
				
				if (++counter % 500 == 0)
				{
					Console.WriteLine($"Updated {counter} firms");
					await _dbpiScore.Create(newValues);
					newValues.Clear();
				}
			}
			if (newValues.Any())
			{
				Console.WriteLine($"Updated {counter} firms");
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
		}

		#endregion Private Methods
	}
}