using System;
using System.Collections.Generic;

namespace Models
{
	public class PiotroskiScore
	{

		#region Public Properties

		public long EBITDA { get; set; }
		public int FYear { get; set; }
		public DateTime LastUpdate { get; set; }
		public Dictionary<string, decimal> ProfitablityRatios { get; set; }
		public int Rating { get; set; }
		public float? Revenue { get; set; }
		public string SimId { get; set; }
		public string Ticker { get; set; }

		#endregion Public Properties
	}

	public class PiotroskiScoreMd : PiotroskiScore, IBaseModel
	{

		#region Public Properties

		public string Id { get; set; }

		#endregion Public Properties


		#region Public Constructors

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

		#endregion Public Constructors
	}
}