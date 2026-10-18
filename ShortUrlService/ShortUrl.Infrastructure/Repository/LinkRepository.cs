using Microsoft.EntityFrameworkCore;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Infrastructure.Repository
{
    public class LinkRepository : ILinkRepository
    {
        private readonly AppDbContext _dbContext;
        public LinkRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CreateLinkAsync(string longurl, string shortcode, User creator)
        {
            if (string.IsNullOrEmpty(shortcode)) return false;

            var link = await _dbContext.links.FirstOrDefaultAsync(l => l.ShortCode == shortcode);

            if(link == null)
            {
                link = new Link
                {
                    Creator = creator,
                    LongUrl = longurl,
                    ShortCode = shortcode,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.links.Add(link);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                Console.WriteLine("ссылка уже создана");
                return false;
            }
        }

        public async Task<string?> GetLongUrlAsync(string shortcode)
        {
            if (string.IsNullOrEmpty(shortcode)) return null;

            var link = await _dbContext.links
                .Include(l => l.Creator)
                .FirstOrDefaultAsync(l => l.ShortCode == shortcode);

            if (link != null) return link.LongUrl;

            else
            {
                Console.WriteLine("линка не найдена");
                return null;
            }
        }

        public async Task<string?> GetShortCodeAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var link = await _dbContext.links
                .Include(l => l.Creator)
                .FirstOrDefaultAsync(l => l.LongUrl == url);

            if (link != null) return link.ShortCode;

            else
            {
                Console.WriteLine("линка не найдена");
                return null;
            }
        }

        public async Task<Link?> GetUserLinkAsync(string url, string deviceid)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (string.IsNullOrEmpty(deviceid)) return null;

            var link = await _dbContext.links
                .FirstOrDefaultAsync(c => c.Creator.DeviceId == deviceid && c.LongUrl == url);

            if (link != null) return link;

            else
            {
                Console.WriteLine("креатор не найден");
                return null;
            }
        }
    }
}
