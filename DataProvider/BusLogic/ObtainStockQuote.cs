using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.BusLogic
{
    public class ObtainStockQuote
    {
		private const string baseUrl = @"https://api.iextrading.com/1.0/stock/symbol/batch?types=quote";
		private readonly ILogger<ObtainStockQuote> _log;

		public ObtainStockQuote(ILogger<ObtainStockQuote> log)
		{
			_log = log;
		}
    }
}
