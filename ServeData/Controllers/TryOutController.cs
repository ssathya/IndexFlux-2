using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Dialogflow.v2.Data;
using System.Text;

namespace ServeData.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TryOutController : Controller
	{
		private static readonly JsonParser jsonParser =
		new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

		// GET: api/TryOut
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET: api/TryOut/5
		[HttpGet("{id}", Name = "Get")]
		public string Get(int id)
		{
			return "value";
		}

		// POST: api/TryOut
		[HttpPost]
		public IActionResult Post([FromBody] GoogleCloudDialogflowV2WebhookRequest value)
		{
			
			string intendDisplayName = value.QueryResult.Intent.DisplayName;
			StringBuilder presidentName = new StringBuilder();
			var parameters = value.QueryResult.Parameters;
			foreach (var key in parameters.Keys)
			{
				presidentName.Append($"Parameter {key} value {parameters[key]}\n");
			}
			var response = new WebhookResponse
			{
				FulfillmentText = $"Called from {intendDisplayName} with parameter {presidentName}",
				Source = "C# application"
			};
			var responseString = response.ToString();
			return new ContentResult
			{
				Content = responseString,
				ContentType = "application/json",
				StatusCode = 200
			};
		}

		

		// PUT: api/TryOut/5
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