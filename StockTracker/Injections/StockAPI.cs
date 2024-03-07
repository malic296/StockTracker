using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using StockTracker.Models;

namespace StockTracker.Injections
{
    public interface IStockAPI
    {
        Task<List<string>> getExampleTickers();
        Task<List<StockData>> getStockValues(DateTime startingDate, string ticker, string interval);

        Dictionary<string, string> getPerformanceDict(List<StockData> inputData);
    }

    public class StockAPI : IStockAPI
    {
        public async Task<List<string>> getExampleTickers()
        {
            string apiUrl = "https://api.polygon.io/v3/reference/tickers?active=true&limit=30&apiKey=MompKnilMmyrB5zVZtk7u9x6rx8_wV8X";
            using(HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        var jsonResponse = JObject.Parse(responseData);
                        var results = jsonResponse["results"];

                        List<string> tickers = results.Select(result => result["ticker"].ToString()).ToList();

                        return tickers;
                    }
                }
                catch (Exception ex)
                {
                    List<string> err = new List<string>();
                    err.Add(ex.Message);
                    return err;
                }
            }
            return new List<string>();
        }

        public async Task<List<StockData>> getStockValues(DateTime startingDate, string ticker, string interval)
        {
            DateTime currentDate = DateTime.Now;

            string startingDateApi = startingDate.ToString("yyyy-MM-dd");
            string currentDateApi = currentDate.ToString("yyyy-MM-dd");

            string apiUrl = $"https://api.polygon.io/v2/aggs/ticker/{ticker}/range/1/{interval}/{startingDateApi}/{currentDateApi}?adjusted=true&sort=asc&limit=30000&apiKey=MompKnilMmyrB5zVZtk7u9x6rx8_wV8X";

            List<StockData> stockData = new List<StockData>();

            using(HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        var jsonResponse = JObject.Parse(responseData);
                        var results = jsonResponse["results"];

                        string tickerValue = jsonResponse["ticker"]?.ToString();


                        if(results != null) { 
                            foreach (var result in results)
                            {
                                StockData stockRecord = new StockData()
                                {
                                    Ticker = tickerValue,
                                    Price = result["vw"].ToObject<double>(),
                                    DateTime = startingDate
                                };

                                stockData.Add(stockRecord);

                                if (interval == "hour")
                                {
                                    startingDate = startingDate.AddHours(1);
                                }
                                else if(interval == "day")
                                {
                                    startingDate = startingDate.AddDays(1);

                                }
                                else if (interval == "minute")
                                {
                                    startingDate = startingDate.AddMinutes(1);

                                }
                                else if (interval == "week")
                                {
                                    startingDate = startingDate.AddDays(7);

                                }
                                else
                                {
                                    startingDate = startingDate.AddDays(7);
                                }
                            }
                        }
                        return stockData;
                        
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
            return new List<StockData>();


        }

        public Dictionary<string, string> getPerformanceDict(List<StockData> inputData)
        {
            Dictionary<string, string> performanceDict = new Dictionary<string, string>();

            double startingPrice = 0;
            double currentPrice = 0;
            double priceDiff = 0;
            double improvePercentage = 0;
            DateTime startingDate = DateTime.Now.AddDays(-1);
            DateTime currentDate = DateTime.Now;
            string improvement = "0";
            double low = 0;
            double peak = 0;


            if (inputData.Count > 0)
            {
                low = inputData.First().Price;
                peak = inputData.First().Price;
                foreach (var item in inputData)
                {
                    if (item.Price > peak)
                    {
                        peak = item.Price;
                    }
                    else if (item.Price < low)
                    {
                        low = item.Price;
                    }
                }
                low = Math.Round(low, 2);
                peak = Math.Round(peak, 2);

                startingPrice = inputData.First().Price;
                startingPrice = Math.Round(startingPrice, 2);
                currentPrice = inputData.Last().Price;
                currentPrice = Math.Round(currentPrice, 2);
                priceDiff = currentPrice - startingPrice;
                priceDiff = Math.Round(priceDiff, 2);
                improvePercentage = (startingPrice > 0) ? (priceDiff / startingPrice) * 100 : 0;
                improvePercentage = Math.Round(improvePercentage, 2);
                startingDate = inputData.First().DateTime;
                currentDate = inputData.Last().DateTime;
                improvement = (startingPrice < currentPrice) ? "1" : "0";
            }
            

            performanceDict.Add("Starting Price", startingPrice.ToString());
            performanceDict.Add("Current Price", currentPrice.ToString());
            performanceDict.Add("Price Difference", priceDiff.ToString());
            performanceDict.Add("Improvement Percentage", improvePercentage.ToString() + "%");
            performanceDict.Add("Starting Date", startingDate.ToString("yyyy-MM-dd"));
            performanceDict.Add("Current Date", currentDate.ToString("yyyy-MM-dd"));
            performanceDict.Add("improvement", improvement);
            performanceDict.Add("Lowest Price", low.ToString());
            performanceDict.Add("Highest Price", peak.ToString());


            return performanceDict;
        }

    }

    

   
}
