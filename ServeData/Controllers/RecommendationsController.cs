using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataProvider.Extensions;
using Google.Apis.Dialogflow.v2.Data;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServeData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
       
        // POST: api/Recommendations
        [HttpPost]
        public IActionResult Post([FromBody] GoogleCloudDialogflowV2WebhookRequest value)
        {
			string intendDisplayName = value.QueryResult.Intent.DisplayName;
			var returnValue = new WebhookResponse
			{
				FulfillmentText = Utilities.ErrorReturnMsg() + Utilities.EndOfCurrentRequest()
			};
			return new ContentResult
			{
				Content = returnValue.ToString(),
				ContentType = "application/json",
				StatusCode = 200
			};
		}

        // PUT: api/Recommendations/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string value)
        {
			return Ok();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
