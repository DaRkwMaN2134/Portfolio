using DeviceDetectorNET;
using DeviceDetectorNET.Parser.Device;
using Microsoft.Extensions.Logging;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Interfaces;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System.Reflection;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShortUrl.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IClientInfoParser _infoParser;
        private readonly IBackgroundChannelTask<ParserDTO?> _channelTask;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserRepository userRepository, IClientInfoParser infoParser, IBackgroundChannelTask<ParserDTO?> channelTask, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _infoParser = infoParser;
            _channelTask = channelTask;
            _logger = logger;
        }

        public async Task<User?> AnonymousUserLoginAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogWarning("[МЕТОД AnonymousUserLoginAsync] DeviceId ПРИШЕЛ ПУСТЫМ");
                return null;
            }
            var user = await _userRepository.GetOrCreateUserAsync(deviceId);
            _logger.LogInformation("[МЕТОД AnonymousUserLoginAsync] ЮЗЕР {DeviceId} УСПЕШНО НАЙДЕН ИЛИ СОЗДАН", deviceId);
            return user;

        }

        public async Task<bool> UserUpdateAsync(string deviceid, HeadersDTO headersDTO,
            UserUpdateRequest data, string ipaddres)
        {
            var currentUser = await _userRepository.GetByDeviceIdAsync(deviceid);
            if (currentUser != null)
            {
                var info = await _infoParser.GetInfoAsync(headersDTO);

                if (info == null) return false;

                currentUser.OS = info.OS;
                currentUser.Device = info.Device;
                currentUser.Resolution = data.Resolution;
                currentUser.IpAddress = ipaddres;
                _logger.LogInformation("[МЕТОД UserUpdateAsync] ДАННЫЕ О ЮЗЕРЕ {DeviceId} УСПЕШНО ОБНОВЛЕНЫ", deviceid);
                await _userRepository.UpdateUserAsync(currentUser);
                return true;
            }
            else
            {
                _logger.LogWarning("[МЕТОД UserUpdateAsync] ДАННЫЕ О ЮЗЕРЕ {DeviceId} НЕ УДАЛОСЬ ОБНОВИТЬ", deviceid);
                return false;
            }
        }
    }
}
