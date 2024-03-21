using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StockTracker.Injections;
using System.Formats.Asn1;
using StockTracker.Models;
using System.Text;
using System.Runtime.CompilerServices;

namespace StockTracker.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IStockAPI _stockAPI;
        private readonly INewsAPI _newsAPI;

        public IndexModel(ILogger<IndexModel> logger, IStockAPI stockClient, INewsAPI newsAPI)
        {
            _logger = logger;
            _stockAPI = stockClient;
            _newsAPI = newsAPI;
        }

        public async Task OnGet()
        {
            tickers = await _stockAPI.getExampleTickers();
            stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddDays(-3), "TSLA", "minute");
            performanceData = _stockAPI.getPerformanceDict(stockDataList);
            selectedTickerFullName = await _stockAPI.tickerFullName(selectedTicker);
            HttpContext.Session.SetString("timespan", "1D");
            improvement = (performanceData["improvement"] == "1") ? "text-success" : "text-danger";

            increaseIntervals = _stockAPI.getIncreaseIntervals(stockDataList);
            decreaseIntervals = _stockAPI.getDecreaseIntervals(stockDataList);

            generalNews = await _newsAPI.GeneralNews(selectedTicker, DateTime.Now);
        }

        public async Task<IActionResult> OnPostTickerSelection()
        {
            tickers = await _stockAPI.getExampleTickers();
            selectedTickerFullName = await _stockAPI.tickerFullName(selectedTicker);

            HttpContext.Session.SetString("ticker", selectedTicker);

            if (HttpContext.Session.TryGetValue("timespan", out byte[] tickerBytes))
            {

                Timespan = HttpContext.Session.GetString("timespan");
            }
            else
            {
                Timespan = "1D";
            }

            DateTime start = DateTime.Now;
            string timespan = "minute";
            switch (Timespan)
            {
                case "1D":
                    start = DateTime.Now.AddDays(-3);
                    timespan = "minute";
                    day = "btn-info"; week = ""; year = ""; month = "";
                    break;
                case "1W":
                    start = DateTime.Now.AddDays(-7);
                    timespan = "hour";
                    day = ""; week = "btn-info"; year = ""; month = "";
                    break;
                case "1M":
                    start = DateTime.Now.AddMonths(-1);
                    timespan = "hour";
                    day = ""; week = ""; year = ""; month = "btn-info";
                    break;
                case "1Y":
                    start = DateTime.Now.AddYears(-1);
                    timespan = "week";
                    day = ""; week = ""; year = "btn-info"; month = "";
                    break;
            }
            
            
            stockDataList = await _stockAPI.getStockValues(start, selectedTicker, timespan);
            performanceData = _stockAPI.getPerformanceDict(stockDataList);
            improvement = (performanceData["improvement"] == "1") ? "text-success" : "text-danger";
            increaseIntervals = _stockAPI.getIncreaseIntervals(stockDataList);
            decreaseIntervals = _stockAPI.getDecreaseIntervals(stockDataList);
            generalNews = await _newsAPI.GeneralNews(selectedTicker, DateTime.Now);
            return Page();
        }

        public async Task<IActionResult> OnPostTimespan()
        {
            tickers = await _stockAPI.getExampleTickers();
 
            if (HttpContext.Session.TryGetValue("ticker", out byte[] tickerBytes))
            {
               
                selectedTicker = HttpContext.Session.GetString("ticker");
            }
            else
            {
                selectedTicker = "TSLA";
            }

            selectedTickerFullName = await _stockAPI.tickerFullName(selectedTicker);

            string ticker = selectedTicker;
            string timespan = Timespan;
            switch (timespan)
            {
                case "1D":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddDays(-3), ticker, "minute");
                    HttpContext.Session.SetString("timespan", "1D");
                    day = "btn-info"; week = ""; year = ""; month = "";
                    break;
                case "1W":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddDays(-7), ticker, "hour");
                    HttpContext.Session.SetString("timespan", "1W");
                    day = ""; week = "btn-info";  year = ""; month = "";
                    break;
                case "1M":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddMonths(-1), ticker, "hour");
                    HttpContext.Session.SetString("timespan", "1M");
                    day = ""; week = ""; year = ""; month = "btn-info";
                    break;
                case "1Y":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddYears(-1), ticker, "week");
                    HttpContext.Session.SetString("timespan", "1Y");
                    day = ""; week = ""; year = "btn-info"; month = "";
                    break;
            }

            performanceData = _stockAPI.getPerformanceDict(stockDataList);
            improvement = (performanceData["improvement"] == "1") ? "text-success" : "text-danger";
            increaseIntervals = _stockAPI.getIncreaseIntervals(stockDataList);
            decreaseIntervals = _stockAPI.getDecreaseIntervals(stockDataList);
            generalNews = await _newsAPI.GeneralNews(selectedTicker, DateTime.Now);
            return Page();
        }

        [BindProperty]
        public string selectedTicker { get; set; } = "TSLA";

        [BindProperty]
        public string Timespan { get; set; }


        public List<int> increaseIntervals { get; set; }
        public List<int> decreaseIntervals { get; set; }
        public string selectedTickerFullName { get; set; } = "Tesla, Inc. Common Stock";

        public List<string> tickers {  get; set; }
        public List <StockData> stockDataList { get; set; }
        public Dictionary<string, string> performanceData { get; set; }

        public List<ArticleInfo> generalNews { get; set; } = new List<ArticleInfo> {
        new ArticleInfo {
            Title = "Something went wrong",
            Description = "",
            Author = "",
            publishedAt = DateTime.Now,
            Url = ""
        }};

        public string improvement { get; set; } = "";

        public string day { get; set; } = "btn-info";
        public string year { get; set; }
        public string month { get; set; }
        public string week { get; set; }
}
}