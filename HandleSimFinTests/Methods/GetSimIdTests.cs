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

		private Mock<ILogger<GetSimId>> mockLogger;

		public GetSimIdTests()
		{
			this.mockRepository = new MockRepository(MockBehavior.Strict);

			this.mockLogger = new Mock<ILogger<GetSimId>>();
			dynamic res = JsonConvert.DeserializeObject(File.ReadAllText("appsettings.json"));			
			Environment.SetEnvironmentVariable("SimFinKey", (string)res.SimFinKey, EnvironmentVariableTarget.Process);			

		}

		public void Dispose()
		{
			this.mockRepository.VerifyAll();
		}

		private GetSimId CreateGetSimId()
		{
			//var mock = new Mock<ILogger<GetSimId>>();
			//var logger = mock.Object;
			//return new GetSimId(
			//	this.mockLogger.Object);
			return new GetSimId(mockLogger.Object);
		}

		[Fact]
		public async Task GetSimIdByTicker_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateGetSimId();
			string ticker = "C";

			// Act
			var result = await unitUnderTest.GetSimIdByTicker(
				ticker);

			// Assert
			Assert.Equal("89126", result);
		}
		[Fact]
		public async Task GetSimIdByCompanyName_StateUnderTest_ExpectedBehavior()
		{
			// Arrange
			var unitUnderTest = this.CreateGetSimId();
			string companyName = "Citigroup";

			// Act
			var result = await unitUnderTest.GetSimIdByCompanyName(
				companyName);

			// Assert
			Assert.Equal("89126", result);
		}
	}
}
