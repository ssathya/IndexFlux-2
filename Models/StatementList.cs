using System.Collections.Generic;

namespace Models
{
	public class StatementList
	{
		public List<StatementDetails> Pl { get; set; }
		public List<StatementDetails> Bs { get; set; }
		public List<StatementDetails> Cf { get; set; }
		public string CompanyId { get; set; }
	}
	public class StatementDetails
	{
		public string Period { get; set; }
		public int Fyear { get; set; }
		public bool Calculated { get; set; }
	}
}
