using Newtonsoft.Json;
using StockTracker.Models;



namespace StockTracker.Injections
{
    public interface INewsAPI
    {
        Task<List<Article>> GeneralNews(string stock);

    }
    public class NewsClassAPI : INewsAPI
    {
        //TODO: Rewrite this method to work with NewsAPI library
        public async Task<List<Article>> GeneralNews(string stock)
        {
            List<Article> news = new List<Article>();

            DateTime currentDate = DateTime.Now.AddDays(-1);

            string currentDateString = currentDate.ToString("yyyy-MM-dd");

            string url = $"https://newsapi.org/v2/everything?q={stock}+stock&from={currentDateString}&to={currentDateString}&sortBy=relevancy&language=en&apiKey=fb2653f7f62843edbf11cff73a200699";

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<NewsApiResponse>(jsonResponse);

                    if (result != null)
                    {
                        foreach (var article in result.Articles)
                        {
                            news.Add(new Article
                            {
                                Title = article.Title,
                                Description = article.Description,
                                Author = article.Author,
                                Url = article.Url,
                                publishedAt = article.publishedAt
                            });
                        }
                    }
                    else
                    {
                        news.Add(new Article
                        {
                            Title = "No general articles about this stock",
                        });
                    }


                }
                else
                {
                    news.Add(new Article
                    {
                        Title = "Something went wrong"
                    });
                }
            }

            return news;
        }
    }
    public class NewsApiResponse
    {
        public List<Article> Articles { get; set; }
    }
}
