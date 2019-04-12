using AutoMapper;
using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoReadWrite.BusLogic
{
    public class HandleSharesOutStanding
    {
		private readonly IDBConnectionHandler<OutstandingSharesMd> _dBConnection;
		private readonly ILogger<HandleSharesOutStanding> _logger;
		private readonly IDownloadOutstandingShares _dos;

		public HandleSharesOutStanding(ILogger<HandleSharesOutStanding> logger,
			IDBConnectionHandler<OutstandingSharesMd> dBConnection,
			IDownloadOutstandingShares dos)
		{
			_dBConnection = dBConnection;
			_dBConnection.ConnectToDatabase("OutstandingShares");
			_logger = logger;
			_dos = dos;
		}
		public async Task<OutstandingShares> GetOutStandingShares(string simId)
		{
			var existingRecord = _dBConnection.Get(x => x.SimId == simId).FirstOrDefault();
			if (existingRecord != null && 
				existingRecord.LastUpdateDate != null && 
				((TimeSpan)(DateTime.Now - existingRecord.LastUpdateDate)).Days < 30)
			{
				return Mapper.Map<OutstandingSharesMd, OutstandingShares>(existingRecord);
			}
			var externalData = await _dos.ObtainAggregatedList(simId);
			if (externalData == null)
			{
				return null;
			}
			string id ="";
			if (existingRecord != null)
			{
				id = existingRecord.Id;
			}
			existingRecord = Mapper.Map<OutstandingShares, OutstandingSharesMd>(externalData);
			existingRecord.LastUpdateDate = DateTime.Now;
			if (!id.IsNullOrWhiteSpace())
			{
				existingRecord.Id = id;
				await _dBConnection.Update(id, existingRecord);
			}
			else
			{
				await _dBConnection.Create(existingRecord);
			}			
			return externalData;
		}
    }
}
