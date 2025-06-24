namespace NewsFlowAPI.Models
{
    public class NewsShare
    {
        public int NewsId { get; set; } 
        public string UserId { get; set; } 
        public DateTime SharedAt { get; set; } 

        public NewsItem NewsItem { get; set; }

        public User User { get; set; }
    }
}
