using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public class ParserDTO
    {
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
        public string? OS { get; set; }
        public string? Referer { get; set; }
        public string? Language { get; set; }
        public string? Browser { get; set; }

    }
}
