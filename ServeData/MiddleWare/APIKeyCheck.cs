using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServeData.MiddleWare
{
	public class APIKeyCheck
    {
		private readonly RequestDelegate _next;
		private const string apiKey = "8439E57A-79FD-4460-AA69-8B063B769D43";
		private const string KeyName = "key";

		public APIKeyCheck(RequestDelegate next)
		{
			_next = next;
		}
		public async Task Invoke(HttpContext context)
		{
			bool validKey = false;
			var checkApiKeyExists = context.Request.Headers.ContainsKey(KeyName);
			if (checkApiKeyExists)
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
