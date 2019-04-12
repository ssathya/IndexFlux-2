using System.Threading.Tasks;
using Models;

namespace HandleSimFin.Methods
{
	public interface IDownloadOutstandingShares
	{
		Task<OutstandingShares> ObtainAggregatedList(string simId);
	}
}