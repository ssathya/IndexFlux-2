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
}