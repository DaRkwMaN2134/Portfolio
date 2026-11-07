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

        public async Task<User?> CreateUserAsync(string deviceid)
        {
            if (string.IsNullOrEmpty(deviceid)) return null;

            var createUser = await _dbContext.users.FromSqlInterpolated($@"
                    INSERT INTO ""users"" (""DeviceId"", ""UserId"", ""IpAddress"", ""Device"", ""OS"", ""Resolution"", ""CreatedAt"", ""IsRegistered"")
                    VALUES ({deviceid}, {null}, {null}, {null}, {null}, {null}, {DateTime.UtcNow}, {false})
                    ON CONFLICT (""DeviceId"") DO UPDATE
                    SET ""DeviceId"" = EXCLUDED.""DeviceId""
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
                Console.WriteLine("анонимный юзер не найден");
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
                Console.WriteLine("залогиненный юзер не найден");
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
    }
}
