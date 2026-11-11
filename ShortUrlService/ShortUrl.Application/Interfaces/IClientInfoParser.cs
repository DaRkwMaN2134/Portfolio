using System;
using System.Collections.Generic;
using System.Text;
using DeviceDetectorNET;
using ShortUrl.Application.DTO;

namespace ShortUrl.Application.Interfaces
{
    public interface IClientInfoParser
    {
        public ParserDTO? GetInfo(string useragent, Dictionary<string, string?> headers);
    }
}
