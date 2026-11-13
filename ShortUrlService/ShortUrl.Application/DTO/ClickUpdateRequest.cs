using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Application.DTO
{
    public record ClickUpdateRequest(string Shortcode, string Resolution, string TimeZone, DateTime? DeviceLocalTime, long clickid);
}
