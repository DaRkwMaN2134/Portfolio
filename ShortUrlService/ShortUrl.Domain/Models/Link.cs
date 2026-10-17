namespace ShortUrl.Domain.Models
{
    public class Link
    {
        public long Id { get; set; }
        public long? CreatorId { get; set; }
        public User? Creator { get; set; }
        public string LongUrl { get; set; }
        public string ShortCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Click> Clicks { get; set; } = new();
    }
}
