using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Domain.Interfaces
{
    public interface ILinkRepository
    {
        public Task<bool> CreateLinkAsync(string longurl, string shortcode, User creator);
        public Task<string?> GetLongUrlAsync(string shortcode);
        public Task<string?> GetShortCodeAsync(string url);
        public Task<Link?> GetUserLinkAsync(string url, string deviceid);
    }
}
