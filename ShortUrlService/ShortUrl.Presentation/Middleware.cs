using ShortUrl.Application.Services;
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

        public async Task InvokeAsync(HttpContext context, UserService userservice)
        {
            context.Response.Headers["Accept-CH"] = "Sec-CH-UA-Model, Sec-CH-UA-Platform-Version, Sec-CH-UA-Full-Version-List";
            context.Response.Headers["Critical-CH"] = "Sec-CH-UA-Model, Sec-CH-UA-Platform-Version, Sec-CH-UA-Full-Version-List";

            string? deviceid = context.Request.Cookies["DeviceId"];
            if (deviceid != null)
            {
                if (Guid.TryParse(deviceid, out _) == true)
                {
                    var user = await userservice.AnonymousUserLoginAsync(deviceid);

                    if (user == null)
                    {
                        context.Response.StatusCode = 400; 
                        return;
                    }

                    context.Items["DeviceId"] = deviceid;
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
