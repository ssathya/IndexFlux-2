using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoReadWrite.Tools;
using MongoReadWrite.Utils;
using System;
using System.Threading.Tasks;

namespace MongoReadWrite
{
	class Program
	{
		static void Main(string[] args)
		{
			var apiKey = GetApiKey();
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				Console.WriteLine("API key is empty");
			}
			else
			{
				Console.WriteLine($"Api KEY: {apiKey}");
			}
			var a = new HandleCompanyList();
			var b = a.GetAllCompaniesAsync().Result;
			foreach (var company in b)
			{
				Console.WriteLine($"{company.Name} => {company.Ticker}");
			}
		}

		

		internal static string GetApiKey()
		{
			var apiKey = Environment.GetEnvironmentVariable("SimFinKey", EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				Console.WriteLine("Did not find API key in process");
				apiKey = Environment.GetEnvironmentVariable("SimFinKey", EnvironmentVariableTarget.Machine);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				Console.WriteLine("Did not find API key in Machine");
				apiKey = Environment.GetEnvironmentVariable("SimFinKey", EnvironmentVariableTarget.User);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				Console.WriteLine("Did not find API key in Machine");
				apiKey = Environment.GetEnvironmentVariable("SimFinKey");
			}

			return apiKey;
		}
	}
}
