using Microsoft.Extensions.Logging;
using ShortUrl.Application.DTO;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShortUrl.Application.Services
{
    public class ClickService
    {
        private readonly IClickRepository _clickRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly ILogger<ClickService> _logger;

        public ClickService(IClickRepository clickRepository, ILinkRepository linkRepository, ILogger<ClickService> logger)
        {
            _clickRepository = clickRepository;
            _linkRepository = linkRepository;
            _logger = logger;
        }
        public async Task<long?> CollectionInfoForClickAsync(long linkid, ParserDTO dtoinfo, ClientContext clientContext)
        {
            if (dtoinfo == null) return null;

            Click click = new Click
            {
                LinkId = linkid,
                CreatedAt = DateTime.UtcNow,
                IpAddress = clientContext.IpAddress,
                Device = dtoinfo.Device,
                OS = dtoinfo.OS,
                Resolution = null,
                Referer = clientContext.Referer,
                UtmSource = clientContext.UtmSource,
                UtmMedium = clientContext.UtmMedium,
                UtmCampaign = clientContext.UtmCampaign,
                Language = clientContext.Language,
                Browser = dtoinfo.Browser,
                TimeZone = null,
                DeviceLocalTime = null
            };
            var clickid = await _clickRepository.CreateClickAsync(click);
            return clickid;
        }

        public async Task UpdateExternalInfoForClick(long clickid, string resolution, string timezone, DateTime? devicelocaltime)
        {
            var click = await _clickRepository.GetByIdAsync(clickid);
            if (click != null)
            {
                click.Resolution = resolution;
                click.TimeZone = timezone;
                click.DeviceLocalTime = devicelocaltime;
                await _clickRepository.UpdateClickAsync(click);
            }
            
        }
        public async Task<AnalyticsResponseDTO> ClickAnalitycsAsync(long linkid)
        {
            // проверка на валидность linkid
            var clicks = await _clickRepository.GetClickAsync(linkid);
            var analytics = new AnalyticsResponseDTO
            {
                TotalClicks = clicks.clickount,
                LastClicks = clicks.lastclick.Select(c => new ClickDTO
                {
                    Id = c.Id,
                    CreatedAt = c.CreatedAt,
                    IpAddress = c.IpAddress,
                    Device = c.Device,
                    OS = c.OS,
                    Resolution = c.Resolution,
                    Referer = c.Referer,
                    Language = c.Language,
                    Browser = c.Browser,
                    TimeZone = c.TimeZone,
                    DeviceLocalTime = c.DeviceLocalTime,
                }).ToList(),
                UniqueIps = clicks.uniqueip
            };
            return analytics;
        }

        public async Task<bool> ClickUpdateAsync(ClickUpdateRequest data)
        {
            var linkid = await _linkRepository.GetLinkIdByShortCodeAsync(data.Shortcode);

            if (linkid != null)
            {
                await UpdateExternalInfoForClick(data.clickid, data.Resolution, data.TimeZone, data.DeviceLocalTime);
                return true;
            }
            else
            {
                _logger.LogInformation("[МЕТОД ClickUpdateAsync] ССЫЛКА ПО КОРОТКОМУ КОДУ {ShortCode} НЕ БЫЛА НАЙДЕНА", data.Shortcode);
                return false;
            }
        }
    }
}
