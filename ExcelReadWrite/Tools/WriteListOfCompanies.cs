using ExcelReadWrite.Extensions;
using HandleSimFin.Methods;
using Humanizer;
using Microsoft.Extensions.Logging;
using Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelReadWrite.Tools
{
	public class WriteListOfCompanies
	{


		#region Private Fields

		private const string workSheetName = "ListedFirms";
		private readonly ILogger _logger;

		private List<CompanyDetail> CompanyDetails;

		#endregion Private Fields


		#region Public Constructors

		public WriteListOfCompanies(ILogger logger)
		{
			_logger = logger;
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<List<CompanyDetail>> GetAllCompanines()
		{
			if (CompanyDetails != null && CompanyDetails.Count != 0)
			{
				return CompanyDetails;
			}
			var dataSource = new DownloadListedFirms(_logger);
			var allCompanies = await ObtainAndCleanExternalData(dataSource);

			foreach (var company in allCompanies)
			{
				company.Name = company.Name.Transform(To.LowerCase, To.TitleCase);
			}
			CompanyDetails = allCompanies;
			return allCompanies;
		}

		public List<CompanyDetail> GetCompanyDetails(string destinationFile)
		{
			if (CompanyDetails != null && CompanyDetails.Count != 0)
			{
				return CompanyDetails;
			}
			using (var package = new ExcelPackage())
			{
				using (var outStream = new FileStream(destinationFile, FileMode.OpenOrCreate))
				{
					package.Load(outStream);
				}
				var workSheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name == workSheetName);
				// CompanyDetails = workSheet.ToList<CompanyDetail>();

				if (workSheet == null)
				{
					return null;
				}
				CompanyDetails = new List<CompanyDetail>();
				var rows = workSheet.Dimension.End.Row;
				try
				{
					for (int i = 2; i <= rows; i++)
					{
						var val1 = workSheet.Cells[$"A{i}"].Text;
						var cd = new CompanyDetail
						{
							SimId = workSheet.Cells[$"A{i}"].Text.Trim(),
							Ticker = workSheet.Cells[$"B{i}"].Text.Trim(),
							Name = workSheet.Cells[$"C{i}"].Text.Trim(),
							IndustryTemplate = workSheet.Cells[$"D{i}"].Text.Trim()
						};
						var strDate = workSheet.Cells[$"E{i}"].Text.Trim();
						if (!string.IsNullOrWhiteSpace(strDate))
						{
							
							var result = DateTime.TryParse(strDate, out DateTime resultValue);
							if (result == true)
							{
								cd.LastUpdate = resultValue;
							}
						}
						CompanyDetails.Add(cd);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					return null;
				}
			}
			return CompanyDetails;
		}
		public async Task WriteAllCompanines(string destinationFile)
		{
			var allCompanies = await GetAllCompanines();

			using (var package = new ExcelPackage())
			{
				ClearOldData(destinationFile, workSheetName, package);
				var worksheet = package.Workbook.Worksheets.Add(workSheetName);
				worksheet.Cells[1, 1].LoadFromCollection(allCompanies, true);

				var fullData = package.GetAsByteArray();
				await File.WriteAllBytesAsync(destinationFile, fullData);
			}
		}

		#endregion Public Methods


		#region Internal Methods

		internal async Task<bool> UpdateIndustryTemplateAsync(string simId, string industryTemplate, string outFile)
		{
			using (var package = new ExcelPackage())
			{
				using (var outStream = new FileStream(outFile, FileMode.OpenOrCreate))
				{
					package.Load(outStream);
				}
				var workSheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name == workSheetName);
				var matchCells = (from cell in workSheet.Cells["a:a"]
								  where cell.Value?.ToString() == simId
								  select cell).ToList();
				if (matchCells == null || matchCells.Count() == 0)
				{
					return false;
				}
				var matchCell = matchCells.First().Address;
				matchCell = matchCell.Replace('A', 'D');
				workSheet.Cells[matchCell].Value = industryTemplate;
				matchCell = matchCell.Replace('D', 'E');
				workSheet.Cells[matchCell].Value = DateTime.Now;
				workSheet.Cells[matchCell].Style.Numberformat.Format =
					DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
				var fullData = package.GetAsByteArray();
				await File.WriteAllBytesAsync(outFile, fullData);
				return true;
			}
		}

		#endregion Internal Methods


		#region Private Methods

		private static void ClearOldData(string destinationFile, string workSheetName, ExcelPackage package)
		{
			using (var outStream = new FileStream(destinationFile, FileMode.OpenOrCreate))
			{
				package.Load(outStream);
			}
			var oldWorkSheet = package.Workbook.Worksheets.SingleOrDefault(x => x.Name == workSheetName);
			if (oldWorkSheet != null)
			{
				package.Workbook.Worksheets.Delete(oldWorkSheet);
			}
		}

		private async Task<List<CompanyDetail>> ObtainAndCleanExternalData(DownloadListedFirms dataSource)
		{
			var allCompanies = await dataSource.GetCompanyList();
			allCompanies = allCompanies.Where(ac => !string.IsNullOrEmpty(ac.Ticker)
						&& !string.IsNullOrEmpty(ac.SimId)
						&& !string.IsNullOrEmpty(ac.Name))
				.OrderBy(ac => ac.Ticker).ToList();
			CompanyDetails = allCompanies;
			return allCompanies;
		}

		#endregion Private Methods

	}
}