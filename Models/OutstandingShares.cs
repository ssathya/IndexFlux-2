using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class OutstandingShares
	{
		public string SimId { get; set; }
		public List<OutstandingValue> OutstandingValues { get; set; }
	}

	

	public class OutstandingValue
	{
		public string Figure { get; set; }
		public string Type { get; set; }
		public string Measure { get; set; }
		public DateTime? Date { get; set; }
		public string Period { get; set; }
		public int? Fyear { get; set; }
		public long? Value { get; set; }
	}

}
