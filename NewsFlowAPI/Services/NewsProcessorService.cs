using AngleSharp;
using Google.Apis.Auth.OAuth2;
using NewsFlowAPI.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NewsFlowAPI.Classifier;

namespace NewsFlowAPI.Services
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
                    continue;


                if (string.IsNullOrEmpty(newsItem.Category))
                    newsItem.Category = _classifier.PredictCategory(newsItem.Title, newsItem.Content);

                _dbContext.News.Add(newsItem);
                await _dbContext.SaveChangesAsync();

                var userIds = await _dbContext.Subscriptions
                    .Where(s => s.Source == newsItem.Source)
                    .Select(s => s.userId)
                    .ToListAsync();

                if (newsItem.Title.Contains("breaking", StringComparison.OrdinalIgnoreCase))
                {
                    var allTokens = await _dbContext.UserDevices
                        .Select(d => d.DeviceToken)
                        .Distinct()
                        .ToListAsync();

                    foreach (var token in allTokens)
                    {
                        await SendPushNotificationV1(token, $"🚨 {newsItem.Title}", newsItem.Content, newsItem.Url, newsItem.Source, newsItem.NewsId);
                    }
                }

                if (userIds.Any())
                {
                    var tokens = await _dbContext.UserDevices
                        .Where(d => userIds.Contains(d.UserId))
                        .Select(d => d.DeviceToken)
                        .Distinct()
                        .ToListAsync();

                    foreach (var token in tokens)
                    {
                        await SendPushNotificationV1(token, newsItem.Title, newsItem.Content, newsItem.Url, newsItem.Source, newsItem.NewsId);
                    }
                }

                
            }


        }

        public static string CleanHtml(string html)
        {
            string decoded = System.Net.WebUtility.HtmlDecode(html);
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = context.OpenAsync(req => req.Content(decoded)).Result;
            string text = document.Body?.TextContent?.Trim() ?? "";
            return text.Normalize(NormalizationForm.FormC);
        }


        private async Task SendPushNotificationV1(string deviceToken, string title, string body, string Url ,string Source, int NewsId)
        {
            string projectId = "newsflownotifications";
            string jsonPath = "newsflownotifications-firebase-adminsdk-fbsvc-bac1a39415.json";

            var message = new
            {
                message = new
                {
                    token = deviceToken,
                    notification = new { title, body },
                    data = new { url = Url, source = Source, newsId =  NewsId.ToString()}
                }
            };

            var credential = GoogleCredential
                .FromFile(jsonPath)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
                $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send",
                content);

            var result = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"FCM response: {result}");
        }
    }


}

