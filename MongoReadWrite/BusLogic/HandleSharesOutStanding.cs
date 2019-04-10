using Microsoft.Extensions.Logging;
using Models;
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

		public HandleSharesOutStanding(ILogger<HandleSharesOutStanding> logger,
			IDBConnectionHandler<OutstandingSharesMd> dBConnection)
		{
			_dBConnection = dBConnection;
			_logger = logger;
		}
    }
}
