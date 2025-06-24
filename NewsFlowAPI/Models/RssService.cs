using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AngleSharp;
using HtmlAgilityPack;
using NewsFlowAPI.Classifier;
namespace NewsFlowAPI.Models
{
    public class RssService
    {
        public async Task<List<NewsItem>> GetNewsAsync(string rssUrl)
        {
            List<NewsItem> newsList = new();
            try
            {
                string forcedCategory = RssSources.FeedCategoryMap
                    .FirstOrDefault(kvp => rssUrl.Equals(kvp.Key)).Value;

                NewsClassifier classifier = null;
                if (string.IsNullOrEmpty(forcedCategory))
                {
                    classifier = new NewsClassifier();

                }

                using var reader = XmlReader.Create(rssUrl);
                var feed = SyndicationFeed.Load(reader);

                foreach (var item in feed.Items)
                {
                    string imageUrl = "";

                    var enclosure = item.Links.FirstOrDefault(l => l.RelationshipType == "enclosure");
                    if (enclosure != null)
                    {
                        imageUrl = enclosure.Uri.ToString();
                    }

                    if (string.IsNullOrEmpty(imageUrl) && item.ElementExtensions.Any())
                    {
                        var thumbElement = item.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail" || e.OuterName == "enclosure");
                        if (thumbElement != null)
                        {
                            imageUrl = thumbElement.GetObject<XElement>().Attribute("url")?.Value ?? "";
                        }
                    }

                    string rawDescription = item.Summary?.Text?.Trim() ?? "";
                    string cleanDescription = CleanHtml(rawDescription);



                    string category = forcedCategory ?? classifier?.PredictCategory(item.Title.Text, cleanDescription);

                    newsList.Add(new NewsItem
                    {
                        Title = item.Title.Text.Trim(),
                        Content = cleanDescription,
                        Url = item.Links.FirstOrDefault()?.Uri.ToString(),
                        PublishedAt = item.PublishDate.DateTime,
                        ImageUrl = imageUrl,
                        Source = rssUrl,
                        Category = category
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading RSS feed: {ex.Message}");
            }

            return newsList;
        }
        private static string CleanHtml(string html)
        {
            string decoded = System.Net.WebUtility.HtmlDecode(html);
            var config = AngleSharp.Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = context.OpenAsync(req => req.Content(decoded)).Result;
            string text = document.Body?.TextContent?.Trim() ?? "";
            return text.Normalize(NormalizationForm.FormC);
        }
    }
}