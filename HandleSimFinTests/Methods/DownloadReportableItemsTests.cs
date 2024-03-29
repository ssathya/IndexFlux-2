using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HandleSimFinTests.Methods
{
	public class DownloadReportableItemsTests : IDisposable
	{
		private MockRepository mockRepository;

		private Mock<ILogger<DownloadReportableItems>> mockLogger;

		public DownloadReportableItemsTests()
		{
			this.mockRepository = new MockRepository(MockBehavior.Strict);

			this.mockLogger = new Mock<ILogger<DownloadReportableItems>>();
			dynamic res = JsonConvert.DeserializeObject(File.ReadAllText("appsettings.json"));
			Environment.SetEnvironmentVariable("SimFinKey", (string)res.SimFinKey, EnvironmentVariableTarget.Process);
		}

		public void Dispose()
		{
			this.mockRepository.VerifyAll();
		}

		private DownloadReportableItems CreateDownloadReportableItems()
		{
			return new DownloadReportableItems(
				this.mockLogger.Object);
		}

		[Fact]
		public async Task DownloadFinancialsAsync_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateDownloadReportableItems();
			StatementList statementList = JsonConvert.DeserializeObject<StatementList>(File.ReadAllText(@"C:\Users\sridh\OneDrive\Documents\Visual Studio 2019\Projects\DataProvider\Data\result.json"));

			// Act
			var result = await unitUnderTest.DownloadFinancialsAsync(
				statementList);
			var countOfBs = result.Where(r => r.Statement == StatementType.BalanceSheet).Count();
			var countOfPl = result.Where(r => r.Statement == StatementType.ProfitLoss).Count();
			var countOfCf = result.Where(r => r.Statement == StatementType.CashFlow).Count();

			// Assert
			Assert.True(result != null);
			Assert.True(countOfBs != 0);
			Assert.True(countOfPl != 0);
			Assert.True(countOfCf != 0);
			var txtToWrite = JsonConvert.SerializeObject(result, Formatting.Indented);
			File.WriteAllText(@"C:\Users\sridh\OneDrive\Documents\Visual Studio 2019\Projects\DataProvider\Data\FinData.json", txtToWrite);
		}
	}
}