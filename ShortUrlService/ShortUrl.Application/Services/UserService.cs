using DeviceDetectorNET;
using DeviceDetectorNET.Parser.Device;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Interfaces;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShortUrl.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IClientInfoParser _infoParser;
        public UserService(IUserRepository userRepository, IClientInfoParser infoParser)
        {
            _userRepository = userRepository;
            _infoParser = infoParser;
        }

        public async Task<User?> AnonymousUserLoginAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return null;
            var user = await _userRepository.CreateUserAsync(deviceId);
            return user;

        }

        public async Task<bool> UserUpdateAsync(string deviceid, string useragent, Dictionary<string, string?> headers, UserUpdateRequest data, string ipaddres)
        {
            var currentUser = await _userRepository.GetByDeviceIdAsync(deviceid);
            if (currentUser != null)
            {
                var info = _infoParser.GetInfo(useragent, headers);

                if (info == null) return false;

                currentUser.OS = info.OS;
                currentUser.Device = info.Device;
                currentUser.Resolution = data.Resolution;
                currentUser.IpAddress = ipaddres;
                await _userRepository.UpdateUserAsync(currentUser);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
