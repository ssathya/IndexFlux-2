using ExcelReadWrite.Tools;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelReadWrite
{
	internal class Program
	{

		#region Private Fields

		private static List<CompanyDetail> companyDetails;
		private static string outFile = @"c:\users\sridh\Downloads\Delme\All.xlsx";

		#endregion Private Fields


		#region Private Methods

		private static ILogger CreateLogger<T>()
		{
			var lf = new LoggerFactory();
			var localLogger = LoggerFactoryExtensions.CreateLogger(lf, typeof(Logger<T>));
			return localLogger;
		}

		private async static Task GenerateExcel()
		{
			var logger = CreateLogger<WriteListOfCompanies>();
			var wlc = new WriteListOfCompanies(logger);
			await wlc.WriteAllCompanines(outFile);

			companyDetails = wlc.GetCompanyDetails(outFile);
			if (companyDetails == null || companyDetails.Count == 0)
			{
				return;
			}

			logger = CreateLogger<WriteFinancials>();
			var wf = new WriteFinancials(logger);
			int count = 0;
			var banks = new string[] { "C", "BBT", "PNC", "GS", "JPM" };			
			var retailers = new string[] { "CVS", "RAD", "BBY", "BBBY", "AMZN" };
			var insurance = new string[] { "AET", "HUM", "UNH", "MET" };
			var miniCompDetail = companyDetails.Where(cd => banks.Contains(cd.Ticker)).ToList();			
			miniCompDetail.AddRange(companyDetails.Where(cd => retailers.Contains(cd.Ticker)).ToList());
			miniCompDetail.AddRange(companyDetails.Where(cd => insurance.Contains(cd.Ticker)).ToList());
			
			foreach (var companyDetail in miniCompDetail)
			{				
				var simId = companyDetail.SimId;
				Console.WriteLine($"Starting {count}: {simId} at {DateTime.Now}");
				Console.WriteLine($"Starting fetching financial for {companyDetail.Name}");
				var result1 = await wf.UpdateWorkSheetForCompanyAsync(simId, outFile);

				if (result1 == false)
				{
					Console.WriteLine($"WriteFinancials did not work for {companyDetail.Ticker}");
				}
				else
				{
					Console.WriteLine($"WriteFinancials worked for {companyDetail.Ticker}");
				}
				if (++count >= 100)
				{
					break;
				}
			}
			Console.WriteLine("WriteFinancials works");
		}

		private static void Main(string[] args)
		{
			var result = GenerateExcel();
			result.Wait();
		}

		#endregion Private Methods
	}
}