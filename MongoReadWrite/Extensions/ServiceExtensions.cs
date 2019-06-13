using Amazon;
using HandleSimFin.Helpers;
using HandleSimFin.Methods;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using MongoHandler.Utils;
using MongoReadWrite.BusLogic;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using SpreadSheetReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoReadWrite.Extensions
{
	public static class ServiceExtensions
	{
		public static  string BucketName = @"talk2control-1";
		public static RegionEndpoint Region = RegionEndpoint.USEast1;
		#region Internal Methods

		internal static void AddKeysToEnvironment(this IServiceCollection services)
		{
			var readS3Objs = new ReadS3Objects(BucketName, Region);

			var keysToServices = JsonConvert
				.DeserializeObject<List<EntityKeys>>(readS3Objs
					.GetDataFromS3("Random.txt")
				.Result);
			foreach (var entityKeys in keysToServices)
			{
				if (!string.IsNullOrEmpty(entityKeys.Entity)
					&& !string.IsNullOrEmpty(entityKeys.Key))
					Environment.SetEnvironmentVariable(entityKeys.Entity, entityKeys.Key);
			}
		}

		internal static IConfigurationBuilder BuildConfigurationBuilder()
		{
			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddEnvironmentVariables();
			return configurationBuilder;
		}

		internal static string[] GetListOfSnP500Companines()
		{
			var snPList500 = new string[] {"RF", "VMC", "RSG", "MCHP", "FCX", "PNW", "WMT", "TSN", "JBHT", "RE", "ATVI", "EA", "GOOGL", "GOOG", "FB", "TWTR", "NFLX", "DIS", "GPS", "ROST", "EBAY", "MAT", "CLX", "MNST", "CVX", "OXY", "BEN", "V", "WFC", "SCHW", "FRC", "SIVB", "AMGN", "GILD", "MCK", "A", "EW", "ISRG", "RMD", "VAR", "ALGN", "COO", "ILMN", "NKTR", "JEC", "RHI", "ADBE", "ADSK", "CDNS", "ORCL", "SYMC", "SNPS", "ANET", "CSCO", "JNPR", "PYPL", "KEYS", "INTU", "NTAP", "CRM", "AMAT", "KLAC", "LRCX", "AMD", "AVGO", "INTC", "MXIM", "NVDA", "QCOM", "XLNX", "FTNT", "AAPL", "HPE", "HPQ", "WDC", "AVY", "HCP", "PLD", "ARE", "CBRE", "ESS", "MAC", "O", "DLR", "EQIX", "PSA", "EIX", "SRE", "DISH", "CMG", "TAP", "XEC", "DVA", "WU", "NEM",
				"BLL", "AIV", "UDR", "CHTR", "BKNG", "SYF", "HIG", "PBCT", "ALXN", "CI", "UTX", "SWK", "URI", "APH", "IT", "XRX", "DHR", "INCY", "LEN", "CCL", "NCLH", "RCL", "DRI", "RJF", "WCG", "HRS", "ROP", "CSX", "CTXS", "FIS", "REG", "SBAC", "NEE", "HD", "PHM", "NWL", "GPC", "KO", "IVZ", "ICE", "AFL", "STI", "UPS", "DAL", "ROL", "EFX", "GPN", "FLT", "TSS", "SO", "LW", "MU", "LKQ", "MCD", "ULTA", "ADM", "WBA", "CAG", "MDLZ", "NTRS", "DFS", "CBOE", "CME", "AJG", "ALL", "ABT", "BAX", "ABBV", "BA", "DE", "UAL", "FBHS", "CAT", "DOV", "GWW", "ITW", "MSI", "CF", "PKG", "VTR", "EQR", "EXC", "ZBH", "ANTM", "LLY", "CMI", "DRE", "SPG", "NI", "PFG", "MDT", "AGN", "PRGO", "ALLE", "JCI", "ETN", "IR", "ACN", "STX", "APTV",
				"YUM", "BF.B", "HUM", "SLB", "CTL", "ALB", "ETR", "IDXX", "DISCA", "DISCK", "UAA", "UA", "MAR", "MKC", "TROW", "LMT", "HST", "FRT", "TRIP", "TJX", "AMG", "STT", "BIIB", "VRTX", "WAT", "ABMD", "HOLX", "PKI", "TMO", "RTN", "GE", "IPGP", "AKAM", "ADI", "SWKS", "BXP", "AMT", "IRM", "ES", "BSX", "BWA", "F", "GM", "WHR", "K", "SYK", "MAS", "DWDP", "CMS", "DTE", "BBY", "TGT", "GIS", "HRL", "AMP", "USB", "UNH", "CHRW", "FAST", "MMM", "MOS", "ECL", "XEL", "LEG", "HRB", "ORLY", "CERN", "CNC", "EMR", "KSU", "JKHY", "EVRG", "AEE", "BRK.B", "UNP", "MYL", "LYB", "MGM", "WYNN", "CHD", "CPB", "PRU", "CELG", "BDX", "DGX", "JNJ", "MRK", "ZTS", "HON", "VRSK", "ADP", "CTSH", "SEE", "PEG", "NRG", "AWK", "IPG", "OMC", "CBS", "VZ", "TTWO", "FOXA", "FOX", "VIAB", "NWSA", "NWS", "FL", "CPRI", "RL", "PVH", "TPR", "TIF", "MHK", "STZ", "CL", "COTY", "EL", "PEP", "PM", "HES", "BLK", "BK", "AXP", "MA", "C", "JPM", "MCO", "MSCI", "NDAQ", "SPGI", "MMC", "ETFC", "GS", "MS", "MET", "AIZ", "L", "JEF", "AIG", "TRV", "MTB", "REGN", "BMY", "HSIC", "PFE", "ARNC", "LLL", "XYL", "NLSN", "BR", "GLW", "PAYX", "IBM", "IFF", "SLG", "VNO", "KIM", "ED", "HBI", "VFC", "LOW", "BAC", "BHF", "BBT", "LH", "IQV", "QRVO", "RHT", "MLM", "NUE", "DUK", "LB", "M",
				"KR", "SJM", "PG", "MPC", "CINF", "PGR", "FITB", "HBAN", "KEY", "CAH", "MTD", "TDG", "CTAS", "PH", "SHW", "WELL", "AEP", "FE", "DVN", "OKE", "WMB", "HP", "NKE", "FLIR", "CMCSA", "KHC", "HSY", "LNC", "PNC", "ABC", "TFX", "UHS", "XRAY", "WAB", "AME", "ANSS", "FMC", "APD", "PPG", "PPL", "HAS", "CFG", "CVS", "TXT", "LIN", "GRMN", "CB", "TEL", "DG", "AZO", "TSCO", "UNM", "HCA", "FDX", "EMN", "IP", "MAA", "T", "DHI", "SYY", "KMB", "XOM", "BHGE", "HAL", "NOV", "APC", "APA", "COG", "CXO", "COP", "FANG", "EOG", "MRO", "NBL", "PXD", "HFC", "PSX", "VLO", "KMI", "CMA", "TMK", "AAL", "LUV", "FLR", "PWR", "CPRT", "WM", "FLS", "ADS", "TXN", "CE", "CCI", "ATO", "CNP", "PNR", "FTI", "AON", "WLTW", "INFO", "ZION",
				"EXR", "AAP", "DLTR", "HLT", "KMX", "MO", "COF", "GD", "HII", "NOC", "NSC", "VRSN", "DXC", "WRK", "D", "AES", "AVB", "JWN", "AMZN", "EXPE", "SBUX", "COST", "EXPD", "ALK", "PCAR", "FTV", "FFIV", "MSFT", "WY", "KSS", "HOG", "AOS", "ROK", "SNA", "FISV", "LNT", "WEC"};
			var snPList400 = new string[] { "AAN", "ACHC", "ACIW", "ADNT", "ATGE", "ACM", "ACC", "AEO", "AFG", "AGCO", "AHL", "AKRX", "ALE", "ALEX", "APY", "ATI", "AMCX", "AN", "ARW", "ARRS", "ASB", "ASGN", "ASH", "ATO", "ATR", "AVNS", "AVT", "AYI", "BBBY", "BC", "BCO", "BDC", "BID", "BIG", "BIO", "BKH", "BL", "KB", "BMS", "BOH", "BRO", "BXS", "BYD", "CABO", "CAKE", "CAR", "CARS", "CASY", "CATY", "CBSH", "CBT", "CC", "CDK", "CFR", "CGNX", "CHE", "CHDN", "CHFC", "CHK", "CIEN", "CLB", "CLGX", "CLH", "CLI", "CMC", "CMD", "CMP", "CNK", "CNO", "COHR", "CONE", "COR", "CPE", "CPT", "CR", "CREE", "CRI", "CRL", "CRS", "CRUS", "CNX", "CSL", "CTLT", "CUZ", "CVLT", "CXW", "CW", "CBRL", "CY", "DAN", "DCI", "DDS", "DECK", "DEI", "DKS", "DLPH", "DLX", "DNB", "DNKN", "DNOW", "DO", "DPZ", "DRQ", "DY", "EAT", "EGN", "EHC", "EME", "ENR", "ENS", "EPC", "EPR", "ERI", "ESL", "ESV", "EV", "EVR", "EWBC", "EXEL", "EXP", "FAF", "FDS", "FHN", "FICO", "FII", "FIVE", "FLO", "FR", "FNB", "FSLR", "FULT", "GATX", "GEF", "GEO", "GGG", "GHC", "GME", "GMED", "GNTX", "GNW", "GPOR", "GVA", "GWR", "HAE", "HAIN", "HWC", "HCSG", "HE", "HELE", "HIW", "HNI", "HOMB", "HPT", "HQY", "HR", "HRC", "HUBB", "ICUI", "IDA", "IART", "IBKR", "IBOC", "IDCC", "IDTI", "IEX", "INGR", "INT", "ISCA", "ITT", "JACK", "JBGS", "JBL", "JHG", "JBLU", "JCOM", "JKHY", "JLL", "JW.A", "KBH", "KBR", "KEX", "KMPR", "KMT", "KNX", "KRC", "LAMR", "LANC", "LHO", "LDOS", "LGND", "LECO", "LFUS", "LII", "LITE", "LIVN", "RAMP", "LM", "LOGM", "LPNT", "LPT", "LPX", "LSI", "LSTR", "LW", "LYV", "MAN", "MANH", "MASI", "MBFI", "MCY", "MD", "MDP", "MDR", "MDRX", "MDSO", "MDU", "MIK", "MKSI", "MKTX", "MLHR", "MMS", "MNK", "MOH", "MPW", "MPWR", "MSA", "MSM", "MTDR", "MTX", "MTZ", "MUR", "MUSA", "NATI", "NAVI", "NBR", "NCR", "NDSN", "NEU", "NFG", "NJR", "NNN", "NTCT", "NUS", "NUVA", "NVR", "NVT", "NWE", "NYCB", "NYT", "OAS", "ODFL", "ODP", "OFC", "OGE", "OGS", "OHI", "OII", "OLLI", "OLN", "OI", "ORI", "OSK", "OZK", "PACW", "PBF", "PB", "PBH", "PBI", "PCH", "PDCO", "PENN", "PII", "PLT", "PNFP", "PNM", "POL", "POOL", "POST", "PRAH", "PRI", "PTC", "PTEN", "PZZA", "QEP", "R", "RBC", "RDC", "RGA", "RGLD", "RIG", "RLGY", "RNR", "ROL", "RPM", "RRC", "RS", "RYN", "SABR", "SAFM", "SAIC", "SAM", "SBH", "SBNY", "SBRA", "SCI", "SEIC", "SF", "SFM", "SGMS", "SIG", "SIX", "SKT", "SKX", "SLAB", "SLGN", "SLM", "SM", "SMG", "SNH", "SNV", "SNX", "SON", "SPN", "STE", "STL", "STLD", "SWN", "SWX", "SXT", "SYNA", "SYNH", "TCF", "TCBI", "TCO", "TDC", "TDS", "TDY", "TECD", "TECH", "TER", "TEX", "TFX", "TGNA", "THC", "THG", "THO", "THS", "TKR", "TOL", "TPH", "TPX", "TR", "TREE", "TRMB", "TRMK", "TRN", "TTC", "TUP", "TXRH", "TYL", "UBSI", "UE", "UFS", "UGI", "ULTI", "UMBF", "UMPQ", "UNFI", "UTHR", "UN", "IT", "URBN", "VAC", "VC", "VLY", "VMI", "VVV", "VVC", "VSM", "VSAT", "VSH", "WRB", "WAB", "WAFD", "WBS", "WEN", "WERN", "WEX", "WOR", "WPX", "WRI", "WSM", "WSO", "WST", "WTFC", "WTR", "WTW", "WWD", "WWE", "WYND", "X", "Y", "ZBRA" };
			var large1000Companies = new string[] { "AAPL", "MSFT", "AMZN", "GOOG", "GOOG", "FB", "JNJ", "JPM", "V", "XOM", "WMT", "BAC", "PG", "INTC", "CSCO", "MA", "VZ", "T", "DIS", "CVX", "PFE", "WFC", "BA", "UNH", "KO", "MRK", "CMCSA", "ORCL", "PEP", "C", "NFLX", "MCD", "ABT", "PM", "ADBE", "IBM", "PYPL", "AVGO", "AVGO", "LLY", "MMM", "CRM", "ACN", "UNP", "ABBV", "HON", "UTX", "AMGN", "MDT", "MDT", "NVDA", "NKE", "TXN", "TMO", "Cost", "MO", "BGNE", "SBUX", "AXP", "low", "DHR", "NEE", "LMT", "DWDP", "AMT", "BKNG", "GILD", "CAT", "CHTR", "USB", "GE", "UPS", "MS", "VMW", "BMY", "COP", "GS", "MDLZ", "BLK", "SYK", "ADP", "QCOM", "CVS", "INTU", "CELG", "TJX", "SLB", "BDX", "ISRG", "DUK", "DUK", "CB", "EPD", "ANTM", "TMUS", "CME", "CSX", "EL", "SCHW", "PNC", "D", "EOG", "CL", "CI", "GM", "SPG", "SO", "SPGI", "ECL", "DE", "CCI", "RTN", "NSC", "FDX", "ITW", "BK", "BSX", "GD", "WBA", "ILMN", "AGN", "AGN", "MMC", "OXY", "NOC", "ZTS", "EXC", "EXC", "TSLA", "MU", "VRTX", "PLD", "MAR", "DELL", "BIIB", "KMI", "ICE", "PGR", "EMR", "WM", "MET", "WDAY", "NOW", "PSX", "DOW", "APD", "BX", "AON", "PRU", "TGT", "KMB", "ADI", "AMAT", "SHW", "CTSH", "COF", "AEP", "QSR", "AIG", "MPC", "KHC", "BAX", "REGN", "EW", "CP", "HCA", "ADSK", "PSA", "DAL", "EQIX", "BBT", "CCL", "F", "AFL", "VFC", "VLO", "TRV", "ROST", "ROP", "STZ", "FIS", "SYY", "MCO", "ETN", "SRE", "WMB", "ATVI", "XLNX", "LYB", "JCI", "EBAY", "FISV", "ALL", "RHT", "ORLY", "DG", "HUM", "APC", "HPQ", "SQ", "YUM", "APH", "ALXN", "GIS", "AMD", "AMTD", "TEL", "LRCX", "PEG", "MNST", "FTV", "PAYX", "OKE", "PXD", "LUV", "XEL", "EA", "EQR", "STI", "PPG", "HAL", "AVB", "IR", "GLW", "ED", "IQV", "STT", "BHGE", "AZO", "TWTR", "CMI", "RCL", "ZBH", "HLT", "SIRI", "RSG", "DLTR", "TROW", "DFS", "ANET", "S", "A", "PCAR", "DLR", "PH", "HSY", "TDG", "WEC", "ADM", "MSI", "FDC", "PANW", "WLTW", "CXO", "MTB", "FOX", "ALGN", "MCHP", "SYF", "IMO", "LSXMA", "HPE", "MELI", "VRSN", "PPL", "SBAC", "DTE", "UAL", "ROK", "VRSK", "MCK", "LULU", "ES", "ES", "INFO", "SWK", "GPN", "CTAS", "HRL", "FLT", "FE", "FE", "VTR", "TSN", "O", "EIX", "NTRS", "ULTA", "BXP", "CNC", "CERN", "KR", "CQP", "FCX", "KLAC", "MKC", "LBTYA", "SPLK", "HES", "K", "WY", "FAST", "VEEV", "HRS", "BBY", "CMG", "AME", "CBS", "AMP", "CLX", "BLL", "IDXX", "NTAP", "EXPE", "NEM", "AWK", "MSCI", "HIG", "CLR", "ESS", "CDNS", "MTD", "IAC", "BEN", "CHD", "FITB", "IP", "OMC", "ETR", "CSGP", "DXC", "NUE", "TSS", "FANG", "SNPS", "AEE", "KEYS", "LLL", "WAT", "DHI", "GWW", "KEY", "LNG", "GRMN", "SSNC", "GPC", "INCY", "MXIM", "MTCH", "LEN", "FTNT", "TEVA", "VMC", "CFG", "DISH", "ANSS", "AGR", "TWLO", "ABC", "SWKS", "MRVL", "BMRN", "RF", "CDW", "WDC", "ARE", "WYNN", "SYMC", "SNAP", "PFG", "CMS", "EFX", "LH", "AAL", "L", "CNP", "DISCA", "CPRT", "AJG", "XYL", "NDAQ", "MGM", "RMD", "CAG", "HBAN", "DVN", "MRO", "IT", "COO", "IFF", "CINF", "HCP", "DRI", "ROL", "DOV", "STX", "HST", "MKL", "MYL", "MYL", "GDDY", "CAH", "W", "APA", "LYV", "EXPD", "TFX", "SJM", "LNC", "CE", "CTXS", "HOLX", "TRU", "CTL", "ACGL", "WPC", "MLM", "BR", "TIF", "NBL", "WCG", "AAP", "TAP", "SIVB", "KKR", "NCLH", "TSCO", "EXR", "KMX", "VNO", "AKAM", "ODFL", "VIAB", "UDR", "CMA", "VAR", "RJF", "WAB", "CHRW", "SGEN", "HEI", "TXT", "MAA", "ETFC", "CNA", "EXAS", "ABMD", "UBNT", "DGX", "PCG", "MAS", "KSU", "CPB", "IEX", "AES", "NRG", "ALLY", "ATO", "EMN", "COG", "IONS", "UHS", "KSS", "XRAY", "HAS", "PAYC", "JKHY", "FTI", "FTI", "CBOE", "JBHT", "LNT", "PKI", "DATA", "JEC", "OKTA", "LII", "DRE", "BURL", "DXCM", "STE", "PNW", "FMC", "RE", "ULTI", "TRMB", "NOV", "OTEX", "DPZ", "MOS", "IRM", "NI", "TTWO", "RL", "ELS", "SUI", "FDS", "CPT", "URI", "MKTX", "FFIV", "FRT", "WLK", "GPS", "SE", "PVH", "LKQ", "CF", "JNPR", "AGNC", "LEA", "AVY", "TMK", "CGNX", "ALNY", "CVNA", "QRVO", "MHK", "DOCU", "ARNC", "ON", "RGA", "LDOS", "WRK", "NLSN", "DBX", "UA", "Y", "PKG", "SPR", "TRGP", "HSIC", "DVA", "BWA", "TTD", "IPGP", "BIO", "SRPT", "ZEN", "ZION", "AFG", "ALLE", "SEIC", "WEX", "ALB", "GGG", "SNA", "WHR", "RNG", "ADS", "MTN", "BRO", "IVZ", "IPG", "GLPI", "BLUE", "GWRE", "TYL", "COTY", "NNN", "WU", "NDSN", "ERIE", "BKI", "PHM", "ST", "FICO", "HFC", "VOYA", "H", "LAMR", "BAH", "RPM", "RHI", "ETSY", "XRX", "UNM", "AOS", "VER", "OAK", "ANGI", "ZG", "HLF", "JAZZ", "TTC", "MDB", "NBIX", "WWE", "USFD", "AZPN", "ARMK", "ARMK", "BERY", "HDS", "EEFT", "KRC", "M", "CCK", "ALK", "EWBC", "CDK", "NTNX", "OHI", "ZAYO", "NWS", "KAR", "BFAM", "POST", "TECH", "AIV", "SLG", "BG", "KIM", "UHAL", "CSL", "TRIP", "ALV", "ARW", "SEE", "STOR", "MASI", "COLM", "FL", "XEC", "JLL", "LB", "FBHS", "NRZ", "PNR", "EXEL", "AMH", "HUBS", "PBCT", "LOGI", "ATR", "PRGO", "HUBB", "CREE", "G", "WBC", "XPO", "HP", "HRC", "POOL", "DCI", "HBI", "CRL", "DLB", "FLIR", "JWN", "CC", "SERV", "MPWR", "CBSH", "GH", "ROKU", "CFR", "PE", "HOG", "ACC", "ORI", "CZR", "NWL", "FLS", "AXTA", "WTR", "FSLR", "PK", "DNKN", "NATI", "LPLA", "WPX", "BOKF", "INGR", "RS", "PPC", "USG", "GRUB", "PII", "CIEN", "WWD", "RP", "CY", "APO", "FLEX", "OLLI", "AIZ", "ALSN", "CUBE", "HXL", "COUP", "SWI", "NKTR", "HUN", "EPR", "AMG", "LOPE", "Mac", "PWR", "LEG", "AYI", "VSM", "FLR", "NYT", "GNTX", "PSTG", "TOL", "AGCO", "PEGA", "EQT", "SKX", "GDI", "SNX", "MKSI", "BPOP", "NXST", "PODD", "ALKS", "HRB", "RIG", "CR", "BRX", "NFG", "AYX", "TDC", "SEB", "MAN", "BPL", "AA", "GRA", "CHE", "FCNCA", "SRCL", "BMS", "ARRY", "AVT", "COMM", "JBLU", "ACM", "CIT", "CHK", "ASH", "ZNGA", "LFUS", "LAZ", "GWR", "IDA", "CASY", "MUR", "MDSO", "NUAN", "CNK", "TLRY", "RDN", "HHC", "BHF", "ICUI", "AGO", "EV", "TREE", "PACW", "VAC", "WAL", "JBL", "UTHR", "BC", "CACI", "GT", "PS", "MAT", "LITE", "OGS", "TFSL", "FND", "OMF", "GMED", "SMG", "MSM", "FLO", "IART", "LSTR", "MMS", "SLM", "WSM", "SWX", "PNFP", "LSI", "HQY", "TNET", "SAIC", "NJR", "TXRH", "BKH", "JCOM", "LANC", "PINC", "EME", "NVCR", "WEN", "PFGC", "CARG", "SIX", "AL", "IBKC", "AWI", "ALE", "RYN", "OLN", "FGEN", "LOGM", "SBGI", "TRCO", "CLH", "PEB", "SIGI", "VRNT", "TECD", "MTZ", "COR", "EXP", "BCO", "CUZ", "PBF", "THO", "DKS", "BLKB", "SATS", "COHR", "AEO", "HWC", "CHDN", "ESGR", "BPMC", "SAVE", "MANH", "CBRL", "WRI", "FNB", "STAG", "GHC", "ACAD", "IRBT", "FHB", "AAXN", "SMTC", "AAN", "THS", "TGNA", "TNDM", "CIM", "ISBC", "ISBC", "BKU", "R", "REXR", "AGIO", "MIC", "NCR", "NWE", "SBRA", "ELLI", "TPX", "SRC", "AN", "LIVN", "AMCX", "SHO", "SHO", "OUT", "ENR", "NAV", "MFA", "UMBF", "FII", "PGRE", "GBT", "ALRM", "APU", "ATI", "PTEN", "FEYE", "GDOT", "OMCL", "CSOD", "MRCY", "HOMB", "BCPC", "URBN", "LHCG", "LPX", "HLI", "DORM", "SAM", "NOVT", "SAFM", "CLDR", "FOLD", "TRN", "SKYW", "PRLB", "ENS", "UFS", "NAVI", "HELE", "OI", "SPB", "TCBI", "YELP", "IGT", "SXT", "NUVA", "ORA", "SJI", "X", "DK", "VMI", "FNSR", "WTS", "AB", "KWR", "CMD", "TERP", "GEL", "LM", "CLF", "BOX", "BGCP", "COKE", "CNDT", "ICPT", "RES", "ESI", "AVA", "MDP", "CRUS", "CMPR", "BL", "IBOC", "HI", "AR", "KFY", "NEP", "CCOI", "ALTR", "ALTR", "RRC", "SFM", "SITE", "AJRD", "BRC", "ACHC", "FUL", "AHL", "WCC", "HCSG", "FIZZ", "AGI", "ARI", "SWN", "ABM", "ACIA", "GTN", "HALO", "EE", "ARLP", "MYGN", "GHDX", "FELE", "AIT", "ARNA", "TR", "IPAR", "MGEE", "YEXT", "GEO", "WSBC", "CVA", "WK", "WOR", "ADNT", "BBBY", "AKR", "BRKS", "MRTX", "IDCC", "DLPH", "PEGI", "SANM", "KBH", "CNS", "SHAK", "ALGT", "MC", "AIMC", "AMKR", "AEIS", "EGHT", "CAKE", "INT", "CRSP" };
			var combinedList = snPList500
				.Concat(snPList400)
				.Concat(large1000Companies)
				.Distinct().ToArray();

			return combinedList;
		}

		internal static void RegisterDependencyInjections(this IServiceCollection services, IConfigurationRoot configuration)
		{
			var loggerFactory = new LoggerFactory();
			loggerFactory.AddNLog();
			services.AddSingleton<ILoggerFactory>(loggerFactory);
			var logSection = configuration.GetSection("Logging");
			services.AddLogging(builder =>
			{
				builder.AddConfiguration(configuration.GetSection("Logging"))
				.AddConsole();
			});

			services.AddScoped<IDBConnectionHandler<CompanyDetailMd>, DBConnectionHandler<CompanyDetailMd>>();
			services.AddScoped<IDBConnectionHandler<CompanyFinancialsMd>, DBConnectionHandler<CompanyFinancialsMd>>();
			services.AddScoped<IDBConnectionHandler<OutstandingSharesMd>, DBConnectionHandler<OutstandingSharesMd>>();
			services.AddScoped<IDBConnectionHandler<PiotroskiScoreMd>, DBConnectionHandler<PiotroskiScoreMd>>();

			services.AddScoped<DownloadListedFirms>();
			services.AddScoped<DownloadReportableItems>();
			services.AddScoped<HandleCompanyList>();
			services.AddScoped<HandleFinacials>();
			services.AddScoped<HandleSharesOutStanding>();
			services.AddScoped<IDownloadOutstandingShares, DownloadOutstandingShares>();
			services.AddScoped<ListOfStatements>();
			services.AddScoped<DataFileReader>();
			services.AddScoped<WriteAnalyzedValues>();
		}

		#endregion Internal Methods
	}
}