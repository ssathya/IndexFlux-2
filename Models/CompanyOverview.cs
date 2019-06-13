namespace Models
{
	public class CompanyOverview
	{
		public string Symbol { get; set; }
		public string CompanyName { get; set; }
		public string Exchange { get; set; }
		public string Industry { get; set; }
		public string Website { get; set; }
		public string Description { get; set; }
		public string Ceo { get; set; }
		public string IssueType { get; set; }
		public string Sector { get; set; }
		public string[] Tags { get; set; }
	}
}