﻿using Microsoft.AspNetCore.Mvc;
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
            //stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddDays(-2), "TSLA", "minute");
            stockDataList = new List<StockData>();
        }

        public async Task<IActionResult> OnPost()
        {
            tickers = await _stockAPI.getExampleTickers();
            stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddDays(-2), selectedTicker, "minute");
            return Page();
        }

        public async Task<IActionResult> OnPostTimespan()
        {
            tickers = await _stockAPI.getExampleTickers();
            string ticker = selectedTicker;
            string timespan = Timespan;
            switch (timespan)
            {
                case "1D":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddDays(-2), ticker, "minute");
                    break;
                case "1W":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddDays(-7), ticker, "hour");
                    break;
                case "1M":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddMonths(-1), ticker, "hour");
                    break;
                case "1Y":
                    stockDataList = await _stockAPI.getStockValues(DateTime.Now.AddYears(-1), ticker, "week");
                    break;
            }
            
            return Page();
        }
        
        [BindProperty]
        public string selectedTicker { get; set; } = "TSLA";

        [BindProperty]
        public string Timespan { get; set; }

        public List<string> tickers {  get; set; }
        public List <StockData> stockDataList { get; set; }
    }
}