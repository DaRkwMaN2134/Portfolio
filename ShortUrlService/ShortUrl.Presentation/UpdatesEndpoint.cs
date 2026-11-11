using DeviceDetectorNET;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;

namespace ShortUrl.Presentation
{
    public static class UpdatesEndpoint
    {
        public static void MapUpdateEndpoints(this WebApplication app)
        {
            app.MapPost("/api/click/update", ClickUpdateEndpoint);
            app.MapPost("/api/user/update", UserUpdateEndpoint);
        }

        private static async Task<IResult> ClickUpdateEndpoint(ClickUpdateRequest data, ClickService clickservice, ILinkRepository linkrep, IUserRepository userep)
        {
            var linkid = await linkrep.GetLinkIdByShortCodeAsync(data.Shortcode);

            if (linkid != null)
            {
                Console.WriteLine($"[ТУТ МЕГА ССЫЛКА]  {linkid}");
                await clickservice.UpdateExternalInfoForClick(linkid.Value, data.Resolution, data.TimeZone, data.DeviceLocalTime);
                return Results.Ok();
            }
            else
            {
                Console.WriteLine($"[МЕГА ССЫЛКА НЕ НАЙДЕНА]");
                return Results.NotFound();
            }
        }

        private static async Task<IResult> UserUpdateEndpoint(UserUpdateRequest data, IUserRepository userep, HttpContext context)
        {
            if (context.Items["DeviceId"] == null) return Results.BadRequest();

            var deviceid = context.Items["DeviceId"].ToString();
            var currentUser = await userep.GetByDeviceIdAsync(deviceid);
            if (currentUser != null)
            {
                Console.WriteLine($"[ТУТ МЕГА ЮЗЕР]  {currentUser.DeviceId}");
                var headers = context.Request.Headers.ToDictionary(
                    a => a.Key,
                    a => a.Value.ToArray().FirstOrDefault());
                var clientHints = ClientHints.Factory(headers);
                var uainfo = new DeviceDetector(context.Request.Headers.UserAgent.ToString(), clientHints);
                uainfo.Parse();

                currentUser.Resolution = data.Resolution;
                currentUser.OS = uainfo.GetOs().Match.Family.ToString() + " " + uainfo.GetOs().Match.Version; ;
                currentUser.Device = uainfo.GetDeviceName();
                currentUser.IpAddress = context.Connection.RemoteIpAddress?.ToString();
                await userep.UpdateUserAsync(currentUser);
                return Results.Ok();
            }
            else
            {
                Console.WriteLine($"[МЕГА ЮЗЕР НЕ НАЙДЕН]");
                return Results.NotFound();
            }
        }
    }
}
