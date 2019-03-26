using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using OfficeOpenXml;
using System.Collections.Generic;
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

		#endregion Private Fields


		#region Public Properties

		private List<CompanyDetail> CompanyDetails;

		#endregion Public Properties


		#region Public Constructors

		public WriteListOfCompanies(ILogger logger)
		{
			_logger = logger;
		}

		#endregion Public Constructors


		#region Public Methods

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
				
				if (workSheet == null)
				{
					return null;
				}
				CompanyDetails = new List<CompanyDetail>();
				var rows = workSheet.Dimension.End.Row;
				for (int i = 2; i <= rows; i++)
				{
					var cd = new CompanyDetail
					{
						SimId = workSheet.Cells[$"A{i}"].Value.ToString(),
						Ticker = workSheet.Cells[$"B{i}"].Value.ToString(),
						Name = workSheet.Cells[$"C{i}"].Value.ToString()
					};
					CompanyDetails.Add(cd);
				}

			}
			return CompanyDetails;
		}

		public async Task WriteAllCompanines(string destinationFile)
		{
			var dataSource = new DownloadListedFirms(_logger);
			var allCompanies = await ObtainAndCleanExternalData(dataSource);

			

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