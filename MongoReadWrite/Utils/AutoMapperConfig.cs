using AutoMapper;
using Models;

namespace MongoReadWrite.Utils
{
	public  class AutoMapperConfig : Profile
	{
		public AutoMapperConfig()
		{
			CreateMap<CompanyFinancials, CompanyFinancialsMd>()
				.ForMember(d => d.Id, t => t.Ignore())
				.ReverseMap();
			CreateMap<CompanyDetail, CompanyDetailMd>()
				.ForMember(d => d.Id, t => t.Ignore())
				.ReverseMap();
		}
		public static void Start()
		{
			// CreateMap<CompanyFinancials, CompanyFinancialsMd>();

			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<CompanyFinancials, CompanyFinancialsMd>()
				.ForMember(d => d.Id, t => t.Ignore())
				.ReverseMap();
				cfg.CreateMap<CompanyDetail, CompanyDetailMd>()
				.ForMember(d => d.Id, t => t.Ignore())
				.ReverseMap();
			});

			//Mapper.Initialize(cf => cf.CreateMap<CompanyFinancials, CompanyFinancialsMd>()
			//	.ForMember(d => d.Id, t => t.Ignore())
			//	.ReverseMap());

			//Mapper.Initialize(cd => cd.CreateMap<CompanyDetail, CompanyDetailMd>()
			//	.ForMember(d => d.Id, t => t.Ignore())
			//	.ReverseMap());
		}

	}
}