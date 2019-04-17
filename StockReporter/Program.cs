using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace StockReporter
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			logger.Debug("In main");
			CreateWebHostBuilder(args).Build().Run();
			logger.Debug("Exiting application");
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
			.ConfigureLogging(logging =>
			{
				logging.ClearProviders();
				logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
			})
			.UseNLog();
	}
}
