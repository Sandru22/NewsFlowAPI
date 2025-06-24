namespace NewsFlowAPI.Models
{
    public class NewsTrainingInput
    {
        public string UserId { get; set; }               
        public string Category { get; set; }             
        public string Title { get; set; }               
        public string Content { get; set; }          
        public float Label { get; set; }
    }
}
