using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace HandleSimFinTests.Methods
{
	public class ListOfStatementsTests : IDisposable
	{
		private MockRepository mockRepository;

		private Mock<ILogger> mockLogger;

		public ListOfStatementsTests()
		{
			this.mockRepository = new MockRepository(MockBehavior.Strict);

			this.mockLogger = new Mock<ILogger>();
			dynamic res = JsonConvert.DeserializeObject(File.ReadAllText("appsettings.json"));
			Environment.SetEnvironmentVariable("SimFinKey", (string)res.SimFinKey, EnvironmentVariableTarget.Process);
		}

		public void Dispose()
		{
			this.mockRepository.VerifyAll();
		}

		private ListOfStatements CreateListOfStatements()
		{
			return new ListOfStatements(
				this.mockLogger.Object);
		}

		[Fact]
		public async Task FetchStatementList_StateUnderTest_ExpectedBehaviorCompanyName()
		{
			// Arrange
			var unitUnderTest = this.CreateListOfStatements();
			string identifyer = "Citigroup";
			IdentifyerType identifyerType = IdentifyerType.CompanyName;

			// Act
			var result = await unitUnderTest.FetchStatementList(
				identifyer,
				identifyerType);

			// Assert
			Assert.True(result.Bs.Count != 0);
		}

		[Fact]
		public async Task FetchStatementList_StateUnderTest_ExpectedBehaviorTicker()
		{
			// Arrange
			var unitUnderTest = this.CreateListOfStatements();
			string identifyer = "BBY";
			IdentifyerType identifyerType = IdentifyerType.Ticker;

			// Act
			var result = await unitUnderTest.FetchStatementList(
				identifyer,
				identifyerType);

			// Assert
			Assert.True(result.Bs.Count != 0);
		}
		[Fact]
		public async Task FetchStatementList_StateUnderTest_ExpectedBehaviorSimId()
		{
			// Arrange
			var unitUnderTest = this.CreateListOfStatements();
			string identifyer = "71192";
			IdentifyerType identifyerType = IdentifyerType.SimFinId;

			// Act
			var result = await unitUnderTest.FetchStatementList(
				identifyer,
				identifyerType);

			// Assert
			Assert.True(result.Bs.Count != 0);
		}
		[Fact]
		public async Task FetchStatementList_StateUnderTest_UnExpectedBehaviorSimId()
		{
			// Arrange
			var unitUnderTest = this.CreateListOfStatements();
			string identifyer = "0";
			IdentifyerType identifyerType = IdentifyerType.SimFinId;

			// Act
			var result = await unitUnderTest.FetchStatementList(
				identifyer,
				identifyerType);

			// Assert
			Assert.True(result == null || result.Bs.Count == 0);
		}
		[Fact]
		public void FactStatementList_RemovePastTTMs_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateListOfStatements();
			string identifyer = "71192";
			IdentifyerType identifyerType = IdentifyerType.SimFinId;

			// Act
			var result =  unitUnderTest.FetchStatementList(
				identifyer,
				identifyerType).Result;
			if (result != null)
			{
				result = unitUnderTest.RemovePastTTMs(result);
			}
			var bs = result.Bs.FindAll(b => b.Period.Contains("TTM-"));
			//Assert
			Assert.True(bs.Count == 0);
		}
		[Fact]
		public void FactStatementList_ExtractYearEndReports_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateListOfStatements();
			string identifyer = "71192";
			IdentifyerType identifyerType = IdentifyerType.SimFinId;

			// Act
			var result = unitUnderTest.FetchStatementList(
				identifyer,
				identifyerType).Result;
			if (result != null)
			{
				result = unitUnderTest.ExtractYearEndReports(result);
			}

			var bs = result.Bs.FindAll(b => b.Period.Contains("TTM-"));
			var bs1 = result.Pl.FindAll(b => b.Period.Contains("FY"));
			//Assert
			Assert.True(bs.Count == 0);
			Assert.False(bs1.Count == 0);
		}
	}
}
