using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Task CreateUserAsync(string deviceid);
        public Task<User> GetUserAsync(string? deviceid, string? userid, string? username);
        public Task UpdateUserAsync(User userdto);
    }
}
