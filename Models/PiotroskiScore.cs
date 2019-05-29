using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class PiotroskiScore
    {
		public string SimId { get; set; }
		public int FYear { get; set; }
		public int Rating { get; set; }
		public Dictionary<string,decimal> ProfitablityRatios { get; set; }
		public long EBITDA { get; set; }
		public DateTime LastUpdate { get; set; }
		public string Ticker { get; set; }
	}
	public class PiotroskiScoreMd : PiotroskiScore, IBaseModel
	{
		public string Id { get; set; }
		public PiotroskiScoreMd()
		{

		}
		public PiotroskiScoreMd(PiotroskiScore ps)
		{
			//SimId = ps.SimId;
			//FYear = ps.FYear;
			//Rating = ps.Rating;
			//ProfitablityRatios = ps.ProfitablityRatios;
			//EBITDA = ps.EBITDA;
		}
	}
}
