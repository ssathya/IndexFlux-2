using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using MongoReadWrite.BusLogic;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite
{
	internal class Program
	{
		private const int financialDownloadLimit = 60;

		#region Public Properties

		private static IServiceProvider Provider;

		#endregion Public Properties


		#region Public Methods

		public static void Main(string[] args)
		{
			var services = new ServiceCollection();
			SetupDependencies(services);
			AutoMapperConfig.Start();

			var handleCompList = Provider.GetService<HandleCompanyList>();

			var handleFin = Provider.GetService<HandleFinacials>();
			var compDetailsLst = handleCompList.GetAllCompaniesFromDbAsync().Result;
			if (compDetailsLst == null || compDetailsLst.Count < 2357)
			{
				compDetailsLst = handleCompList.GetAllCompaniesAsync().Result;
			}
			Console.WriteLine("Obtained list of companies");

			UpdateDataFromExternalFeed(compDetailsLst);

			var analyzeFin = Provider.GetService<AnalyzeFinancial>();			

			Stopwatch stopWatch = new Stopwatch();
			compDetailsLst.Shuffle();
			foreach (var companyDetail in compDetailsLst.Where(c => c.LastUpdate != null).Take(400))
			{
				var cd = companyDetail;
				stopWatch.Reset();
				stopWatch.Start();
				Console.WriteLine($"\nPrinting financial values for {cd.Name}");
				var compFinLst = analyzeFin.ComputeScoresAsync(cd.SimId).Result;
				stopWatch.Stop();
				Task.WaitAll();
				//DisplayTimeTaken(stopWatch, $"was the time to parse {cd.Name}'s finance");


			}

			Console.WriteLine("Done");
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


			var miniCompDetails = compDetailsLst.OrderByDescending(cd => cd.Name).ToList();

			var listCount = miniCompDetails.Count();
			miniCompDetails.Shuffle();
			Console.WriteLine($"Obtaining for {listCount} companies");
			DownloadFinancialData(miniCompDetails, downloadCount);
		}

		#endregion Private Methods

	}
}