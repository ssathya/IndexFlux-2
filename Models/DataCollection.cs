using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class DataCollection
    {
		public string SimId { get; set; }
		public string CompanyName { get; set; }
		public string Sector { get; set; }
		public string Ticker { get; set; }
		public float Revenue { get; set; }
		public float EbitdaCurrent { get; set; }
		public float Ebitda1YrAgo { get; set; }
		public float Ebitda2YrAgo { get; set; }
		public float Ebitda3YrAgo { get; set; }

		public float NetMargin { get; set; }
		public int PiotroskiScoreCurrent { get; set; }
		public int PiotroskiScore1YrAgo { get; set; }
		public int PiotroskiScore2YrAgo { get; set; }
		public int PiotroskiScore3YrAgo { get; set; }
		public float GrossMargin { get; set; }
		public float OperatingMargin { get; set; }
		public float ReturnOnEquity { get; set; }
		public float ReturnOnAssets { get; set; }

	}
}
