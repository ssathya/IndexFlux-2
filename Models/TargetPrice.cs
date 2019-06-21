using System;

namespace Models
{
	public class TargetPrice
	{
		public string Symbol { get; set; }
		public DateTime UpdatedDate { get; set; }
		public float PriceTargetAverage { get; set; }
		public int PriceTargetHigh { get; set; }
		public int PriceTargetLow { get; set; }
		public int NumberOfAnalysts { get; set; }
	}
}