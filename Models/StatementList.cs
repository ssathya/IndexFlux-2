using System.Collections.Generic;

namespace Models
{
	public class StatementList
	{
		public List<Pl> Pl { get; set; }
		public List<Bs> Bs { get; set; }
		public List<Cf> Cf { get; set; }
	}
	public class Pl
	{
		public string Period { get; set; }
		public int Fyear { get; set; }
		public bool Calculated { get; set; }
	}

	public class Bs
	{
		public string Period { get; set; }
		public int Fyear { get; set; }
		public bool Calculated { get; set; }
	}

	public class Cf
	{
		public string Period { get; set; }
		public int Fyear { get; set; }
		public bool Calculated { get; set; }
	}

}
