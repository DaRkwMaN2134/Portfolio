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

            await _dbContext.Database
                    .ExecuteSqlInterpolatedAsync($@"
                    INSERT INTO ""users"" (""DeviceId"", ""UserId"", ""IpAddress"", ""Device"", ""OS"", ""Resolution"", ""CreatedAt"", ""IsRegistered"")
                    VALUES ({deviceid}, {null}, {null}, {null}, {null}, {null}, {DateTime.UtcNow}, {false})
                    ON CONFLICT (""DeviceId"") DO UPDATE
                    SET ""DeviceId"" = EXCLUDED.""DeviceId""");
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
    }
}
