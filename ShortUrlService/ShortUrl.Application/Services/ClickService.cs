using ShortUrl.Application.DTO;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.Services
{
    public class ClickService
    {
        private readonly IClickRepository _clickRepository;
        public ClickService(IClickRepository clickRepository)
        {
            _clickRepository = clickRepository;
        }
        public async Task CollectionInfoForClickAsync(long linkid, Click clickinfo)
        {
            await _clickRepository.CreateClickAsync(linkid, clickinfo);
        }

        public async Task UpdateExternalInfoForClick(long linkid, string resolution, string timezone, DateTime? devicelocaltime)
        {
            var click = await _clickRepository.GetLastClickAsync(linkid);
            if(click != null)
            {
                Console.WriteLine($"[ID ЭТОЙ ССЫЛКИ] = {click.LinkId}");
                click.Resolution = resolution;
                click.TimeZone = timezone;
                click.DeviceLocalTime = devicelocaltime;
                await _clickRepository.UpdateLastClickAsync(click);
            }
        }
        public async Task<List<Click>> ClickAnalitycsAsync(long linkid)
        {
            var clicks = await _clickRepository.GetClickAsync(linkid);
            if (clicks != null)
            {
                return clicks;
            }
            else
            {
                Console.WriteLine("Клики не обнаружены");
                return null;
            }
        }
    }
}
