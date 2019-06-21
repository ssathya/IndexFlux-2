using Models;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
	public interface IDownloadMarketSummary
	{
		Task<QuotesFromWorldTrading> GetIndexValues();
	}
}