using NewsFlowAPI.Models;

namespace NewsFlowAPI.Classifier
{
    public class NewsProcessorService
    {
        private readonly RssService _rssService;
        private readonly NewsClassifier _classifier;
        private readonly NewsDbContext _dbContext;

        public NewsProcessorService(RssService rssService, NewsClassifier classifier, NewsDbContext dbContext)
        {
            _rssService = rssService;
            _classifier = classifier;
            _dbContext = dbContext;
        }

        public async Task FetchAndStoreNewsAsync(List<string> rssUrls)
        {
            List<NewsItem> allNews = new();

            foreach (var rssUrl in rssUrls)
            {
                var news = await _rssService.GetNewsAsync(rssUrl);
                allNews.AddRange(news);
            }

            foreach (var newsItem in allNews)
            {
                if (_dbContext.News.Any(n => n.Url == newsItem.Url))
                {
                    continue; // Evităm duplicarea știrilor
                }

                // Clasificare automată
                newsItem.Category = _classifier.PredictCategory(newsItem.Title, newsItem.Content);

                _dbContext.News.Add(newsItem);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
