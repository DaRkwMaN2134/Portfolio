using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public class BufferDTO
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LongUrl { get; set; }
        public string ShortCode { get; set; }
        public long? CreatorId { get; set; }
    }
}
