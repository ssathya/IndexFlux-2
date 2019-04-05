using Models;
using MongoDB.Driver;
using MongoReadWrite.Tools;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MongoReadWrite
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			AutoMapperConfig.Start();
			var handleCompList = new HandleCompanyList();
			var handleFin = new HandleFinacials();
			var compDetailsLst = handleCompList.GetAllCompaniesFromDbAsync().Result;
			if (compDetailsLst == null || compDetailsLst.Count < 100)
			{
				compDetailsLst = handleCompList.GetAllCompaniesAsync().Result;
			}
			Console.WriteLine("Obtained list of companies");

			// UpdateDataFromExternalFeed(compDetailsLst);
			var simId = compDetailsLst.FirstOrDefault(cd => cd.Ticker == "GE").SimId;
			var compFinLst = handleFin.ReadFinanceValues(simId);
			Console.WriteLine("Done");
		}



		private static void UpdateDataFromExternalFeed(System.Collections.Generic.List<CompanyDetail> compDetailsLst)
		{
			compDetailsLst = compDetailsLst.Where(cd => cd.Ticker != null).ToList();
			compDetailsLst = compDetailsLst.Where(cd => cd.Ticker != "").ToList();
			var handleFinancials = new HandleFinacials();
			var spylist = new string[] {
 "RF", "VMC", "RSG", "MCHP", "FCX", "PNW", "WMT", "TSN", "JBHT", "RE", "ATVI", "EA", "GOOGL", "GOOG", "FB", "TWTR", "NFLX", "DIS", "GPS", "ROST", "EBAY", "MAT", "CLX", "MNST", "CVX", "OXY", "BEN", "V", "WFC", "SCHW", "FRC", "SIVB", "AMGN", "GILD", "MCK", "A", "EW", "ISRG", "RMD", "VAR", "ALGN", "COO", "ILMN", "NKTR", "JEC", "RHI", "ADBE", "ADSK", "CDNS", "ORCL", "SYMC", "SNPS", "ANET", "CSCO", "JNPR", "PYPL", "KEYS", "INTU", "NTAP", "CRM", "AMAT", "KLAC", "LRCX", "AMD", "AVGO", "INTC", "MXIM", "NVDA", "QCOM", "XLNX", "FTNT", "AAPL", "HPE", "HPQ", "WDC", "AVY", "HCP", "PLD", "ARE", "CBRE", "ESS", "MAC", "O", "DLR", "EQIX", "PSA", "EIX", "SRE", "DISH", "CMG", "TAP", "XEC", "DVA", "WU", "NEM",
"BLL", "AIV", "UDR", "CHTR", "BKNG", "SYF", "HIG", "PBCT", "ALXN", "CI", "UTX", "SWK", "URI", "APH", "IT", "XRX", "DHR", "INCY", "LEN", "CCL", "NCLH", "RCL", "DRI", "RJF", "WCG", "HRS", "ROP", "CSX", "CTXS", "FIS", "REG", "SBAC", "NEE", "HD", "PHM", "NWL", "GPC", "KO", "IVZ", "ICE", "AFL", "STI", "UPS", "DAL", "ROL", "EFX", "GPN", "FLT", "TSS", "SO", "LW", "MU", "LKQ", "MCD", "ULTA", "ADM", "WBA", "CAG", "MDLZ", "NTRS", "DFS", "CBOE", "CME", "AJG", "ALL", "ABT", "BAX", "ABBV", "BA", "DE", "UAL", "FBHS", "CAT", "DOV", "GWW", "ITW", "MSI", "CF", "PKG", "VTR", "EQR", "EXC", "ZBH", "ANTM", "LLY", "CMI", "DRE", "SPG", "NI", "PFG", "MDT", "AGN", "PRGO", "ALLE", "JCI", "ETN", "IR", "ACN", "STX", "APTV",
"YUM", "BF.B", "HUM", "SLB", "CTL", "ALB", "ETR", "IDXX", "DISCA", "DISCK", "UAA", "UA", "MAR", "MKC", "TROW", "LMT", "HST", "FRT", "TRIP", "TJX", "AMG", "STT", "BIIB", "VRTX", "WAT", "ABMD", "HOLX", "PKI", "TMO", "RTN", "GE", "IPGP", "AKAM", "ADI", "SWKS", "BXP", "AMT", "IRM", "ES", "BSX", "BWA", "F", "GM", "WHR", "K", "SYK", "MAS", "DWDP", "CMS", "DTE", "BBY", "TGT", "GIS", "HRL", "AMP", "USB", "UNH", "CHRW", "FAST", "MMM", "MOS", "ECL", "XEL", "LEG", "HRB", "ORLY", "CERN", "CNC", "EMR", "KSU", "JKHY", "EVRG", "AEE", "BRK.B", "UNP", "MYL", "LYB", "MGM", "WYNN", "CHD", "CPB", "PRU", "CELG", "BDX", "DGX", "JNJ", "MRK", "ZTS", "HON", "VRSK", "ADP", "CTSH", "SEE", "PEG", "NRG", "AWK", "IPG", "OMC", "CBS", "VZ", "TTWO", "FOXA", "FOX", "VIAB", "NWSA", "NWS", "FL", "CPRI", "RL", "PVH", "TPR", "TIF", "MHK", "STZ", "CL", "COTY", "EL", "PEP", "PM", "HES", "BLK", "BK", "AXP", "MA", "C", "JPM", "MCO", "MSCI", "NDAQ", "SPGI", "MMC", "ETFC", "GS", "MS", "MET", "AIZ", "L", "JEF", "AIG", "TRV", "MTB", "REGN", "BMY", "HSIC", "PFE", "ARNC", "LLL", "XYL", "NLSN", "BR", "GLW", "PAYX", "IBM", "IFF", "SLG", "VNO", "KIM", "ED", "HBI", "VFC", "LOW", "BAC", "BHF", "BBT", "LH", "IQV", "QRVO", "RHT", "MLM", "NUE", "DUK", "LB", "M",
"KR", "SJM", "PG", "MPC", "CINF", "PGR", "FITB", "HBAN", "KEY", "CAH", "MTD", "TDG", "CTAS", "PH", "SHW", "WELL", "AEP", "FE", "DVN", "OKE", "WMB", "HP", "NKE", "FLIR", "CMCSA", "KHC", "HSY", "LNC", "PNC", "ABC", "TFX", "UHS", "XRAY", "WAB", "AME", "ANSS", "FMC", "APD", "PPG", "PPL", "HAS", "CFG", "CVS", "TXT", "LIN", "GRMN", "CB", "TEL", "DG", "AZO", "TSCO", "UNM", "HCA", "FDX", "EMN", "IP", "MAA", "T", "DHI", "SYY", "KMB", "XOM", "BHGE", "HAL", "NOV", "APC", "APA", "COG", "CXO", "COP", "FANG", "EOG", "MRO", "NBL", "PXD", "HFC", "PSX", "VLO", "KMI", "CMA", "TMK", "AAL", "LUV", "FLR", "PWR", "CPRT", "WM", "FLS", "ADS", "TXN", "CE", "CCI", "ATO", "CNP", "PNR", "FTI", "AON", "WLTW", "INFO", "ZION",
"EXR", "AAP", "DLTR", "HLT", "KMX", "MO", "COF", "GD", "HII", "NOC", "NSC", "VRSN", "DXC", "WRK", "D", "AES", "AVB", "JWN", "AMZN", "EXPE", "SBUX", "COST", "EXPD", "ALK", "PCAR", "FTV", "FFIV", "MSFT", "WY", "KSS", "HOG", "AOS", "ROK", "SNA", "FISV", "LNT", "WEC"
};
			var miniCompDetails = compDetailsLst.Where(cd => spylist.Contains(cd.Ticker)).OrderByDescending(cd => cd.Name).ToList();

			var listCount = miniCompDetails.Count();
			Console.WriteLine($"Obtaining for {listCount} companies");
			Stopwatch stopWatch = new Stopwatch();
			int counter = 0;
			int downloadCount = 0;
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
				var limit = 35;
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