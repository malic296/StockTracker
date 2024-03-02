using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockTracker.Injections;
using System.Formats.Asn1;

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

            string ticker = selectedTicker;
            return Page();
        }

        //vars
        [BindProperty]
        public string selectedTicker { get; set; } = "TSLA";



        public List<string> tickers {  get; set; }
    }
}