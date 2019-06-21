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
	public class GetSimIdTests : IDisposable
	{
		private MockRepository mockRepository;

		private Mock<ILogger> mockLogger;

		public GetSimIdTests()
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

		private GetSimId CreateGetSimId()
		{
			return new GetSimId(mockLogger.Object);
		}

		[Fact]
		public async Task GetSimIdByTicker_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateGetSimId();
			string ticker = "C";
			string ticker1 = "bby";

			// Act
			var result = await unitUnderTest.GetSimIdByTicker(
				ticker);
			var result1 = await unitUnderTest.GetSimIdByTicker(
				ticker1);

			// Assert
			Assert.Equal("89126", result);
			Assert.Equal("71192", result1);
		}

		[Fact]
		public async Task GetSimIdByCompanyName_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateGetSimId();
			string companyName = "Citigroup";
			string companyName1 = "Best Buy";
			// Act
			var result = await unitUnderTest.GetSimIdByCompanyName(
				companyName);
			var result1 = await unitUnderTest.GetSimIdByCompanyName(
				companyName1);

			// Assert
			Assert.Equal("89126", result);
			Assert.Equal("71192", result1);
		}
	}
}