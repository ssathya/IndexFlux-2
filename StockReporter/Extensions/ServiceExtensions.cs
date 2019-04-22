using Amazon;
using HandleSimFin.Methods;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Newtonsoft.Json;
using StockReporter.Helpers;
using System;
using System.Collections.Generic;

namespace StockReporter.Extensions
{
	public static class ServiceExtensions
	{
		internal static void AddKeysToEnvironment(this IServiceCollection services)
		{
			var readS3Objs = new ReadS3Objects(@"talk2control-1", RegionEndpoint.USEast1);

			var keysToServices = JsonConvert
				.DeserializeObject<List<EntityKeys>>(readS3Objs
					.GetDataFromS3("Random.txt")
				.Result);
			foreach (var entityKeys in keysToServices)
			{
				if (!string.IsNullOrEmpty(entityKeys.Entity)
					&& !string.IsNullOrEmpty(entityKeys.Key))
					Environment.SetEnvironmentVariable(entityKeys.Entity, entityKeys.Key);
			}
		}

		internal static void SetupDependencyInjection(this IServiceCollection services)
		{
			services.AddTransient<IDownloadMarketSummary, DownloadMarketSummary>();
			services.AddTransient<IDownloadStockQuote, DownloadStockQuote>();
		}
	}
}