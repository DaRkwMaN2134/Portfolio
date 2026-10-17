using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public class UserDTO
    {
        public List<Link> Links { get; set; } = new();
        public string DeviceId { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
        public string? OS { get; set; }
        public string? Resolution { get; set; }
        public bool IsRegistered { get; set; }
    }
}
