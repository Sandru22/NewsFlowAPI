namespace NewsFlowAPI.Models
{
    public class Subscriptions
    {
       public int Id { get; set; }
       public string userId { get; set; }
        public string Source { get; set; }
        public User User { get; set; }
    }
}
