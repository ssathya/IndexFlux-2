using ExcelReadWrite.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExcelReadWrite
{
	internal class Program
	{
		private static string outFile = @"c:\users\sridh\Downloads\Delme\All.xlsx";		

		private static void Main(string[] args)
		{			
			GenerateExcel();
		}

		private static ILogger CreateLogger<T>()
		{
			var lf = new LoggerFactory();
			var localLogger = LoggerFactoryExtensions.CreateLogger(lf, typeof(Logger<T>));
			return localLogger;
		}

		private static void GenerateExcel()
		{
			var logger = CreateLogger<WriteListOfCompanies>();
			var wlc = new WriteListOfCompanies(logger);
			var results = wlc.WriteAllCompanines(outFile);
			var asdf = wlc.GetCompanyDetails(outFile);
			System.Console.WriteLine(asdf.Count);
			results.Wait();
		}		
	}
}