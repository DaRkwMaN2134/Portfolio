using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public record class HeadersDTO(string Useragent, Dictionary<string, string?> Headers, ClientContext clientContext);
}
