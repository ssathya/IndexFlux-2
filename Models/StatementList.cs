using System;
using System.Collections.Generic;

namespace Models
{
	public class StatementList
	{
		public StatementList()
		{
			Pl = new List<StatementDetails>();
			Bs = new List<StatementDetails>();
			Cf = new List<StatementDetails>();
		}

		public List<StatementDetails> Pl { get; set; }
		public List<StatementDetails> Bs { get; set; }
		public List<StatementDetails> Cf { get; set; }
		public string CompanyId { get; set; }
	}

	public class StatementDetails
	{
		public string Period { get; set; }
		public int Fyear { get; set; }
		public bool Calculated { get; set; }
	}

	public class StatementListMd : StatementList, IBaseModel
	{
		public StatementListMd()
		{
		}

		public StatementListMd(StatementList sl)
		{
			Pl = sl.Pl;
			Bs = sl.Bs;
			Cf = sl.Cf;
			CompanyId = sl.CompanyId;
		}

		public string Id { get; set; }
		public DateTime? LastUpdateDate { get; set; }
	}
}