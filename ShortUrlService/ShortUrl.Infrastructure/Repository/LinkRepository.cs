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

        public async Task CreateLinkAsync(string longurl, string shortcode, User creator)
        {
            if (string.IsNullOrEmpty(shortcode)) return;

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
            }
            else
            {
                Console.WriteLine("ссылка уже создана");
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Link?> GetLinkAsync(string shortcode)
        {
            if (string.IsNullOrEmpty(shortcode)) return null;

            var link = await _dbContext.links
                .Include(l => l.Creator)
                .FirstOrDefaultAsync(l => l.ShortCode == shortcode);

            if (link != null) return link;

            else
            {
                Console.WriteLine("линка не найдена");
                return null;
            }
        }
    }
}
