using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Models;

namespace ServeData.MiddleWare
{
	public static class MiddleWareExtensions
	{
		public static IApplicationBuilder UseAPIKeyMessageHandlerMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<APIKeyCheck>();
		}

		[System.Obsolete]
		public static void SetupAutoMapper(this IServiceCollection services)
		{
			services.AddAutoMapper(typeof(Startup));
			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<PiotroskiScore, PiotroskiScoreMd>()
					.ForMember(d => d.Id, t => t.Ignore())
					.ReverseMap();
			});
		}
	}
}