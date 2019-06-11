using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class Symbols
	{
		public SecuritySymbol[] SymobalList { get; set; }
	}

	

	public class SecuritySymbol
	{
		public string symbol { get; set; }
		public string name { get; set; }
		public string date { get; set; }
		public bool isEnabled { get; set; }
		public string type { get; set; }
		public string iexId { get; set; }
	}

}
