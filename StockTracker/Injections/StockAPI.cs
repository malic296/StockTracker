﻿using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using StockTracker.Models;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;

namespace StockTracker.Injections
{
    public interface IStockAPI
    {
        Task<List<string>> getExampleTickers();
        Task<List<StockData>> getStockValues(DateTime startingDate, string ticker, string interval);

        Dictionary<string, string> getPerformanceDict(List<StockData> inputData);
        List<int> getIncreaseIntervals(List<StockData> inputData);
        List<int> getDecreaseIntervals(List<StockData> inputData);  
        Task<string> tickerFullName(string ticker);
    }

    public class StockAPI : IStockAPI
    {
        private readonly IConfiguration _configuration;

        public StockAPI(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<List<string>> getExampleTickers()
        {
            string key = _configuration["AppSettings:StockApiKey"];
            string apiUrl = $"https://api.polygon.io/v3/reference/tickers?active=true&limit=30&apiKey={key}";
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

            string key = _configuration["AppSettings:StockApiKey"];
            string apiUrl = $"https://api.polygon.io/v2/aggs/ticker/{ticker}/range/1/{interval}/{startingDateApi}/{currentDateApi}?adjusted=true&sort=asc&limit=30000&apiKey={key}";

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
                        
                        
                    }
                    return stockData;
                }
                catch (Exception ex)
                {
                    return stockData;
                }
            }
            


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

            if(inputData == null) {
                return performanceDict;
            }

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
        public async Task<string> tickerFullName(string ticker)
        {
            string key = _configuration["AppSettings:StockApiKey"];
            string apiUrl = $"https://api.polygon.io/v3/reference/tickers?ticker={ticker}&active=true&apiKey={key}";

            using(HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        dynamic responseObj = JsonConvert.DeserializeObject(responseData);

                        if (responseObj.results != null && responseObj.results.Count > 0)
                        {
                            // Získání názvu akcie z prvního výsledku
                            string tickerResult = responseObj.results[0].name;

                            return tickerResult;
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    return $"{ex.Message}";
                }
                
            }
        }

        public List<int> getIncreaseIntervals(List<StockData> inputData)
        {
            if(inputData.Count == 0)
            {
                List<int> list = new List<int>();
                list.Add(0);
                list.Add(0);
                list.Add(0);
                return list;
            }

            List<Interval> increaseList = new List<Interval>();

            
            DateTime startingDate; 
            DateTime peakDate; 
            DateTime currentDate; 
            double startingValue; 
            double currentValue;
            double peakValue; 
            double difference; 

            StockData first = inputData.First<StockData>();
            startingValue = first.Price;
            startingDate = first.DateTime;
            currentDate = first.DateTime;
            currentValue = first.Price;
            peakValue = first.Price;
            peakDate = first.DateTime;

            List<StockData> growthValues = new List<StockData>();

            difference = 0;

            for (int i = 1; i < inputData.Count; i++)
            {
                if (inputData[i].Price > inputData[i - 1].Price)
                {
                    difference = difference + (inputData[i].Price - startingValue);
                    if (inputData[i].Price > peakValue)
                    {
                        peakValue = inputData[i].Price;
                        peakDate = inputData[i].DateTime;
                    } 
                }
                else
                {                    
                    startingDate = inputData[i].DateTime;
                    startingValue = inputData[i].Price;
                    peakValue = inputData[i].Price;
                    peakDate = inputData[i].DateTime;
                    difference = 0;
                    continue;
                }
                Interval growthInterval = new Interval
                { 
                    peak = peakDate,
                    peakValue = peakValue,
                    start = startingDate,
                    improvement = Math.Round(peakValue - startingValue, 2),
                    startValue = startingValue
                };
                increaseList.Add(growthInterval);
            }

            increaseList = increaseList;

            increaseList = increaseList
            .GroupBy(i => i.start)
            .Select(group =>
            {
                var maxPeakValueInterval = group.OrderByDescending(i => i.peakValue).First();

                return new Interval
                {
                    improvement = Math.Round(maxPeakValueInterval.peakValue - group.First().startValue, 2),
                    peak = maxPeakValueInterval.peak,
                    peakValue = maxPeakValueInterval.peakValue,
                    start = group.Key,
                    startValue = group.First().startValue
                };
            })
            .ToList();


            increaseList = increaseList.OrderByDescending(i => i.improvement) .ToList();

            if (increaseList.Count > 3)
            {
                increaseList.RemoveRange(3, increaseList.Count - 3);
            }

            List<int> finalIndexes = new List<int>();

            for (int i = 0; i < increaseList.Count; i++)
            {
                int index = inputData.FindIndex(interval => interval.DateTime == increaseList[i].start);
                finalIndexes.Add(index);

            }
            
            return finalIndexes;
        }

        public List<int> getDecreaseIntervals(List<StockData> inputData)
        {
            if (inputData.Count == 0)
            {
                List<int> list = new List<int>();
                list.Add(0);
                list.Add(0);
                list.Add(0);
                return list;
            }

            List<Interval> decreaseList = new List<Interval>();


            DateTime startingDate;
            DateTime peakDate;
            DateTime currentDate;
            double startingValue;
            double currentValue;
            double peakValue;
            double difference;

            StockData first = inputData.First<StockData>();
            startingValue = first.Price;
            startingDate = first.DateTime;
            currentDate = first.DateTime;
            currentValue = first.Price;
            peakValue = first.Price;
            peakDate = first.DateTime;

            List<StockData> growthValues = new List<StockData>();

            difference = 0;

            for (int i = 1; i < inputData.Count; i++)
            {
                if (inputData[i].Price < inputData[i - 1].Price)
                {
                    difference = difference + (inputData[i].Price - startingValue);
                    if (inputData[i].Price < peakValue)
                    {
                        peakValue = inputData[i].Price;
                        peakDate = inputData[i].DateTime;
                    }
                }
                else
                {

                    startingDate = inputData[i].DateTime;
                    startingValue = inputData[i].Price;
                    peakValue = inputData[i].Price;
                    peakDate = inputData[i].DateTime;
                    difference = 0;
                    continue;
                }
                Interval growthInterval = new Interval
                {
                    peak = peakDate,
                    peakValue = peakValue,
                    start = startingDate,
                    improvement = Math.Round(peakValue - startingValue, 2),
                    startValue = startingValue
                };
                decreaseList.Add(growthInterval);
            }

            decreaseList = decreaseList
            .GroupBy(i => i.start)
            .Select(group =>
            {
                var maxPeakValueInterval = group.OrderByDescending(i => i.peakValue).First();

                return new Interval
                {
                    improvement = Math.Round(maxPeakValueInterval.peakValue - group.First().startValue, 2),
                    peak = maxPeakValueInterval.peak,
                    peakValue = maxPeakValueInterval.peakValue,
                    start = group.Key,
                    startValue = group.First().startValue
                };
            })
            .ToList();


            decreaseList = decreaseList.OrderBy(i => i.improvement).ToList();


            if (decreaseList.Count > 3)
            {
                decreaseList.RemoveRange(3, decreaseList.Count - 3);
            }

            List<int> finalIndexes = new List<int>();

            for (int i = 0; i < decreaseList.Count; i++)
            {
                int index = inputData.FindIndex(interval => interval.DateTime == decreaseList[i].start);
                finalIndexes.Add(index);

            }

            return finalIndexes;
        }


    }
  
}
