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

        public async Task<Link?> CreateLinkAsync(string longurl, string shortcode, User creator)
        {
            if (string.IsNullOrEmpty(shortcode) || string.IsNullOrEmpty(longurl) || creator == null) return null;

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
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch(DbUpdateException ex)
                {
                    Console.WriteLine($"[МЕТОД CreateLinkAsync] ОШИБКА {ex.Message}");
                    return null;
                }
                return link;
            }
            else
            {
                Console.WriteLine("[МЕТОД CreateLinkAsync] ссылка уже создана");
                return null;
            }
        }

        public async Task<string> GetLongUrlAsync(string shortcode)
        {
            if (string.IsNullOrEmpty(shortcode)) return string.Empty;

            var longurl = await _dbContext.links
                .Where(s => s.ShortCode == shortcode)
                .Select(s => s.LongUrl)
                .FirstOrDefaultAsync();

            if (longurl != null) return longurl;

            else
            {
                Console.WriteLine("[МЕТОД GetLongUrlAsync] линка не найдена");
                return string.Empty;
            }
        }

        public async Task<string> GetShortCodeAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            var shortcode = await _dbContext.links
                .Where(l => l.LongUrl == url)
                .Select(l => l.ShortCode)
                .FirstOrDefaultAsync();

            if (shortcode != null) return shortcode;

            else
            {
                Console.WriteLine("[МЕТОД GetShortCodeAsync] линка не найдена");
                return string.Empty;
            }
        }

        public async Task<Link?> GetUserLinkAsync(string url, string deviceid)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(deviceid)) return null;

            var link = await _dbContext.links
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Creator != null && c.Creator.DeviceId == deviceid && c.LongUrl == url);

            if (link != null) return link;

            else
            {
                Console.WriteLine("[МЕТОД GetUserLinkAsync] креатор не найден");
                return null;
            }
        }

        public async Task<List<Link>> GetUserBufferAsync(string deviceid)
        {
            if (string.IsNullOrEmpty(deviceid)) return new List<Link>();
            var linkBuffer = await _dbContext.links
                .AsNoTracking()
                .Where(c => c.Creator != null && c.Creator.DeviceId == deviceid).OrderByDescending(l => l.Id).Take(3).ToListAsync();
            return linkBuffer;
        }

        public async Task<long?> GetLinkIdByShortCodeAsync(string shortcode)
        {
            if (string.IsNullOrEmpty(shortcode)) return null;

            var linkid = await _dbContext.links
                .Where(s => s.ShortCode == shortcode)
                .Select(s => (long?)s.Id)
                .FirstOrDefaultAsync();
            if (linkid != null)
            {
                return linkid;
            }
            else
            {
                return null;
            }
        }

        public async Task<Link?> GetLinkByShortCodeAsync(string shortcode)
        {
            if (string.IsNullOrEmpty(shortcode)) return null;

            var link = await _dbContext.links.FirstOrDefaultAsync(s => s.ShortCode == shortcode);
            if (link != null)
            {
                return link;
            }
            else
            {
                return null;
            }
        }
    }
}
