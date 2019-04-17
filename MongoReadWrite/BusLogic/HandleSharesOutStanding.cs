using AutoMapper;
using HandleSimFin.Methods;
using Microsoft.Extensions.Logging;
using Models;
using MongoReadWrite.Extensions;
using MongoReadWrite.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MongoReadWrite.BusLogic
{
	public class HandleSharesOutStanding
	{

		#region Private Fields

		private const int validityOfRecord = 1;
		private readonly IDBConnectionHandler<OutstandingSharesMd> _dBConnection;
		private readonly IDownloadOutstandingShares _dos;
		private readonly ILogger<HandleSharesOutStanding> _logger;

		#endregion Private Fields


		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="HandleSharesOutStanding"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="dBConnection">The d b connection.</param>
		/// <param name="dos">The dos.</param>
		public HandleSharesOutStanding(ILogger<HandleSharesOutStanding> logger,
			IDBConnectionHandler<OutstandingSharesMd> dBConnection,
			IDownloadOutstandingShares dos)
		{
			_dBConnection = dBConnection;
			_dBConnection.ConnectToDatabase("OutstandingShares");
			_logger = logger;
			_dos = dos;
		}

		#endregion Public Constructors


		#region Public Methods

		/// <summary>
		/// Gets the out standing shares.
		/// </summary>
		/// <param name="simId">The sim identifier.</param>
		/// <returns></returns>
		public async Task<OutstandingShares> GetOutStandingShares(string simId)
		{
			var existingRecord = _dBConnection.Get(x => x.SimId == simId).FirstOrDefault();
			if (existingRecord != null &&
				existingRecord.LastUpdateDate != null &&
				((TimeSpan)(DateTime.Now - existingRecord.LastUpdateDate)).Days < validityOfRecord)
			{
				return Mapper.Map<OutstandingSharesMd, OutstandingShares>(existingRecord);
			}
			var externalData = await _dos.ObtainAggregatedList(simId);
			if (externalData == null)
			{
				_logger.LogError($"Could not download outstanding shares for SimId:{simId}");
				return null;
			}
			string id = "";
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

		#endregion Public Methods
	}
}