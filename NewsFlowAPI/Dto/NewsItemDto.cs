namespace NewsFlowAPI.Dto
{
    public class NewsItemDto
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Source { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }
        public int? Likes { get; set; }
        public string ImageUrl { get; set; }

        public bool HasSubscribed { get; set; } 
    }
}
