using Newtonsoft.Json;
using StockTracker.Models;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using System;



namespace StockTracker.Injections
{



    public interface INewsAPI
    {
        Task<List<ArticleInfo>> GeneralNews(string stock, DateTime dayParameter);
        Task<List<ArticleInfo>> IntervalNews(string stock, List<int> indexes, List<StockData> stockData);

    }
    public class NewsClassAPI : INewsAPI
    {
        private readonly IConfiguration _configuration;
        public NewsClassAPI(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<ArticleInfo>> GeneralNews(string stock, DateTime dayParameter)
        {
            List<ArticleInfo> news = new List<ArticleInfo>();
            DateTime datetimeTo;
            if (dayParameter.Date == DateTime.Today)
            {
                dayParameter = dayParameter.AddDays(-2);
                datetimeTo = DateTime.Now;
            }
            else
            {
                datetimeTo = dayParameter.AddDays(2);
            }

            string key = _configuration["AppSettings:NewsApiKey"];

            var newsApiClient = new NewsApiClient(key);
            var articlesResponse = newsApiClient.GetEverything(new EverythingRequest
            {
                Q = stock+"+stock",
                SortBy = SortBys.Relevancy,
                Language = Languages.EN,
                From = dayParameter,
                To = datetimeTo
                
            });

            if(articlesResponse.TotalResults == 0) {
                news.Add(new ArticleInfo
                {
                    Title = "No Articles were found"
                });
                return news;
            }

            if (articlesResponse.Status == Statuses.Ok)
            {
                foreach (var article in articlesResponse.Articles)
                {
                    news.Add(new ArticleInfo
                    {
                        Title = article.Title,
                        Description = article.Description,
                        Author = article.Author,
                        Url = article.Url,
                        publishedAt = (DateTime)article.PublishedAt
                    });
                }
            }
            else
            {
                news.Add(new ArticleInfo
                {
                    Title = "Something went wrong"
                });

            }

            return news;
        }

        public async Task<List<ArticleInfo>> IntervalNews(string stock, List<int> indexes, List<StockData> stockData)
        {
            List<ArticleInfo> news = new List<ArticleInfo>();

            List<DateTime> Dates = new List<DateTime>();
            List<int> indexesToRemove = new List<int>();
            foreach(var index in indexes)
            {
                if (Dates.Contains(stockData[index].DateTime.Date)){
                    indexesToRemove.Add(index);
                }
                else
                {
                    Dates.Add(stockData[index].DateTime.Date);
                }
                 
            }
            foreach(var indexToRemove in indexesToRemove)
            {
                indexes.Remove(indexToRemove);
            }
            List<ArticleInfo> eachIndexArticles = new List<ArticleInfo>();
            foreach (var index in indexes)
            {
                eachIndexArticles = await GeneralNews(stock, stockData[index].DateTime);
                if(eachIndexArticles.Count > 1)
                {
                    foreach (ArticleInfo article in eachIndexArticles)
                    {
                        news.Add(article);
                    }
                }
                
            }

            return news;
        }
    }
}
