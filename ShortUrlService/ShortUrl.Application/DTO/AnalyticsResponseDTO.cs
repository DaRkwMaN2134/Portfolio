using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public class AnalyticsResponseDTO
    {
        public int TotalClicks { get; set; }
        public List<ClickDTO> LastClicks { get; set; }
        public int UniqueIps { get; set; }
    }
}
