using Microsoft.EntityFrameworkCore;
using ShortUrl.Application.DTO;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Infrastructure.Repository
{
    public class ClickRepository : IClickRepository
    {
        private readonly AppDbContext _dbContext;
        public ClickRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<long> CreateClickAsync(Click click)
        {
            _dbContext.clicks.Add(click);
            await _dbContext.SaveChangesAsync();
            return click.Id;
        }

        public async Task<Click?> GetByIdAsync(long clickid)
        {
            var click = await _dbContext.clicks.FirstOrDefaultAsync(l => l.Id == clickid); 

            if (click != null)
            {
                return click;
            }
            else
            {
                Console.WriteLine("click не найден");
                return null;
            }
        }

        public async Task UpdateClickAsync(Click currentclick)
        {
            if (currentclick == null) return;
            _dbContext.clicks.Update(currentclick);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<(int clickount, List<Click> lastclick, int uniqueip)> GetClickAsync(long linkid)
        {
            var clickCount = await _dbContext.clicks.Where(l => l.LinkId == linkid).CountAsync();
            var lastClicks = await _dbContext.clicks.Where(l => l.LinkId == linkid).OrderByDescending(l => l.CreatedAt).Take(5).ToListAsync();
            var uniqueIpCount = await _dbContext.clicks.Where(l => l.LinkId == linkid).Select(i => i.IpAddress).Distinct().CountAsync();

            return (clickCount, lastClicks, uniqueIpCount);

        }
    }
}
