using Microsoft.EntityFrameworkCore;
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

        public async Task CreateClickAsync(long linkid, Click clickinfo)
        {
            var click = new Click
            {
                LinkId = linkid,
                CreatedAt = DateTime.UtcNow,
                IpAddress = clickinfo.IpAddress,
                Device = clickinfo.Device,
                OS = clickinfo.OS,
                Resolution = clickinfo.Resolution,
                Referer = clickinfo.Referer,
                Language = clickinfo.Language,
                Browser = clickinfo.Browser,
                TimeZone = clickinfo.TimeZone,
                DeviceLocalTime = clickinfo.DeviceLocalTime
            };
            _dbContext.clicks.Add(click);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Click?> GetLastClickAsync(long linkid)
        {
            var click = await _dbContext.clicks.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(l => l.LinkId == linkid);

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

        public async Task UpdateLastClickAsync(Click currentclick)
        {
            _dbContext.clicks.Update(currentclick);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Click?>> GetClickAsync(long linkid)
        {
            var click = await _dbContext.clicks.OrderByDescending(c => c.CreatedAt).Where(c => c.LinkId == linkid).ToListAsync();
            if (click != null)
            {
                return click;
            }
            return null;
        }
    }
}
