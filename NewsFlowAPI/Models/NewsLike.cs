namespace NewsFlowAPI.Models
{
    public class NewsLike
    {
        public int NewsId { get; set; } 
        public string UserId { get; set; }
        public DateTime LikedAt { get; set; } 

        public NewsItem NewsItem { get; set; }

        public User User { get; set; }

    }
}
