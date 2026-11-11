using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public record ClientContext(string IpAddress, string Referer, string Language);
}
