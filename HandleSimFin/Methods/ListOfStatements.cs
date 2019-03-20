using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandleSimFin.Methods
{
    public class ListOfStatements
    {
		private readonly ILogger<ListOfStatements> _logger;

		public ListOfStatements(ILogger<ListOfStatements> logger)
		{
			_logger = logger;
		}

    }
}
