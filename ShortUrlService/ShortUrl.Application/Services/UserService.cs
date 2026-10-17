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
        public async Task UserUpdateAsync(UserDTO userdto, string? deviceid, string? userid, string? username)
        {
            var user = await _userRepository.GetUserAsync(deviceid, userid, username);
            if (user == null)
            {
                Console.WriteLine("Такого пользователя не существует");
            }
            else
            {
                if (userdto.DeviceId != null) user.DeviceId = userdto.DeviceId;
                if (userdto.UserId != null) user.UserId = userdto.UserId;
                if (userdto.Username != null) user.Username = userdto.Username;
                if (userdto.IpAddress != null) user.IpAddress = userdto.IpAddress;
                if (userdto.Device != null) user.Device = userdto.Device;
                if (userdto.OS != null) user.OS = userdto.OS;
                if (userdto.Resolution != null) user.Resolution = userdto.Resolution;
                if (userdto.IsRegistered != false) user.IsRegistered = userdto.IsRegistered;

                await _userRepository.UpdateUserAsync(user);
            }
        }
    }
}
