namespace NewsFlowAPI.Models
{
    public class NewsLike
    {
        public int NewsId { get; set; } // Cheie străină către News
        public string UserId { get; set; } // Cheie străină către User (AspNetUsers)
        public DateTime LikedAt { get; set; } // Data și ora la care s-a dat like

        // Proprietate de navigare către NewsItem
        public NewsItem NewsItem { get; set; }

        // Proprietate de navigare către User (AspNetUsers)
        public User User { get; set; }

    }
}
