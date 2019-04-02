using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoReadWrite.Tools;
using MongoReadWrite.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite
{
	class Program
	{
		static void Main(string[] args)
		{
			
			var a = new HandleCompanyList();

			var b = a.GetAllCompaniesFromDb();
			if (b == null || b.Count < 100)
			{
				b = a.GetAllCompaniesAsync().Result;
			}				
			Console.WriteLine("Obtained list of companies");
			
			b = b.Where(cd => cd.Ticker != null).ToList();
			b = b.Where(cd => cd.Ticker != "").ToList();
			var c = new HandleFinacials();
			var spylist = new string[] {
					"VMC", "RF", "RSG", "MCHP", "FCX", "PNW", "JBHT", "WMT", "TSN", "RE", "FRC", "MXIM", "KEYS", "FTNT", "ANET", "TWTR", "NKTR", "SIVB", "CDNS", "RMD", "ALGN", "AMD", "ARE", "SNPS", "COO", "DLR", "ILMN", "HPE", "ATVI", "PYPL", "O", "EQIX", "AVGO", "GOOGL", "ESS", "FB", "MAC", "LRCX", "MNST", "EW", "NFLX", "ROST", "V", "WDC", "CRM", "ISRG", "HCP", "JEC", "VAR", "CBRE", "GOOG", "PSA", "GILD", "PLD", "SYMC", "EA", "NVDA", "INTU", "RHI", "A", "XLNX", "NTAP", "SCHW", "ADBE", "AMAT", "CSCO", "AMGN", "ADSK", "ORCL", "AVY", "GPS", "OXY", "AAPL", "MAT", "INTC", "DIS", "WFC", "HPQ", "CLX", "CVX", "EBAY", "EIX", "BEN", "JNPR", "KLAC", "MCK", "QCOM", "SRE", "PSA", "EIX", "SRE", "DISH", "CMG", "TAP", "XEC", "DVA", "WU", "NEM", "BLL", "AIV", "UDR", "CHTR", "BKNG", "SYF", "HIG", "PBCT", "ALXN", "CI", "UTX", "SWK", "URI", "APH", "IT", "XRX", "DHR", "INCY", "LEN", "CCL", "NCLH", "RCL", "DRI", "RJF", "WCG", "HRS", "ROP", "CSX", "CTXS", "FIS", "REG", "SBAC", "NEE" };
			var miniCompDetails = b.Where(cd => spylist.Contains(cd.Ticker)).OrderBy(cd => cd.Ticker).ToList();
			Stopwatch stopWatch = new Stopwatch();
			foreach (var company in miniCompDetails)
			{
				stopWatch.Reset();
				stopWatch.Start();
				Console.WriteLine($"Working on {company.Name}; its Ticker is {company.Ticker}");
				var d = c.UpdateStatements(company.SimId).Result;
				var status = d ? "Success" : "Failed";
				Console.WriteLine($"Financial for {company.Name} obtained {status}" );
				stopWatch.Stop();
				var msg = company.Name + " took ";
				DisplayTimeTaken(stopWatch, msg);
			}
			Console.WriteLine("Done");
			
		}

		private static void DisplayTimeTaken(Stopwatch stopWatch, string msg)
		{
			var ts = stopWatch.Elapsed;
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
				ts.Hours, ts.Minutes, ts.Seconds,
				ts.Milliseconds / 10);
			Console.WriteLine($"\n{msg} {elapsedTime}");
		}

		
	}
}
