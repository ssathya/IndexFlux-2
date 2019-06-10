using System;

namespace Models
{
	public class CompanyDetail
	{
		public string SimId { get; set; }
		public string Ticker { get; set; }
		public string Name { get; set; }
		public string IndustryTemplate { get; set; }
		public DateTime? LastUpdate { get; set; }
	}
	public class CompanyDetailMd : CompanyDetail, IBaseModel
	{
		public CompanyDetailMd()
		{

		}
		public CompanyDetailMd(CompanyDetail cd)
		{
			SimId = cd.SimId;
			Ticker = cd.Ticker;
			Name = cd.Name;
			IndustryTemplate = cd.IndustryTemplate;
			LastUpdate = cd.LastUpdate;
		}
		public string Id { get; set; }
	}

}