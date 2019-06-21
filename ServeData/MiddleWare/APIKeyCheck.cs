using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ServeData.MiddleWare
{
	public class APIKeyCheck
	{
		private readonly RequestDelegate _next;
		private const string KeyName = "key";

		public APIKeyCheck(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var apiKey = Environment.GetEnvironmentVariable("AppMaster");
			bool validKey = false;
			var checkApiKeyExists = context.Request.Headers.ContainsKey(KeyName);
			if (checkApiKeyExists && !string.IsNullOrWhiteSpace(apiKey))
			{
				if (context.Request.Headers[KeyName].Equals(apiKey))
				{
					validKey = true;
				}
			}
			if (!validKey)
			{
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				await context.Response.WriteAsync("Not authorized");
			}
			await _next.Invoke(context);
		}
	}
}