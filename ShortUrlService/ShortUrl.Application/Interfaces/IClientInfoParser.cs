using System;
using System.Collections.Generic;
using System.Text;
using DeviceDetectorNET;
using ShortUrl.Application.DTO;

namespace ShortUrl.Application.Interfaces
{
    public interface IClientInfoParser
    {
        public Task<ParserDTO?> GetInfoAsync(HeadersDTO headersDTO);
        public void CleanupDetectorCache();
    }
}
