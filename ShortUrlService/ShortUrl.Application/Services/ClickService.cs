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
    }
}
