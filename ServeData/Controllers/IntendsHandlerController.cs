using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Dialogflow.v2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServeData.MessageProcessors;

namespace ServeData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntendsHandlerController : ControllerBase
    {
		private readonly ILogger<IntendsHandlerController> _log;

		private  ProcessMessages _processMessages { get; }

		public IntendsHandlerController(ILogger<IntendsHandlerController> log, ProcessMessages processMessages)
		{
			_log = log;
			_processMessages = processMessages;
		}
        // GET: api/IntendsHandler
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/IntendsHandler/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/IntendsHandler
        [HttpPost]
		public async Task<IActionResult> Post([FromBody] GoogleCloudDialogflowV2WebhookRequest value)
		{
			string intendDisplayName = value.QueryResult.Intent.DisplayName;
			if (intendDisplayName.Equals("RandomRecommendation"))
			{
				RedirectToAction()
			}
			return await _processMessages.ProcessValuesFromIntents(value);
		}

		// PUT: api/IntendsHandler/5
		[HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
