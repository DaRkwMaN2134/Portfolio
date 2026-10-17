using ShortUrl.Application.DTO;
using ShortUrl.Domain.Interfaces;

namespace ShortUrl.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task UpdateUserInfoAsync(UserDTO userdto, string? deviceid, string? userid)
        {
            var user = await _userRepository.GetUserAsync(deviceid, userid);
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

        public async Task LoginUserAsync(UserDTO userdto, string deviceId, string? userId)
        {
            var user = await _userRepository.GetUserAsync(deviceId, userId);
            if(user == null)
            {
                await _userRepository.CreateUserAsync(deviceId);
                Console.WriteLine($"Юзер {deviceId} создан");
            }
            else
            {
                await UpdateUserInfoAsync(userdto, deviceId, userId);
            }
        }
    }
}
