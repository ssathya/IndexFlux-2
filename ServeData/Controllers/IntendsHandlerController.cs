using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Dialogflow.v2.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestSharp;
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
			var baseURL = Request.Host.ToString();
			var keyUsedToAccess = Request.Headers["key"].ToString();
			
			string intendDisplayName = value.QueryResult.Intent.DisplayName;
			if (intendDisplayName.Equals("recommend") || intendDisplayName.Equals("fundamentals"))
			{
				var client = new RestClient("https://" + baseURL  );
				var request = new RestRequest(BuildActionMethod(intendDisplayName), Method.POST);
				request.AddHeader("key", keyUsedToAccess);
				request.AddJsonBody(value);
				var response = client.Execute(request);	
				if (response.IsSuccessful)
				{
					var returnMsg = response.Content;
					return new ContentResult
					{
						Content = returnMsg,
						ContentType = "application/json",
						StatusCode = 200
					};
				}
				
			}
			return await _processMessages.ProcessValuesFromIntents(value);
		}

		private string BuildActionMethod(string intendDisplayName)
		{
			string returnString = "";
			switch (intendDisplayName)
			{
				case "fundamentals":
					returnString = "/api/Fundamentals";
					break;
				case "recommend":
					returnString = "/api/Recommendations";
					break;
				default:
					break;
			}
			return returnString;
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
