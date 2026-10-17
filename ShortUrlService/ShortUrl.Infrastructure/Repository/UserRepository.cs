using Microsoft.EntityFrameworkCore;
using ShortUrl.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
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
            if (!string.IsNullOrEmpty(deviceid)) return;

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

        public async Task<User?> GetUserAsync(string? deviceid, string? userid, string? username)
        {
            var user = await _dbContext.users.FirstOrDefaultAsync(d =>
            (!string.IsNullOrEmpty(deviceid) && d.DeviceId == deviceid) ||
            (!string.IsNullOrEmpty(userid) && d.UserId == userid) ||
            (!string.IsNullOrEmpty(username) && d.Username == username));

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
            if (!string.IsNullOrEmpty(currentUser.DeviceId)) return;

            var user = await _dbContext.users.FirstOrDefaultAsync(d => d.DeviceId == currentUser.DeviceId);
            if (user == null)
            {
                Console.WriteLine("Такого юзера не существует");
            }
            else
            {
                _dbContext.users.Update(user);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
