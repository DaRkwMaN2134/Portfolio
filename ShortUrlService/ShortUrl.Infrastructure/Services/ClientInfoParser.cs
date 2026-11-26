using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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

        public ClientInfoParser(IDistributedCache cache)
        {
            _distributedCache = cache;
        }
        public async Task<ParserDTO?> GetInfoAsync(string useragent, Dictionary<string, string?> headers)
        {
            if (string.IsNullOrEmpty(useragent) || headers == null) return null;

            var platform = headers.FirstOrDefault(h => h.Key.Equals("sec-ch-ua-platform", StringComparison.OrdinalIgnoreCase)).Value ?? "unknown";

            var key = useragent + "|" + platform;

            string? cachedJson = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                return JsonSerializer.Deserialize<ParserDTO?>(cachedJson)!;
            }
            var clientHints = ClientHints.Factory(headers);
            var uainfo = new DeviceDetector(useragent, clientHints);
            uainfo.SetCache(_detectorCache);
            uainfo.Parse();

            var info = new ParserDTO
            {
                Device = uainfo?.GetDeviceName(),
                OS = uainfo?.GetOs()?.Match != null ? uainfo.GetOs().Match.Family.ToString() + " " + uainfo.GetOs().Match.Version : "Unknown",
                Browser = uainfo?.GetBrowserClient()?.Match != null ? uainfo.GetBrowserClient().Match.Family.ToString() + " " + uainfo.GetBrowserClient().Match.Version : "Unknown"
            };

            await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(info), _cacheOptions);

            return info;
        }
        
        
        public void CleanupDetectorCache()
        {
            _detectorCache.FlushAll();
        }
    }
}
