namespace NewsFlowAPI.Models
{
    public class NewsItem
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }  // Înlocuiește "Description"
        public string Url { get; set; }      // Înlocuiește "Link"
        public string Category { get; set; }
        public int? Likes { get; set; } = 0; // Numărul total de like-uri

        public string? Source { get; set; }
        public string ImageUrl { get; set; }
        public DateTime PublishedAt { get; set; }  // Înlocuiește "PublishDate"

        // Lista de like-uri
        public List<NewsLike> NewsLikes { get; set; } = new List<NewsLike>();

        public List<NewsShare> NewsShares { get; set; } = new List<NewsShare>();
    }
}
