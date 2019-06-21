namespace Models
{
	public class InsuranceCashFlow
	{
		public bool Calculated { get; set; }
		public string CompanyId { get; set; }
		public int FYear { get; set; }
		public string IndustryTemplate { get; set; }
		public StatementType Statement { get; set; }
		public long? NetIncome_StartingLine_1 { get; set; }
		public long? NetIncome_2 { get; set; }
		public long? NetIncomeFromDiscontinuedOperations_3 { get; set; }
		public long? OtherAdjustments_4 { get; set; }
		public long? DepreciationAmortization_5 { get; set; }
		public long? NonCashItems_6 { get; set; }
		public long? StockBasedCompensation_7 { get; set; }
		public long? DeferredIncomeTaxes_8 { get; set; }
		public long? OtherNonCashAdjustments_9 { get; set; }
		public long? NetChangeinOperatingCapital_10 { get; set; }
		public long? NetCashFromDiscontinuedOperations_operating__11 { get; set; }
		public long? CashfromOperatingActivities_12 { get; set; }
		public long? ChangeinFixedAssetsandIntangibles_37 { get; set; }
		public long? DispositionofFixedAssetsIntangibles_13 { get; set; }
		public long? AcquisitionofFixedAssetsIntangibles_14 { get; set; }
		public long? NetChangeinInvestments_36 { get; set; }
		public long? IncreaseinInvestments_15 { get; set; }
		public long? DecreaseinInvestments_16 { get; set; }
		public long? OtherInvestingActivities_17 { get; set; }
		public long? NetCashFromDiscontinuedOperations_investing__18 { get; set; }
		public long? CashfromInvestingActivities_19 { get; set; }
		public long? DividendsPaid_20 { get; set; }
		public long? CashFrom_Repaymentof_Debt_21 { get; set; }
		public long? CashFrom_Repaymentof_ShortTermDebtnet_22 { get; set; }
		public long? CashFrom_Repaymentof_LongTermDebtnet_23 { get; set; }
		public long? RepaymentsofLongTermDebt_24 { get; set; }
		public long? CashFromLongTermDebt_25 { get; set; }
		public long? Cash_Repurchase_ofEquity_26 { get; set; }
		public long? IncreaseinCapitalStock_27 { get; set; }
		public long? DecreaseinCapitalStock_28 { get; set; }
		public long? ChangeinInsuranceReserves_29 { get; set; }
		public long? OtherFinancingActivities_30 { get; set; }
		public long? NetCashFromDiscontinuedOperations_financing__31 { get; set; }
		public long? CashfromFinancingActivities_32 { get; set; }
		public long? NetCashBeforeDiscOperationsandFX_45 { get; set; }
		public long? ChangeinCashfromDiscOperationsandOther_34 { get; set; }
		public long? NetCashBeforeExchangeRates_44 { get; set; }
		public long? EffectofForeignExchangeRates_33 { get; set; }
		public long? NetChangesinCash_35 { get; set; }
	}
}