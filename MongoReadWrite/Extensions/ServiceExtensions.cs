using HandleSimFin.Methods;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using MongoReadWrite.BusLogic;
using MongoReadWrite.Utils;
using System.Linq;

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

			services.AddScoped<IDBConnectionHandler<CompanyDetailMd>, DBConnectionHandler<CompanyDetailMd>>();
			services.AddScoped<IDBConnectionHandler<CompanyFinancialsMd>, DBConnectionHandler<CompanyFinancialsMd>>();
			services.AddScoped<IDBConnectionHandler<OutstandingSharesMd>, DBConnectionHandler<OutstandingSharesMd>>();			

			services.AddScoped<AnalyzeFinancial>();
			services.AddScoped<DownloadListedFirms>();
			services.AddScoped<DownloadReportableItems>();
			services.AddScoped<HandleCompanyList>();
			services.AddScoped<HandleFinacials>();
			services.AddScoped<HandleSharesOutStanding>();
			services.AddScoped<IDownloadOutstandingShares, DownloadOutstandingShares>();
			services.AddScoped<ListOfStatements>();

			
		}
		internal static string[] GetListOfSnP500Companines()
		{
			var snPList500 = new string[] {"RF", "VMC", "RSG", "MCHP", "FCX", "PNW", "WMT", "TSN", "JBHT", "RE", "ATVI", "EA", "GOOGL", "GOOG", "FB", "TWTR", "NFLX", "DIS", "GPS", "ROST", "EBAY", "MAT", "CLX", "MNST", "CVX", "OXY", "BEN", "V", "WFC", "SCHW", "FRC", "SIVB", "AMGN", "GILD", "MCK", "A", "EW", "ISRG", "RMD", "VAR", "ALGN", "COO", "ILMN", "NKTR", "JEC", "RHI", "ADBE", "ADSK", "CDNS", "ORCL", "SYMC", "SNPS", "ANET", "CSCO", "JNPR", "PYPL", "KEYS", "INTU", "NTAP", "CRM", "AMAT", "KLAC", "LRCX", "AMD", "AVGO", "INTC", "MXIM", "NVDA", "QCOM", "XLNX", "FTNT", "AAPL", "HPE", "HPQ", "WDC", "AVY", "HCP", "PLD", "ARE", "CBRE", "ESS", "MAC", "O", "DLR", "EQIX", "PSA", "EIX", "SRE", "DISH", "CMG", "TAP", "XEC", "DVA", "WU", "NEM",
				"BLL", "AIV", "UDR", "CHTR", "BKNG", "SYF", "HIG", "PBCT", "ALXN", "CI", "UTX", "SWK", "URI", "APH", "IT", "XRX", "DHR", "INCY", "LEN", "CCL", "NCLH", "RCL", "DRI", "RJF", "WCG", "HRS", "ROP", "CSX", "CTXS", "FIS", "REG", "SBAC", "NEE", "HD", "PHM", "NWL", "GPC", "KO", "IVZ", "ICE", "AFL", "STI", "UPS", "DAL", "ROL", "EFX", "GPN", "FLT", "TSS", "SO", "LW", "MU", "LKQ", "MCD", "ULTA", "ADM", "WBA", "CAG", "MDLZ", "NTRS", "DFS", "CBOE", "CME", "AJG", "ALL", "ABT", "BAX", "ABBV", "BA", "DE", "UAL", "FBHS", "CAT", "DOV", "GWW", "ITW", "MSI", "CF", "PKG", "VTR", "EQR", "EXC", "ZBH", "ANTM", "LLY", "CMI", "DRE", "SPG", "NI", "PFG", "MDT", "AGN", "PRGO", "ALLE", "JCI", "ETN", "IR", "ACN", "STX", "APTV",
				"YUM", "BF.B", "HUM", "SLB", "CTL", "ALB", "ETR", "IDXX", "DISCA", "DISCK", "UAA", "UA", "MAR", "MKC", "TROW", "LMT", "HST", "FRT", "TRIP", "TJX", "AMG", "STT", "BIIB", "VRTX", "WAT", "ABMD", "HOLX", "PKI", "TMO", "RTN", "GE", "IPGP", "AKAM", "ADI", "SWKS", "BXP", "AMT", "IRM", "ES", "BSX", "BWA", "F", "GM", "WHR", "K", "SYK", "MAS", "DWDP", "CMS", "DTE", "BBY", "TGT", "GIS", "HRL", "AMP", "USB", "UNH", "CHRW", "FAST", "MMM", "MOS", "ECL", "XEL", "LEG", "HRB", "ORLY", "CERN", "CNC", "EMR", "KSU", "JKHY", "EVRG", "AEE", "BRK.B", "UNP", "MYL", "LYB", "MGM", "WYNN", "CHD", "CPB", "PRU", "CELG", "BDX", "DGX", "JNJ", "MRK", "ZTS", "HON", "VRSK", "ADP", "CTSH", "SEE", "PEG", "NRG", "AWK", "IPG", "OMC", "CBS", "VZ", "TTWO", "FOXA", "FOX", "VIAB", "NWSA", "NWS", "FL", "CPRI", "RL", "PVH", "TPR", "TIF", "MHK", "STZ", "CL", "COTY", "EL", "PEP", "PM", "HES", "BLK", "BK", "AXP", "MA", "C", "JPM", "MCO", "MSCI", "NDAQ", "SPGI", "MMC", "ETFC", "GS", "MS", "MET", "AIZ", "L", "JEF", "AIG", "TRV", "MTB", "REGN", "BMY", "HSIC", "PFE", "ARNC", "LLL", "XYL", "NLSN", "BR", "GLW", "PAYX", "IBM", "IFF", "SLG", "VNO", "KIM", "ED", "HBI", "VFC", "LOW", "BAC", "BHF", "BBT", "LH", "IQV", "QRVO", "RHT", "MLM", "NUE", "DUK", "LB", "M",
				"KR", "SJM", "PG", "MPC", "CINF", "PGR", "FITB", "HBAN", "KEY", "CAH", "MTD", "TDG", "CTAS", "PH", "SHW", "WELL", "AEP", "FE", "DVN", "OKE", "WMB", "HP", "NKE", "FLIR", "CMCSA", "KHC", "HSY", "LNC", "PNC", "ABC", "TFX", "UHS", "XRAY", "WAB", "AME", "ANSS", "FMC", "APD", "PPG", "PPL", "HAS", "CFG", "CVS", "TXT", "LIN", "GRMN", "CB", "TEL", "DG", "AZO", "TSCO", "UNM", "HCA", "FDX", "EMN", "IP", "MAA", "T", "DHI", "SYY", "KMB", "XOM", "BHGE", "HAL", "NOV", "APC", "APA", "COG", "CXO", "COP", "FANG", "EOG", "MRO", "NBL", "PXD", "HFC", "PSX", "VLO", "KMI", "CMA", "TMK", "AAL", "LUV", "FLR", "PWR", "CPRT", "WM", "FLS", "ADS", "TXN", "CE", "CCI", "ATO", "CNP", "PNR", "FTI", "AON", "WLTW", "INFO", "ZION",
				"EXR", "AAP", "DLTR", "HLT", "KMX", "MO", "COF", "GD", "HII", "NOC", "NSC", "VRSN", "DXC", "WRK", "D", "AES", "AVB", "JWN", "AMZN", "EXPE", "SBUX", "COST", "EXPD", "ALK", "PCAR", "FTV", "FFIV", "MSFT", "WY", "KSS", "HOG", "AOS", "ROK", "SNA", "FISV", "LNT", "WEC"};
			var snPList400 = new string[] { "AAN", "ACHC", "ACIW", "ADNT", "ATGE", "ACM", "ACC", "AEO", "AFG", "AGCO", "AHL", "AKRX", "ALE", "ALEX", "APY", "ATI", "AMCX", "AN", "ARW", "ARRS", "ASB", "ASGN", "ASH", "ATO", "ATR", "AVNS", "AVT", "AYI", "BBBY", "BC", "BCO", "BDC", "BID", "BIG", "BIO", "BKH", "BL", "KB", "BMS", "BOH", "BRO", "BXS", "BYD", "CABO", "CAKE", "CAR", "CARS", "CASY", "CATY", "CBSH", "CBT", "CC", "CDK", "CFR", "CGNX", "CHE", "CHDN", "CHFC", "CHK", "CIEN", "CLB", "CLGX", "CLH", "CLI", "CMC", "CMD", "CMP", "CNK", "CNO", "COHR", "CONE", "COR", "CPE", "CPT", "CR", "CREE", "CRI", "CRL", "CRS", "CRUS", "CNX", "CSL", "CTLT", "CUZ", "CVLT", "CXW", "CW", "CBRL", "CY", "DAN", "DCI", "DDS", "DECK", "DEI", "DKS", "DLPH", "DLX", "DNB", "DNKN", "DNOW", "DO", "DPZ", "DRQ", "DY", "EAT", "EGN", "EHC", "EME", "ENR", "ENS", "EPC", "EPR", "ERI", "ESL", "ESV", "EV", "EVR", "EWBC", "EXEL", "EXP", "FAF", "FDS", "FHN", "FICO", "FII", "FIVE", "FLO", "FR", "FNB", "FSLR", "FULT", "GATX", "GEF", "GEO", "GGG", "GHC", "GME", "GMED", "GNTX", "GNW", "GPOR", "GVA", "GWR", "HAE", "HAIN", "HWC", "HCSG", "HE", "HELE", "HIW", "HNI", "HOMB", "HPT", "HQY", "HR", "HRC", "HUBB", "ICUI", "IDA", "IART", "IBKR", "IBOC", "IDCC", "IDTI", "IEX", "INGR", "INT", "ISCA", "ITT", "JACK", "JBGS", "JBL", "JHG", "JBLU", "JCOM", "JKHY", "JLL", "JW.A", "KBH", "KBR", "KEX", "KMPR", "KMT", "KNX", "KRC", "LAMR", "LANC", "LHO", "LDOS", "LGND", "LECO", "LFUS", "LII", "LITE", "LIVN", "RAMP", "LM", "LOGM", "LPNT", "LPT", "LPX", "LSI", "LSTR", "LW", "LYV", "MAN", "MANH", "MASI", "MBFI", "MCY", "MD", "MDP", "MDR", "MDRX", "MDSO", "MDU", "MIK", "MKSI", "MKTX", "MLHR", "MMS", "MNK", "MOH", "MPW", "MPWR", "MSA", "MSM", "MTDR", "MTX", "MTZ", "MUR", "MUSA", "NATI", "NAVI", "NBR", "NCR", "NDSN", "NEU", "NFG", "NJR", "NNN", "NTCT", "NUS", "NUVA", "NVR", "NVT", "NWE", "NYCB", "NYT", "OAS", "ODFL", "ODP", "OFC", "OGE", "OGS", "OHI", "OII", "OLLI", "OLN", "OI", "ORI", "OSK", "OZK", "PACW", "PBF", "PB", "PBH", "PBI", "PCH", "PDCO", "PENN", "PII", "PLT", "PNFP", "PNM", "POL", "POOL", "POST", "PRAH", "PRI", "PTC", "PTEN", "PZZA", "QEP", "R", "RBC", "RDC", "RGA", "RGLD", "RIG", "RLGY", "RNR", "ROL", "RPM", "RRC", "RS", "RYN", "SABR", "SAFM", "SAIC", "SAM", "SBH", "SBNY", "SBRA", "SCI", "SEIC", "SF", "SFM", "SGMS", "SIG", "SIX", "SKT", "SKX", "SLAB", "SLGN", "SLM", "SM", "SMG", "SNH", "SNV", "SNX", "SON", "SPN", "STE", "STL", "STLD", "SWN", "SWX", "SXT", "SYNA", "SYNH", "TCF", "TCBI", "TCO", "TDC", "TDS", "TDY", "TECD", "TECH", "TER", "TEX", "TFX", "TGNA", "THC", "THG", "THO", "THS", "TKR", "TOL", "TPH", "TPX", "TR", "TREE", "TRMB", "TRMK", "TRN", "TTC", "TUP", "TXRH", "TYL", "UBSI", "UE", "UFS", "UGI", "ULTI", "UMBF", "UMPQ", "UNFI", "UTHR", "UN", "IT", "URBN", "VAC", "VC", "VLY", "VMI", "VVV", "VVC", "VSM", "VSAT", "VSH", "WRB", "WAB", "WAFD", "WBS", "WEN", "WERN", "WEX", "WOR", "WPX", "WRI", "WSM", "WSO", "WST", "WTFC", "WTR", "WTW", "WWD", "WWE", "WYND", "X", "Y", "ZBRA" };
			var combinedList = snPList500.Concat(snPList400).Distinct().ToArray();

			return combinedList;
		}
	}
}