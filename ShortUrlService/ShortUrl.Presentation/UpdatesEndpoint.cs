using DeviceDetectorNET;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using UAParser;

namespace ShortUrl.Presentation
{
    public static class UpdatesEndpoint
    {
        public static void MapUpdateEndpoints(this WebApplication app)
        {
            app.MapPost("/api/click/update", ClickUpdateEndpoint);
            app.MapPost("/api/user/update", UserUpdateEndpoint);
        }

        private static async Task<IResult> ClickUpdateEndpoint(ClickUpdateRequest data, ClickService clickservice)
        {
            var update = await clickservice.ClickUpdateAsync(data);

            if(!update) return Results.NotFound();

            return Results.Ok();
        }

        private static async Task<IResult> UserUpdateEndpoint(UserService userservice, UserUpdateRequest data,  HttpContext context)
        {
            if (context.Items["DeviceId"] == null) return Results.BadRequest();
            var deviceid = context.Items["DeviceId"].ToString();
            
            var headers = context.Request.Headers.ToDictionary(
                a => a.Key,
                a => a.Value.ToArray().FirstOrDefault());
            

            var useragent = context.Request.Headers.UserAgent.ToString();
            var IpAddress = context.Connection.RemoteIpAddress?.ToString();

            var headersDTO = new HeadersDTO(
                useragent,
                headers,
                null
                );

            var update = await userservice.UserUpdateAsync(deviceid, headersDTO, data, IpAddress);

            if(!update) return Results.NotFound();

            return Results.Ok();
        }
    }
}
