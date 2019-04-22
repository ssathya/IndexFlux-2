using System.Threading.Tasks;
using Models;

namespace HandleSimFin.Methods
{
	public interface IDownloadMarketSummary
	{
		Task<IndexData> GetIndexValues();
	}
}