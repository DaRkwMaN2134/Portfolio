
namespace ShortUrl.Domain
{
    public class Click
    {
        public long Id { get; set; }
        public long LinkId { get; set; }
        public Link Link { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? IpAddress {  get; set; }
        public string? Device {  get; set; }
        public string? OS { get; set; }
        public string? Resolution { get; set; }
        public string? Referer { get; set; }
        public string? Language { get; set; }
        public string? Browser {  get; set; }
        public string? TimeZone { get; set; }
        public DateTime? DeviceLocalTime { get; set; }
    }
}
