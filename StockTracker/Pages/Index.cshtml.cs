using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockTracker.Injections;
using System.Formats.Asn1;
using StockTracker.Models;

namespace StockTracker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IStockAPI _stockAPI;

        public IndexModel(ILogger<IndexModel> logger, IStockAPI stockClient)
        {
            _logger = logger;
            _stockAPI = stockClient;
        }

        public async Task OnGet()
        {
            tickers = await _stockAPI.getExampleTickers();
        }

        public async Task<IActionResult> OnPost()
        {
            tickers = await _stockAPI.getExampleTickers();
            List <StockData> list = await _stockAPI.getStockValues(DateTime.Now.AddMonths(-1), "TSLA", "hour");
            string ticker = selectedTicker;
            return Page();
        }
        
        [BindProperty]
        public string selectedTicker { get; set; } = "TSLA";



        public List<string> tickers {  get; set; }
    }
}