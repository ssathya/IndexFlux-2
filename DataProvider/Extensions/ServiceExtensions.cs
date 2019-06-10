using Amazon;
using DataProvider.BusLogic;
using HandleSimFin.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Extensions
{
    public static class ServiceExtensions
    {
		public static string BucketName = @"talk2control-1";
		public static RegionEndpoint Region = RegionEndpoint.USEast1;
		public static void AddKeysToEnvironment(this IServiceCollection services)
		{
			var readS3Objs = new ReadS3Objects(BucketName, Region);
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
		public static void SetupDependencies(this IServiceCollection services)
		{			
			services.AddScoped<ObtainMarketSummary>();
			services.AddScoped<ObtainTrenders>();
			services.AddScoped<ObtainNews>();
			services.AddScoped<ObtainStockQuote>();
			services.AddScoped<EnvHandler>();
		}

	}
}
