using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack; // Necesită instalarea HtmlAgilityPack din NuGet

namespace NewsFlowAPI.Models
{
    public class RssService
    {
        public async Task<List<NewsItem>> GetNewsAsync(string rssUrl)
        {
            List<NewsItem> newsList = new();
            try
            {
                using var reader = XmlReader.Create(rssUrl);
                var feed = SyndicationFeed.Load(reader);

                foreach (var item in feed.Items)
                {
                    string imageUrl = "";

                    // Caută imaginea în enclosure
                    var enclosure = item.Links.FirstOrDefault(l => l.RelationshipType == "enclosure");
                    if (enclosure != null)
                    {
                        imageUrl = enclosure.Uri.ToString();
                    }

                    // Caută imaginea în elemente personalizate
                    if (string.IsNullOrEmpty(imageUrl) && item.ElementExtensions.Any())
                    {
                        var thumbElement = item.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "thumbnail" || e.OuterName == "enclosure");

                        if (thumbElement != null)
                        {
                            imageUrl = thumbElement.GetObject<XElement>().Attribute("url")?.Value ?? "";
                        }
                    }

                    // Curățăm descrierea de HTML
                    string rawDescription = item.Summary?.Text ?? "No description";
                    string cleanDescription = CleanHtml(rawDescription);

                    newsList.Add(new NewsItem
                    {
                        Title = item.Title.Text.Trim(),
                        Content = cleanDescription,
                        Url = item.Links.FirstOrDefault()?.Uri.ToString(),
                        PublishedAt = item.PublishDate.DateTime,
                        ImageUrl = imageUrl
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading RSS feed: {ex.Message}");
            }

            return newsList;
        }

        // Metodă pentru curățarea HTML-ului din text
        private static string CleanHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText.Trim();
        }
    }
}