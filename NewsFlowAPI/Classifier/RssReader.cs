using HtmlAgilityPack;
using NewsFlowAPI.Models;
using System.ServiceModel.Syndication;
using System.Xml;

namespace NewsFlowAPI.Classifier
{
    public class RssReader
    {
        public async Task<List<NewsItem>> GetNewsFromFeeds(string[] rssUrls)
        {
            var newsList = new List<NewsItem>();

            using var httpClient = new HttpClient();

            foreach (var url in rssUrls)
            {
                try
                {
                    var response = await httpClient.GetStringAsync(url);

                    using var reader = XmlReader.Create(new System.IO.StringReader(response));
                    var feed = SyndicationFeed.Load(reader);
                    

                    foreach (var item in feed.Items)
                    {
                        var CleanContent = CleanHtml(item.Summary?.Text ?? "");

                        newsList.Add(new NewsItem
                        {
                            Title = item.Title.Text,
                            Content = item.Summary?.Text ?? "",
                            Url = CleanContent,
                            PublishedAt = item.PublishDate.UtcDateTime,
                            Source =item.SourceFeed.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la preluarea RSS {url}: {ex.Message}");
                }
            }

            return newsList;
        }

        private static string CleanHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText.Trim();
        }
    }
}
