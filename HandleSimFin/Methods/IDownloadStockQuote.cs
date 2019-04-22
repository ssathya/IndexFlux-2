using System.Threading.Tasks;
using Models;

namespace HandleSimFin.Methods
{
	public interface IDownloadStockQuote
	{
		Task<MarketQuote> DownloadQuote(string ticker);
	}
}