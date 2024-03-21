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
                dayParameter = dayParameter.AddDays(-3);
                datetimeTo = DateTime.Now;
            }
            else
            {
                datetimeTo = dayParameter.AddDays(3);
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
    }
}
