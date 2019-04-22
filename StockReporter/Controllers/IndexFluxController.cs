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
		private readonly IDownloadStockQuote _downloadStockQuote;

		public IndexFluxController(ILogger<IndexFluxController> logger, 
			IDownloadMarketSummary downloadMarketSummary,
			IDownloadStockQuote downloadStockQuote)
		{
			_logger = logger;
			_downloadMarketSummary = downloadMarketSummary;
			_downloadStockQuote = downloadStockQuote;
		}

		// GET: IndexFlux
		public async Task<ActionResult> Index()
		{
			var indeData = await _downloadMarketSummary.GetIndexValues();
			if (indeData == null)
			{
				_logger.LogError("Error while downloading index values");
				return StatusCode(500, "Error while downloading index values");
			}
			var quote = await _downloadStockQuote.DownloadQuote("BBY");
			if (quote == null)
			{
				_logger.LogError("Error while downloading Quotes");
				return StatusCode(500, "Error while downloading Quotes");
			}
			return Ok(quote);
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