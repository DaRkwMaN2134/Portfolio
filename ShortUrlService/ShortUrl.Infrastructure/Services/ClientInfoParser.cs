using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ShortUrl.Infrastructure.Services
{
    public class ClientInfoParser : IClientInfoParser
    {
        private readonly IDistributedCache _distributedCache;
        private static readonly ICache _detectorCache = new DictionaryCache();
        private static readonly DistributedCacheEntryOptions _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        private readonly ILogger<ClientInfoParser> _logger;

        public ClientInfoParser(IDistributedCache cache, ILogger<ClientInfoParser> logger)
        {
            _distributedCache = cache;
            _logger = logger;
        }
        public async Task<ParserDTO?> GetInfoAsync(HeadersDTO headersDTO)
        {
            if (string.IsNullOrEmpty(headersDTO.Useragent) || headersDTO.Headers == null) return null;

            var platform = headersDTO.Headers.FirstOrDefault(h => h.Key.Equals("sec-ch-ua-platform", StringComparison.OrdinalIgnoreCase)).Value ?? "unknown";

            var key = headersDTO.Useragent + "|" + platform;

            try
            {
                string? cachedJson = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(cachedJson))
                {
                    return JsonSerializer.Deserialize<ParserDTO?>(cachedJson)!;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Redis недоступен при чтении кэша User-Agent. Парсим напрямую.");
            }

            var clientHints = ClientHints.Factory(headersDTO.Headers);
            var uainfo = new DeviceDetector(headersDTO.Useragent, clientHints);
            uainfo.SetCache(_detectorCache);
            uainfo.Parse();

            var info = new ParserDTO
            {
                Device = uainfo?.GetDeviceName(),
                OS = uainfo?.GetOs()?.Match != null ? uainfo.GetOs().Match.Family.ToString() + " " + uainfo.GetOs().Match.Version : "Unknown",
                Browser = uainfo?.GetBrowserClient()?.Match != null ? uainfo.GetBrowserClient().Match.Family.ToString() + " " + uainfo.GetBrowserClient().Match.Version : "Unknown"
            };

            try
            {
                await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(info), _cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось записать User-Agent в Redis.");
            }

            return info;
        }
        
        
        public void CleanupDetectorCache()
        {
            _detectorCache.FlushAll();
        }
    }
}
