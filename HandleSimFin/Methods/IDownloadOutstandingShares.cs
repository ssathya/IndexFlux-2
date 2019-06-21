using Models;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
	public interface IDownloadOutstandingShares
	{
		Task<OutstandingShares> ObtainAggregatedList(string simId);
	}
}