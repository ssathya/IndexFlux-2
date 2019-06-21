using Models;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
	public interface IDownloadStockQuote
	{
		Task<MarketQuote> DownloadQuote(string ticker);
	}
}