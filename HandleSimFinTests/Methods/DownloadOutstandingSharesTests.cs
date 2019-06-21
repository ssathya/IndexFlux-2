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
	public class DownloadOutstandingSharesTests : IDisposable
	{
		private MockRepository mockRepository;

		private Mock<ILogger<DownloadOutstandingShares>> mockLogger;

		public DownloadOutstandingSharesTests()
		{
			this.mockRepository = new MockRepository(MockBehavior.Strict);

			this.mockLogger = new Mock<ILogger<DownloadOutstandingShares>>();
			dynamic res = JsonConvert.DeserializeObject(File.ReadAllText("appsettings.json"));
			Environment.SetEnvironmentVariable("SimFinKey", (string)res.SimFinKey, EnvironmentVariableTarget.Process);
		}

		public void Dispose()
		{
			this.mockRepository.VerifyAll();
		}

		private DownloadOutstandingShares CreateDownloadOutstandingShares()
		{
			return new DownloadOutstandingShares(
				this.mockLogger.Object);
		}

		[Fact]
		public async Task ObtainAggregatedList_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateDownloadOutstandingShares();
			string simId = "244314";

			// Act
			var result = await unitUnderTest.ObtainAggregatedList(
				simId);

			// Assert
			Assert.True(result.OutstandingValues.Count != 0);
		}
	}
}