﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;
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
			var selectedFirms = new string[] { "MSFT", "GE", "BBY", "KO", "CAT", "DOV", "CW" };
			Stopwatch stopWatch = new Stopwatch();
			foreach (var selectedFirm in selectedFirms)
			{
				var cd = compDetailsLst.FirstOrDefault(c => c.Ticker == selectedFirm);				
				if (cd != null)
				{					
					stopWatch.Reset();
					stopWatch.Start();
					Console.WriteLine($"Printing financial values for {cd.Name}");					
					var compFinLst = analyzeFin.ReadFinanceValues(cd.SimId);
					stopWatch.Stop();
					DisplayTimeTaken(stopWatch, $"was the time to parse {cd.Name}'s finance");
				}
				else
				{
					Console.WriteLine($"Did not find a record for {selectedFirm}");
				}
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

		private static void DownloadFinancialData(System.Collections.Generic.List<CompanyDetail> miniCompDetails)
		{
			Stopwatch stopWatch = new Stopwatch();
			int counter = 0;
			int downloadCount = 0;
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
				var limit = 53;
				if (downloadCount >= limit)
				{
					Console.WriteLine($"Obtained data for more than {limit} companies. Terminating this run.");
					break;
				}
				else
				{
					Console.WriteLine($"Downloaded {downloadCount} against allocated quota of {limit} for this run");
				}
			}
		}
		private static void SetupDependencies(IServiceCollection services)
		{
			var configurationBuilder = ServiceExtensions.BuildConfigurationBuilder();

			var configuration = configurationBuilder.Build();
			services.AddSingleton<IConfiguration>(configuration);

			ServiceExtensions.RegisterDependencyInjections(services);

			Provider = services.BuildServiceProvider();
		}

		private static void UpdateDataFromExternalFeed(System.Collections.Generic.List<CompanyDetail> compDetailsLst)
		{
			compDetailsLst = compDetailsLst.Where(cd => cd.Ticker != null).ToList();
			compDetailsLst = compDetailsLst.Where(cd => cd.Ticker != "").ToList();

			var miniCompDetails = compDetailsLst.Where(cd => (ServiceExtensions.GetListOfSnP500Companines())
				.Contains(cd.Ticker))
				.OrderByDescending(cd => cd.Name)
				.ToList();

			var listCount = miniCompDetails.Count();
			Console.WriteLine($"Obtaining for {listCount} companies");
			DownloadFinancialData(miniCompDetails);
		}

		#endregion Private Methods

	}
}