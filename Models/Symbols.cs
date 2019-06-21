using System;

namespace Models
{
	public class Symbols
	{
		public SecuritySymbol[] SymobalList { get; set; }
	}

	public class SecuritySymbol
	{
		public string Symbol { get; set; }
		public string Exchange { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }
		public string Type { get; set; }
		public string IexId { get; set; }
		public string Region { get; set; }
		public string Currency { get; set; }
		public bool IsEnabled { get; set; }
	}
}