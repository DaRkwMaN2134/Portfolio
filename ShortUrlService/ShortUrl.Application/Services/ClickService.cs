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
        public async Task CollectionInfoForClickAsync(Click click, ParserDTO dtoinfo, ClientContext clientContext)
        {
            if (dtoinfo == null || click == null)
            {
                _logger.LogWarning("[МЕТОД CollectionInfoForClickAsync] ВХОДНЫЕ ДАННЫЕ ОКАЗАЛИСЬ ПУСТЫЕ {DtoInfo} : {Click}", dtoinfo, click);
                return;
            }

            click.CreatedAt = DateTime.UtcNow;
            click.IpAddress = clientContext.IpAddress;
            click.Device = dtoinfo.Device;
            click.OS = dtoinfo.OS;
            click.Referer = clientContext.Referer;
            click.UtmSource = clientContext.UtmSource;
            click.UtmMedium = clientContext.UtmMedium;
            click.UtmCampaign = clientContext.UtmCampaign;
            click.Language = clientContext.Language;
            click.Browser = dtoinfo.Browser;

            await _clickRepository.UpdateClickAsync(click);
            _logger.LogInformation("[МЕТОД CollectionInfoForClickAsync] ИНФОРМАЦИЯ О КЛИКЕ УСПЕШНО ОБНОВЛЕНА. Id КЛИКА: {ClickId}", click.Id);
        }

        public async Task UpdateExternalInfoForClick(ClickUpdateRequest data)
        {
            var click = await _clickRepository.GetByIdAsync(data.clickid);
            if (click != null)
            {
                click.Resolution = data.Resolution;
                click.TimeZone = data.TimeZone;
                click.DeviceLocalTime = data.DeviceLocalTime;
                click.UtmSource = data.UtmSource;
                click.UtmMedium = data.UtmMedium;
                click.UtmCampaign = data.UtmCampaign;
                await _clickRepository.UpdateClickAsync(click);
                _logger.LogInformation("[МЕТОД UpdateExternalInfoForClick] ИНФОРМАЦИЯ О КЛИКЕ УСПЕШНО ОБНОВЛЕНА. Id КЛИКА: {ClickId}", click.Id);
            }
            else
            {
                _logger.LogWarning("[МЕТОД UpdateExternalInfoForClick] КЛИК НЕ БЫЛ НАЙДЕН");
            }
        }
        public async Task<AnalyticsResponseDTO> ClickAnalitycsAsync(long linkid)
        {
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
            _logger.LogInformation("[МЕТОД ClickAnalitycsAsync] АНАЛИТИКА О КЛИКЕ УСПЕШНО НАЙДЕНА");
            return analytics;
        }

        public async Task<bool> ClickUpdateAsync(ClickUpdateRequest data)
        {
            var linkid = await _linkRepository.GetLinkIdByShortCodeAsync(data.Shortcode);

            if (linkid != null)
            {
                await UpdateExternalInfoForClick(data);
                _logger.LogInformation("[МЕТОД ClickUpdateAsync] ССЫЛКА ПО КОРОТКОМУ КОДУ {ShortCode} БЫЛА НАЙДЕНА", data.Shortcode);
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
