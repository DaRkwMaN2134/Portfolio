using ShortUrl.Domain.Models;

namespace ShortUrl.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> GetOrCreateUserAsync(string deviceid);
        public Task<User?> GetByDeviceIdAsync(string deviceid);
        public Task<User?> GetByUserIdAsync(string userid);
        public Task UpdateUserAsync(User user);
        public Task CleanupAnonymousUsersAsync();
    }
}
