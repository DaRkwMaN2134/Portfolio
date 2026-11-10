using DeviceDetectorNET;
using Microsoft.Extensions.Caching.Memory;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ShortUrl.Presentation
{
    public static class LinkEndpoints
    {
        public static void MapLinkEndpoints(this WebApplication app)
        {
            app.MapGet("/b/buffer", BufferEndpoint);
            app.MapPost("/shorten", ShortenEndpoint);
            app.MapGet("/s/{shortcode}", ShortcodeEndpoint);
        }


        private static async Task<IResult> BufferEndpoint(LinkService linkservice, HttpContext context)
        {
            if (context.Items["DeviceId"] == null) return Results.BadRequest();

            var deviceid = context.Items["DeviceId"].ToString();

            var links = await linkservice.GetBufferAsync(deviceid); // вынести в BufferDTO/LinkDTO

            var result = links.Select(l => new
            {
                l.Id,
                l.CreatedAt,
                l.LongUrl,
                l.ShortCode,
                l.CreatorId
            });
            return Results.Ok(result);
        }

        private static async Task<IResult> ShortenEndpoint(UrlRequest request, LinkService linkservice, IUserRepository userrep, HttpContext context)
        {
            if (context.Items["DeviceId"] == null) return Results.BadRequest();
            var deviceid = context.Items["DeviceId"].ToString();

            Console.WriteLine($"[ТУТ МЕГА DEVICEID]  {deviceid}");

            var user = await userrep.GetByDeviceIdAsync(deviceid);
            if (user == null)
            {
                return Results.NotFound();
            }

            var shortcode = await linkservice.ShortenLinkAsync(request.LongUrl, deviceid, user);
            if (!string.IsNullOrEmpty(shortcode))
            {
                return Results.Ok(new { ShortUrl = $"http://localhost:5000/s/{shortcode}" });
            }
            else
            {
                return Results.BadRequest();
            }

        }

        private static async Task<IResult> ShortcodeEndpoint(string shortcode, ClickService clickservice, UserService userservice, 
            HttpContext context, ILinkRepository linkrep, IUserRepository userep, IMemoryCache cache)
        {
            var currentLink = await linkrep.GetLongUrlAsync(shortcode);
            if (currentLink == null)
            {
                return Results.NotFound();
            }
            else
            {
                Console.WriteLine($"[Запрос] TraceId: {context.TraceIdentifier}");
                var userAgent = context.Request.Headers.UserAgent.ToString();

                if (cache.TryGetValue(userAgent, out DeviceDetector result) == false)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                    .SetPriority(CacheItemPriority.High);

                    var headers = context.Request.Headers.ToDictionary(
                        a => a.Key,
                        a => a.Value.ToArray().FirstOrDefault());


                    var clientHints = ClientHints.Factory(headers);
                    var uainfo = new DeviceDetector(userAgent, clientHints);
                    uainfo.Parse();
                    cache.Set(userAgent, uainfo, cacheOptions);
                    result = uainfo;
                }
                if (result == null)
                {
                    return Results.NotFound();
                }

                Click clickinfo = new Click
                {
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    Device = result.GetDeviceName(),
                    OS = result.GetOs().Match.Family.ToString() + " " + result.GetOs().Match.Version,
                    Resolution = null, // JS
                    Referer = context.Request.Headers.Referer,
                    Language = context.Request.Headers.AcceptLanguage.ToString(),
                    Browser = result.GetBrowserClient().Match.Family + " " + result.GetBrowserClient().Match.Version,
                    TimeZone = null, // JS
                    DeviceLocalTime = null // JS
                };

                await clickservice.CollectionInfoForClickAsync(currentLink.Id, clickinfo);

                string encodedLongUrl = WebUtility.UrlEncode(currentLink.LongUrl);
                string encodedShortcode = WebUtility.UrlEncode(shortcode);

                string redirectUrl = $"/redirect.html?url={encodedLongUrl}&code={encodedShortcode}";
                return Results.Redirect(redirectUrl);
            }
        }
    }
}
