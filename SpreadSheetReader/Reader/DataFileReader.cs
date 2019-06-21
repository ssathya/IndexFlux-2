using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ExcelDataReader;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace SpreadSheetReader.Reader
{
	public class DataFileReader
	{
		private const int BufferSize = 32768;
		public List<DataCollection> dataCollection;

		public DataFileReader()
		{
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			dataCollection = new List<DataCollection>();
		}

		public void ParseKeyFinance(string inputFile)
		{
			using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
			{
				ReadExcelFile(stream);
			}
		}

		public async Task ParseKeyFinanceFromS3(string bucketName, RegionEndpoint region, string objectName)
		{
			var client = new AmazonS3Client(region);
			var request = new GetObjectRequest
			{
				BucketName = bucketName,
				Key = objectName
			};
			var fileName = GetTempFileName();
			using (var fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, BufferSize,
				FileOptions.RandomAccess | FileOptions.DeleteOnClose))
			using (GetObjectResponse response = await client.GetObjectAsync(request))
			using (Stream responseStream = response.ResponseStream)
			{
				var data = new byte[BufferSize];
				int bytesRead = 0;
				do
				{
					bytesRead = responseStream.Read(data, 0, BufferSize);
					fs.Write(data, 0, bytesRead);
				}
				while (bytesRead > 0);
				ReadExcelFile(fs);
			}
		}

		private string GetTempFileName()
		{
			return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");
		}

		private void ReadExcelFile(Stream stream)
		{
			using (var reader = ExcelReaderFactory.CreateReader(stream))
			{
				var result = reader.AsDataSet(new ExcelDataSetConfiguration
				{
					ConfigureDataTable = (data) => new ExcelDataTableConfiguration
					{
						UseHeaderRow = true,
						ReadHeaderRow = rowReader =>
						{
							rowReader.Read();
							rowReader.Read();
							rowReader.Read();
						}
					}
				});
				DataTableCollection table = result.Tables;
				DataTable resultTable = table["results"];
				PopulateCollection(resultTable);
			}
		}

		private void PopulateCollection(DataTable table)
		{
			for (int row = 0; row < table.Rows.Count; row++)
			{
				var rowValue = table.Rows[row];
				var rowRecord = new DataCollection();
				foreach (DataColumn column in table.Columns)
				{
					StoreValueInRecord(column, rowValue, rowRecord);
				}
				if (!string.IsNullOrWhiteSpace(rowRecord.CompanyName) && !string.IsNullOrWhiteSpace(rowRecord.Ticker))
				{
					dataCollection.Add(rowRecord);
				}
			}
		}

		private void StoreValueInRecord(DataColumn column, DataRow rowValue, DataCollection rowRecord)
		{
			bool conversionResult;
			float floatConvValue;
			int intConvValue;
			float million = 1000000;
			switch (column.ColumnName)
			{
				case "Column1":
					rowRecord.CompanyName = rowValue[column].ToString();
					break;

				case "Sector":
					rowRecord.Sector = rowValue[column].ToString();
					break;

				case "Ticker":
					rowRecord.Ticker = rowValue[column].ToString();
					break;

				case "Revenues (in million, TTM, USD)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.Revenue = conversionResult ? floatConvValue * million : 0;
					break;

				case "EBITDA (in million, TTM, USD)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.EbitdaCurrent = conversionResult ? floatConvValue * million : 0;
					break;

				case "EBITDA (in million, TTM-1, USD)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.Ebitda1YrAgo = conversionResult ? floatConvValue * million : 0;
					break;

				case "EBITDA (in million, TTM-2, USD)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.Ebitda2YrAgo = conversionResult ? floatConvValue * million : 0;
					break;

				case "EBITDA (in million, TTM-3, USD)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.Ebitda3YrAgo = conversionResult ? floatConvValue * million : 0;
					break;

				case "Net Margin (TTM)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.NetMargin = conversionResult ? floatConvValue * 100 : 0;
					break;

				case "P. F-Score (TTM)":
					conversionResult = int.TryParse(rowValue[column].ToString(), out intConvValue);
					rowRecord.PiotroskiScoreCurrent = conversionResult ? intConvValue : 0;
					break;

				case "P. F-Score (TTM-1)":
					conversionResult = int.TryParse(rowValue[column].ToString(), out intConvValue);
					rowRecord.PiotroskiScore1YrAgo = conversionResult ? intConvValue : 0;
					break;

				case "P. F-Score (TTM-2)":
					conversionResult = int.TryParse(rowValue[column].ToString(), out intConvValue);
					rowRecord.PiotroskiScore2YrAgo = conversionResult ? intConvValue : 0;
					break;

				case "P. F-Score (TTM-3)":
					conversionResult = int.TryParse(rowValue[column].ToString(), out intConvValue);
					rowRecord.PiotroskiScore3YrAgo = conversionResult ? intConvValue : 0;
					break;

				case "Gross Margin (TTM)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.GrossMargin = conversionResult ? floatConvValue * 100 : 0;
					break;

				case "Operating Margin (TTM)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.OperatingMargin = conversionResult ? floatConvValue * 100 : 0;
					break;

				case "ROE (TTM)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.ReturnOnEquity = conversionResult ? floatConvValue * 100 : 0;
					break;

				case "ROA (TTM)":
					conversionResult = float.TryParse(rowValue[column].ToString(), out floatConvValue);
					rowRecord.ReturnOnAssets = conversionResult ? floatConvValue * 100 : 0;
					break;

				default:
					return;
			}
		}
	}
}