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
        /* public async Task UpdateUserInfoAsync(User userinfo, string? deviceid, string? userid)
        {
            var user = await _userRepository.GetUserAsync(deviceid, userid);
            if (user == null)
            {
                Console.WriteLine("Такого пользователя не существует");
            }
            else
            {
                if (userinfo.DeviceId != null) user.DeviceId = userinfo.DeviceId;
                if (userinfo.UserId != null) user.UserId = userinfo.UserId;
                if (userinfo.Username != null) user.Username = userinfo.Username;
                if (userinfo.IpAddress != null) user.IpAddress = userinfo.IpAddress;
                if (userinfo.Device != null) user.Device = userinfo.Device;
                if (userinfo.OS != null) user.OS = userinfo.OS;
                if (userinfo.Resolution != null) user.Resolution = userinfo.Resolution;
                if (userinfo.IsRegistered != false) user.IsRegistered = userinfo.IsRegistered;

                await _userRepository.UpdateUserAsync(user);
            } 
        }     этот метод скорей всего избыточен    */

        public async Task LoginUserAsync(User userinfo, string deviceId, string? userId)
        {
            var user = await _userRepository.GetUserAsync(deviceId, userId);
            if(user == null)
            {
                await _userRepository.CreateUserAsync(deviceId);
                Console.WriteLine($"Юзер {deviceId} создан");
            }
            else
            {
                await _userRepository.UpdateUserAsync(user);
                //await UpdateUserInfoAsync(userinfo, deviceId, userId);
            }
        }

        public async Task GetBufferAsync(User creator)
        {
            var user = await _userRepository.GetUserAsync("123d", null);
            var links = await _userRepository.GetUserBufferAsync(user);
            foreach (var link in links)
            {
                Console.WriteLine(link.LongUrl); // потом будет подвязка к API или куда-либо еще
            }
        }
    }
}
