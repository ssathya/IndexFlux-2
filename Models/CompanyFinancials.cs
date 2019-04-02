using System.Collections.Generic;

namespace Models
{
	public enum StatementType
	{
		BalanceSheet,
		CashFlow,
		ProfitLoss
	};

	public class CalculationScheme
	{
		#region Public Properties

		public int Fyear { get; set; }
		public string Period { get; set; }
		public int Sign { get; set; }
		public int StatementId { get; set; }

		#endregion Public Properties
	}

	public class CompanyFinancials
	{
		#region Public Properties

		public bool Calculated { get; set; }
		public List<CalculationScheme> CalculationSchemes { get; set; }
		public string CompanyId { get; set; }
		public int FYear { get; set; }
		public string IndustryTemplate { get; set; }
		public StatementType Statement { get; set; }
		public List<Value> Values { get; set; }
		#endregion Public Properties
	}
	public class CompanyFinancialsMd : CompanyFinancials, IBaseModel
	{
		public CompanyFinancialsMd()
		{

		}
		public CompanyFinancialsMd(CompanyFinancials cf)
		{
			Calculated = cf.Calculated;
			CalculationSchemes = cf.CalculationSchemes;
			CompanyId = cf.CompanyId;
			FYear = cf.FYear;
			IndustryTemplate = cf.IndustryTemplate;
			Statement = cf.Statement;
			Values = cf.Values;
		}
		public string Id { get; set; }
	}
	public class Value
	{
		#region Public Properties

		public bool CheckPossible { get; set; }
		public int DisplayLevel { get; set; }
		public int Parent_tid { get; set; }
		public string StandardisedName { get; set; }
		public int Tid { get; set; }
		public int Uid { get; set; }
		public long? ValueAssigned { get; set; }
		public long? ValueCalculated { get; set; }
		public long? ValueChosen { get; set; }

		#endregion Public Properties
	}
}