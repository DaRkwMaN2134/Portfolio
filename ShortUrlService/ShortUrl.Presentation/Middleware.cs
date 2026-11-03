using ShortUrl.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Presentation
{
    public class Middleware
    {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserRepository userrep)
        {
            context.Response.Headers["Accept-CH"] = "Sec-CH-UA-Model, Sec-CH-UA-Platform-Version, Sec-CH-UA-Full-Version-List";
            context.Response.Headers["Critical-CH"] = "Sec-CH-UA-Model, Sec-CH-UA-Platform-Version, Sec-CH-UA-Full-Version-List";

            string? cookie = context.Request.Cookies["DeviceId"];
            if (cookie != null)
            {
                if (Guid.TryParse(cookie, out _) == true)
                {
                    var user = await userrep.GetUserAsync(cookie, null);
                    if (user == null)
                    {
                        await userrep.CreateUserAsync(cookie);
                    }
                    context.Items.Add("DeviceId", cookie);
                    await _next(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    Console.WriteLine("[DEVICEID БЫЛ ПОДМЕНЕН]");
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}
