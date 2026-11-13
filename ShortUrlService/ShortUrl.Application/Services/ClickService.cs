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
        public ClickService(IClickRepository clickRepository, ILinkRepository linkRepository)
        {
            _clickRepository = clickRepository;
            _linkRepository = linkRepository;
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
                Console.WriteLine($"[ID ЭТОЙ ССЫЛКИ] = {click.LinkId}");
                click.Resolution = resolution;
                click.TimeZone = timezone;
                click.DeviceLocalTime = devicelocaltime;
                await _clickRepository.UpdateClickAsync(click);
            }
            
        }
        public async Task<AnalyticsResponseDTO> ClickAnalitycsAsync(long linkid) // очень странный код у этого метода
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
                Console.WriteLine($"[ТУТ МЕГА ССЫЛКА]  {linkid}");
                await UpdateExternalInfoForClick(data.clickid, data.Resolution, data.TimeZone, data.DeviceLocalTime);
                return true;
            }
            else
            {
                Console.WriteLine($"[МЕГА ССЫЛКА НЕ НАЙДЕНА]");
                return false;
            }
        }
    }
}
