using Microsoft.EntityFrameworkCore;
using ShortUrl.Domain;
using ShortUrl.Domain.Interfaces;
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
            var link = await _dbContext.links.FirstOrDefaultAsync(l =>
            (!string.IsNullOrEmpty(longurl) && l.LongUrl == longurl) ||
            (!string.IsNullOrEmpty(shortcode) && l.ShortCode == shortcode));

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
    }
}
