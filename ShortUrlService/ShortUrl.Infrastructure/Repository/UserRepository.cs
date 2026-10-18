using Microsoft.EntityFrameworkCore;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;

namespace ShortUrl.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;
        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateUserAsync(string deviceid)
        {
            if (string.IsNullOrEmpty(deviceid)) return;

            var user = await _dbContext.users.FirstOrDefaultAsync(d => d.DeviceId == deviceid);
            if (user == null)
            {
                user = new User
                {
                    Links = new(),
                    DeviceId = deviceid, 
                    UserId = null, 
                    Username = null, 
                    OS = null, 
                    Resolution = null, 
                    CreatedAt = DateTime.UtcNow , 
                    IsRegistered = false
                };
                _dbContext.users.Add(user);
            }
            else
            {
                Console.WriteLine("юзер уже создан");
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserAsync(string? deviceid, string? userid)
        {
            var user = await _dbContext.users.FirstOrDefaultAsync(d =>
            (!string.IsNullOrEmpty(deviceid) && d.DeviceId == deviceid) ||
            (!string.IsNullOrEmpty(userid) && d.UserId == userid));

            if (user != null)
            {
                return user;
            }
            else
            {
                Console.WriteLine("юзер не найден");
                return null;
            }
        }

        public async Task UpdateUserAsync(User currentUser)
        {
            _dbContext.users.Update(currentUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Link>> GetUserBufferAsync(User creator)
        {
            var linkBuffer = await _dbContext.users.
                Where(c => c.DeviceId == creator.DeviceId).
                SelectMany(l => l.Links).OrderByDescending(l => l.Id).Take(3).ToListAsync();
            return linkBuffer;
        }
    }
}
