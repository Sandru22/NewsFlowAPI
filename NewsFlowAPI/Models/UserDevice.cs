namespace NewsFlowAPI.Models
{
    public class UserDevice
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string DeviceToken { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
