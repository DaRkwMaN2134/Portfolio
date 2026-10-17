using ShortUrl.Domain.Models;

namespace ShortUrl.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task CreateUserAsync(string deviceid);
        public Task<User?> GetUserAsync(string deviceid, string? userid);
        public Task UpdateUserAsync(User user);
    }
}
