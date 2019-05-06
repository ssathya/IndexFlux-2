using Amazon;
using HandleSimFin.Helpers;
using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace StockReporter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		private readonly List<EntityKeys> _keysToServices;

		public ValuesController()
		{
			var readS3Objs = new ReadS3Objects(@"talk2control-1", RegionEndpoint.USEast1);

			_keysToServices = JsonConvert
				.DeserializeObject<List<EntityKeys>>(readS3Objs
					.GetDataFromS3("Random.txt")
				.Result);
		}

		// GET api/values
		/// <summary>
		/// Gets all values as  strings.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			var displayString = new List<string>();
			foreach (var services in _keysToServices)
			{
				displayString.Add(services.Entity);
				displayString.Add(services.Key);
			}
			displayString.Clear();
			return displayString.ToArray();
		}

		// GET api/values/5
		/// <summary>
		/// Gets the values associated to the specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}