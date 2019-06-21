using Google.Apis.Dialogflow.v2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServeData.MessageProcessors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServeData.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TryOutController : Controller
	{
		#region Private Fields

		private readonly ILogger<TryOutController> _log;
		private readonly ProcessMessages _processMessages;

		#endregion Private Fields

		#region Public Constructors

		public TryOutController(ILogger<TryOutController> log, ProcessMessages processMessages)
		{
			_log = log;
			_processMessages = processMessages;
		}

		#endregion Public Constructors

		#region Public Methods

		// DELETE: api/ApiWithActions/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}

		// GET: api/TryOut
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// POST: api/TryOut
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] GoogleCloudDialogflowV2WebhookRequest value)
		{
			return await _processMessages.ProcessValuesFromIntents(value);
		}

		// PUT: api/TryOut/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		#endregion Public Methods
	}
}