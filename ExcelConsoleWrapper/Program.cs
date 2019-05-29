using SpreadSheetReader.Reader;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExcelConsoleWrapper
{
	class Program
	{
		static void Main(string[] args)
		{
			var dfr = new DataFileReader();
			dfr.ParseKeyFinance(@"c:\users\sridh\Downloads\1. Key Ratios.xlsx");
			var results = dfr.dataCollection.OrderByDescending(d => d.PiotroskiScoreCurrent)
				.ThenByDescending(d=>d.PiotroskiScore1YrAgo)
				.ThenByDescending(d=>d.PiotroskiScore2YrAgo)
				.ThenByDescending(d=>d.PiotroskiScore3YrAgo)
				.ThenByDescending(d=>d.Revenue)				
				.ToArray();
			Console.WriteLine("These are firms that have been doing well for the past 3 years");

			for (
				int i = 0; i < 20; i++)
			{
				Console.WriteLine($"Ticker :{results[i].Ticker} " +
					$"\nName : {results[i].CompanyName} " +
					$"\n\tCurrent rating : {results[i].PiotroskiScoreCurrent} " +
					$"\n\tRating year ago : {results[i].PiotroskiScore1YrAgo} " +
					$"\n\tRating year ago : {results[i].PiotroskiScore1YrAgo} " +
					$"\n\tRating two year ago : {results[i].PiotroskiScore2YrAgo} " +
					$"\n\tRating three years ago : {results[i].PiotroskiScore3YrAgo} " +
					$"\n\tEBITDA : {ToKMB((decimal)results[i].Ebitda)} " +
					$"\n\tRevenues :  {ToKMB((decimal)results[i].Revenue)}" +
					$"\n\tNet Margin : {ToKMB((decimal)results[i].NetMargin)}%" +
					$"\n\tGross Margin : {ToKMB((decimal)results[i].GrossMargin)}" +
					$"\n\tOperating Margin : {ToKMB((decimal)results[i].OperatingMargin)}%" +
					$"\n\tReturn on Equity : {ToKMB((decimal)results[i].ReturnOnEquity)}%" +
					$"\n\tReturn on Assets : {ToKMB((decimal)results[i].ReturnOnAssets)}%\n"); 
			}

		}
		private static string ToKMB(decimal num)
		{
			if (num > 999999999 || num < -999999999)
			{
				return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999999 || num < -999999)
			{
				return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999 || num < -999)
			{
				return num.ToString("0,.#K", CultureInfo.InvariantCulture);
			}
			else
			{
				return num.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}
