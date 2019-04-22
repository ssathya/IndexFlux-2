using HandleSimFin.Methods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace StockReporter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class IndexFluxController : ControllerBase
	{
		private readonly ILogger<IndexFluxController> _logger;
		private readonly IDownloadMarketSummary _downloadMarketSummary;

		public IndexFluxController(ILogger<IndexFluxController> logger, IDownloadMarketSummary downloadMarketSummary)
		{
			_logger = logger;
			_downloadMarketSummary = downloadMarketSummary;
		}

		// GET: IndexFlux
		public async Task<ActionResult> Index()
		{
			var indeData = await _downloadMarketSummary.GetIndexValues();
			if (indeData == null)
			{
				return StatusCode(500, "Error while downloading index values");
			}
			return Ok(indeData);
		}

		// POST: IndexFlux/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(IFormCollection collection)
		{
			try
			{
				// TODO: Add insert logic here

				return RedirectToAction(nameof(Index));
			}
			catch
			{
				return StatusCode(500);
			}
		}
	}
}