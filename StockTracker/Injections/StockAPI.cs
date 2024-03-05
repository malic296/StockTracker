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
                                    startingDate = startingDate.AddMinutes(20);
                                }
                                else if(interval == "day")
                                {
                                    startingDate = startingDate.AddHours(2);

                                }
                                else if (interval == "minute")
                                {
                                    startingDate = startingDate.AddMinutes(3);

                                }
                                else if (interval == "week")
                                {
                                    startingDate = startingDate.AddDays(1);

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

    }

   
}
