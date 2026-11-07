using ShortUrl.Application.DTO;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;

namespace ShortUrl.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AnonymousUserLoginAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return null;
            var user = await _userRepository.CreateUserAsync(deviceId);
            return user;

        }
    }
}
