using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using MongoHandler.Extensions;
using MongoReadWrite.BusLogic;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Diagnostics;
using System.Linq;

namespace MongoReadWrite
{
	internal class Program
	{
		#region Private Fields

		private const int financialDownloadLimit = 75;
		private static ILogger<Program> _logger;

		private static IServiceProvider Provider;

		#endregion Private Fields

		#region Public Methods

		public static void Main(string[] args)
		{
			var services = new ServiceCollection();
			ServiceExtensions.AddKeysToEnvironment(services);
			SetupDependencies(services);
#pragma warning disable CS0612 // Type or member is obsolete
			AutoMapperConfig.Start();
#pragma warning restore CS0612 // Type or member is obsolete

			var handleCompList = Provider.GetService<HandleCompanyList>();

			var handleFin = Provider.GetService<HandleFinacials>();
			_logger = Provider.GetService<ILogger<Program>>();
			_logger.LogDebug("Application Started");
			var compDetailsLst = handleCompList.GetAllCompaniesFromDbAsync().Result;
			if (compDetailsLst == null || compDetailsLst.Count < 2357)
			{
				compDetailsLst = handleCompList.GetAllCompaniesAsync().Result;
			}
			Console.WriteLine("Obtained list of companies");

			UpdateDataFromExternalFeed(compDetailsLst);

			try
			{
				var wav = Provider.GetService<WriteAnalyzedValues>();
				var wavResult = wav.UpdateAnalysis();
				wavResult.Wait();
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Error parsing and writing s/s \n{ex.Message}");
			}

			Console.WriteLine("Done");
			_logger.LogDebug("Done");
			//Provider.GetService()
		}

		#endregion Public Methods

		#region Private Methods

		private static void DisplayTimeTaken(Stopwatch stopWatch, string msg)
		{
			var ts = stopWatch.Elapsed;
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
				ts.Hours, ts.Minutes, ts.Seconds,
				ts.Milliseconds / 10);
			Console.WriteLine($"\n{msg} {elapsedTime}");
		}

		private static void DownloadFinancialData(System.Collections.Generic.List<CompanyDetail> miniCompDetails, int downloadStart)
		{
			Stopwatch stopWatch = new Stopwatch();
			int counter = 0;
			int downloadCount = downloadStart;
			var handleFinancials = Provider.GetService<HandleFinacials>();
			var listCount = miniCompDetails.Count();
			foreach (var company in miniCompDetails)
			{
				stopWatch.Reset();
				stopWatch.Start();
				Console.WriteLine($"Working on {++counter} of {listCount}  => {company.Name}; its Ticker is {company.Ticker}");
				var getFinanceStatus = handleFinancials.UpdateStatements(company.SimId).Result;
				var status = getFinanceStatus ? "Success" : "Failed";
				Console.WriteLine($"Financial for {company.Name} obtained {status}");
				stopWatch.Stop();
				var msg = company.Name + " took ";
				DisplayTimeTaken(stopWatch, msg);
				downloadCount += stopWatch.Elapsed.Seconds > 4 ? 1 : 0;
				//var limit = financialDownloadLimit;
				if (downloadCount >= financialDownloadLimit)
				{
					Console.WriteLine($"Obtained data for more than {financialDownloadLimit} companies. Terminating this run.");
					break;
				}
				else
				{
					Console.WriteLine($"Downloaded {downloadCount} against allocated quota of {financialDownloadLimit} for this run");
				}
			}
		}

		private static void SetupDependencies(IServiceCollection services)
		{
			var configurationBuilder = ServiceExtensions.BuildConfigurationBuilder();
			var configuration = configurationBuilder.Build();
			services.AddSingleton<IConfiguration>(configuration);
			ServiceExtensions.RegisterDependencyInjections(services, configuration);

			Provider = services.BuildServiceProvider();
		}

		private static void UpdateDataFromExternalFeed(System.Collections.Generic.List<CompanyDetail> compDetailsLst)
		{
			compDetailsLst = compDetailsLst.Where(cd => cd.Ticker != null).ToList();
			compDetailsLst = compDetailsLst.Where(cd => cd.Ticker != "").ToList();

			var yesterday = DateTime.Now.AddDays(-1);
			var miniCompDetails = compDetailsLst.OrderByDescending(cd => cd.Name).ToList();
			compDetailsLst = compDetailsLst.FindAll(cd => cd.LastUpdate >= yesterday);
			var ss = compDetailsLst.FindAll(cd => cd.LastUpdate >= DateTime.Today);
			ss = compDetailsLst.FindAll(cd => cd.LastUpdate >= DateTime.Now);
			var downloadCount = compDetailsLst.FindAll(cd => cd.LastUpdate >= yesterday).Count;
			if (downloadCount >= financialDownloadLimit)
			{
				Console.WriteLine($"Already exceeded today's download limit of {financialDownloadLimit}; " +
					$"today's count is {downloadCount}" +
					$"\n Not downloading anymore today....");
				return;
			}
			var listCount = miniCompDetails.Count();
			miniCompDetails.Shuffle();
			Console.WriteLine($"Obtaining for {listCount} companies");
			_logger.LogDebug("Starting to download financial data");
			DownloadFinancialData(miniCompDetails, downloadCount);
			_logger.LogDebug("Today's quota of financial data done");
		}

		#endregion Private Methods
	}
}