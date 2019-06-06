using Microsoft.AspNetCore.Builder;

namespace ServeData.MiddleWare
{
	public static class MiddleWareExtensions
	{
		public static IApplicationBuilder UseAPIKeyMessageHandlerMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<APIKeyCheck>();
		}
	}
}
