using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Domain.Interfaces
{
    public interface ILinkRepository
    {
        public Task CreateLinkAsync(string longurl, string shortcode, User creator);
        public Task<Link?> GetLinkAsync(string shortcode);
    }
}
