using HandleSimFin.Methods;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using MongoReadWrite.BusLogic;
using MongoReadWrite.Utils;

namespace MongoReadWrite.Extensions
{
	public static class ServiceExtensions
	{
		internal static IConfigurationBuilder BuildConfigurationBuilder()
		{
			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();
			return configurationBuilder;
		}
		internal static void RegisterDependencyInjections(this IServiceCollection services)
		{
			var loggerFactory = new LoggerFactory();

			services.AddSingleton<ILoggerFactory>(loggerFactory);
			services.AddLogging();

			services.AddScoped<HandleCompanyList, HandleCompanyList>();
			services.AddScoped<DownloadListedFirms, DownloadListedFirms>();
			services.AddScoped<HandleFinacials, HandleFinacials>();
			services.AddScoped<ListOfStatements, ListOfStatements>();
			services.AddScoped<DownloadReportableItems, DownloadReportableItems>();

			services.AddSingleton<IDBConnectionHandler<CompanyDetailMd>, DBConnectionHandler<CompanyDetailMd>>();
			services.AddSingleton<IDBConnectionHandler<CompanyFinancialsMd>, DBConnectionHandler<CompanyFinancialsMd>>();
		}
		internal static string[] GetListOfSnP500Companines()
		{
			var snPList = new string[] {"RF", "VMC", "RSG", "MCHP", "FCX", "PNW", "WMT", "TSN", "JBHT", "RE", "ATVI", "EA", "GOOGL", "GOOG", "FB", "TWTR", "NFLX", "DIS", "GPS", "ROST", "EBAY", "MAT", "CLX", "MNST", "CVX", "OXY", "BEN", "V", "WFC", "SCHW", "FRC", "SIVB", "AMGN", "GILD", "MCK", "A", "EW", "ISRG", "RMD", "VAR", "ALGN", "COO", "ILMN", "NKTR", "JEC", "RHI", "ADBE", "ADSK", "CDNS", "ORCL", "SYMC", "SNPS", "ANET", "CSCO", "JNPR", "PYPL", "KEYS", "INTU", "NTAP", "CRM", "AMAT", "KLAC", "LRCX", "AMD", "AVGO", "INTC", "MXIM", "NVDA", "QCOM", "XLNX", "FTNT", "AAPL", "HPE", "HPQ", "WDC", "AVY", "HCP", "PLD", "ARE", "CBRE", "ESS", "MAC", "O", "DLR", "EQIX", "PSA", "EIX", "SRE", "DISH", "CMG", "TAP", "XEC", "DVA", "WU", "NEM",
				"BLL", "AIV", "UDR", "CHTR", "BKNG", "SYF", "HIG", "PBCT", "ALXN", "CI", "UTX", "SWK", "URI", "APH", "IT", "XRX", "DHR", "INCY", "LEN", "CCL", "NCLH", "RCL", "DRI", "RJF", "WCG", "HRS", "ROP", "CSX", "CTXS", "FIS", "REG", "SBAC", "NEE", "HD", "PHM", "NWL", "GPC", "KO", "IVZ", "ICE", "AFL", "STI", "UPS", "DAL", "ROL", "EFX", "GPN", "FLT", "TSS", "SO", "LW", "MU", "LKQ", "MCD", "ULTA", "ADM", "WBA", "CAG", "MDLZ", "NTRS", "DFS", "CBOE", "CME", "AJG", "ALL", "ABT", "BAX", "ABBV", "BA", "DE", "UAL", "FBHS", "CAT", "DOV", "GWW", "ITW", "MSI", "CF", "PKG", "VTR", "EQR", "EXC", "ZBH", "ANTM", "LLY", "CMI", "DRE", "SPG", "NI", "PFG", "MDT", "AGN", "PRGO", "ALLE", "JCI", "ETN", "IR", "ACN", "STX", "APTV",
				"YUM", "BF.B", "HUM", "SLB", "CTL", "ALB", "ETR", "IDXX", "DISCA", "DISCK", "UAA", "UA", "MAR", "MKC", "TROW", "LMT", "HST", "FRT", "TRIP", "TJX", "AMG", "STT", "BIIB", "VRTX", "WAT", "ABMD", "HOLX", "PKI", "TMO", "RTN", "GE", "IPGP", "AKAM", "ADI", "SWKS", "BXP", "AMT", "IRM", "ES", "BSX", "BWA", "F", "GM", "WHR", "K", "SYK", "MAS", "DWDP", "CMS", "DTE", "BBY", "TGT", "GIS", "HRL", "AMP", "USB", "UNH", "CHRW", "FAST", "MMM", "MOS", "ECL", "XEL", "LEG", "HRB", "ORLY", "CERN", "CNC", "EMR", "KSU", "JKHY", "EVRG", "AEE", "BRK.B", "UNP", "MYL", "LYB", "MGM", "WYNN", "CHD", "CPB", "PRU", "CELG", "BDX", "DGX", "JNJ", "MRK", "ZTS", "HON", "VRSK", "ADP", "CTSH", "SEE", "PEG", "NRG", "AWK", "IPG", "OMC", "CBS", "VZ", "TTWO", "FOXA", "FOX", "VIAB", "NWSA", "NWS", "FL", "CPRI", "RL", "PVH", "TPR", "TIF", "MHK", "STZ", "CL", "COTY", "EL", "PEP", "PM", "HES", "BLK", "BK", "AXP", "MA", "C", "JPM", "MCO", "MSCI", "NDAQ", "SPGI", "MMC", "ETFC", "GS", "MS", "MET", "AIZ", "L", "JEF", "AIG", "TRV", "MTB", "REGN", "BMY", "HSIC", "PFE", "ARNC", "LLL", "XYL", "NLSN", "BR", "GLW", "PAYX", "IBM", "IFF", "SLG", "VNO", "KIM", "ED", "HBI", "VFC", "LOW", "BAC", "BHF", "BBT", "LH", "IQV", "QRVO", "RHT", "MLM", "NUE", "DUK", "LB", "M",
				"KR", "SJM", "PG", "MPC", "CINF", "PGR", "FITB", "HBAN", "KEY", "CAH", "MTD", "TDG", "CTAS", "PH", "SHW", "WELL", "AEP", "FE", "DVN", "OKE", "WMB", "HP", "NKE", "FLIR", "CMCSA", "KHC", "HSY", "LNC", "PNC", "ABC", "TFX", "UHS", "XRAY", "WAB", "AME", "ANSS", "FMC", "APD", "PPG", "PPL", "HAS", "CFG", "CVS", "TXT", "LIN", "GRMN", "CB", "TEL", "DG", "AZO", "TSCO", "UNM", "HCA", "FDX", "EMN", "IP", "MAA", "T", "DHI", "SYY", "KMB", "XOM", "BHGE", "HAL", "NOV", "APC", "APA", "COG", "CXO", "COP", "FANG", "EOG", "MRO", "NBL", "PXD", "HFC", "PSX", "VLO", "KMI", "CMA", "TMK", "AAL", "LUV", "FLR", "PWR", "CPRT", "WM", "FLS", "ADS", "TXN", "CE", "CCI", "ATO", "CNP", "PNR", "FTI", "AON", "WLTW", "INFO", "ZION",
				"EXR", "AAP", "DLTR", "HLT", "KMX", "MO", "COF", "GD", "HII", "NOC", "NSC", "VRSN", "DXC", "WRK", "D", "AES", "AVB", "JWN", "AMZN", "EXPE", "SBUX", "COST", "EXPD", "ALK", "PCAR", "FTV", "FFIV", "MSFT", "WY", "KSS", "HOG", "AOS", "ROK", "SNA", "FISV", "LNT", "WEC"};
			return snPList;
		}
	}
}