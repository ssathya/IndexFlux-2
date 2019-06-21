using System;
using System.Collections.Generic;

namespace Models
{
	public class OutstandingShares
	{
		public string SimId { get; set; }
		public List<OutstandingValue> OutstandingValues { get; set; }
		public DateTime? LastUpdateDate { get; set; }
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

	public class OutstandingSharesMd : OutstandingShares, IBaseModel
	{
		public string Id { get; set; }
	}
}