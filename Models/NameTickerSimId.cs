using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class NameTickerSimId
	{
		public CompanyDetail[] CompanyDetails { get; set; }
	}

	public class CompanyDetail
	{
		public string SimId { get; set; }
		public string Ticker { get; set; }
		public string Name { get; set; }
	}

}
