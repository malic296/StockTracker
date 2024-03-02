using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace StockTracker.Injections
{
    public interface IStockAPI
    {
        Task<List<string>> getExampleTickers();
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

    }
}
