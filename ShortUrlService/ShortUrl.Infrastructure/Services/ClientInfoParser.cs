using DeviceDetectorNET;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Infrastructure.Services
{
    public class ClientInfoParser : IClientInfoParser
    {
        private readonly IMemoryCache _memoryCache;
        public ClientInfoParser(IMemoryCache cache)
        {
            _memoryCache = cache;
        }
        public ParserDTO? GetInfo(string useragent, Dictionary<string, string?> headers)
        {
            if (string.IsNullOrEmpty(useragent) || headers == null) return null;

            var platform = headers.ContainsKey("sec-ch-ua-platform") ? headers["sec-ch-ua-platform"] : "unknown";
            var key = useragent + "|" + platform;

            if (!_memoryCache.TryGetValue(key, out DeviceDetector? result))
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                .SetPriority(CacheItemPriority.High);

                var clientHints = ClientHints.Factory(headers);
                var uainfo = new DeviceDetector(useragent, clientHints);
                uainfo.Parse();
                _memoryCache.Set(key, uainfo, cacheOptions);
                result = uainfo;
            }
            var info = new ParserDTO
            {
                Device = result?.GetDeviceName(),
                OS = result?.GetOs()?.Match != null ? result.GetOs().Match.Family.ToString() + " " + result.GetOs().Match.Version : "Unknown",
                Browser = result?.GetBrowserClient()?.Match != null ? result.GetBrowserClient().Match.Family.ToString() + " " + result.GetBrowserClient().Match.Version : "Unknown"
            };

            return info;
        }
    }
}
