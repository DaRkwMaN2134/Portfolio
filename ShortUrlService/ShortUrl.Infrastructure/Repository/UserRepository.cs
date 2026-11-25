using DeviceDetectorNET.Parser.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;

namespace ShortUrl.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(AppDbContext dbContext, ILogger<UserRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<User?> GetOrCreateUserAsync(string deviceid)
        {
            if (string.IsNullOrEmpty(deviceid)) return null;

            var createUser = await _dbContext.users.FromSqlInterpolated($@"
                    INSERT INTO ""users"" (""DeviceId"", ""LastSeenAt"", ""UserId"", ""IpAddress"", ""Device"", ""OS"", ""Resolution"", ""CreatedAt"", ""IsRegistered"")
                    VALUES ({deviceid}, {DateTime.UtcNow}, {null}, {null}, {null}, {null}, {null}, {DateTime.UtcNow}, {false})
                    ON CONFLICT (""DeviceId"") DO UPDATE
                    SET ""LastSeenAt"" = EXCLUDED.""LastSeenAt""
                    RETURNING *").ToListAsync();

            var user = createUser.First();
            return user;
        }

        /* public async Task CreateRegisteredUserAsync(string deviceid, string userid)  добавим реализацию когда нужно будет добавить JWT для юзера
        {
        } */




        public async Task<User?> GetByDeviceIdAsync(string deviceid)
        {
            if(string.IsNullOrEmpty(deviceid)) return null;
            var user = await _dbContext.users.FirstOrDefaultAsync(d => d.DeviceId == deviceid);
            if (user == null)
            {
                _logger.LogInformation("[Юзер по DEVICE ID {DeviceId} не найден]", deviceid);
                return null;
            }
            else
            {
                return user;
            }
        }

        public async Task<User?> GetByUserIdAsync(string userid)
        {
            if (string.IsNullOrEmpty(userid)) return null;
            var user = await _dbContext.users.FirstOrDefaultAsync(d => d.UserId == userid);
            if (user == null)
            {
                _logger.LogInformation("[Юзер по USER ID {UserId} не найден]", userid);
                return null;
            }
            else
            {
                return user;
            }
        }



        public async Task UpdateUserAsync(User currentUser)
        {
            if (currentUser == null) return;

            _dbContext.users.Update(currentUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CleanupAnonymousUsersAsync()
        {
            await _dbContext.users.Where(r => r.IsRegistered == false && r.LastSeenAt < DateTime.UtcNow.AddDays(-30)).ExecuteDeleteAsync();
        }
    }
}
