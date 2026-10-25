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

        public async Task InvokeAsync(HttpContext context, IUserRepository userRep)
        {
            string? cookie = context.Request.Cookies["DeviceId"];
            if (cookie == null)
            {
                var value = Guid.NewGuid().ToString();
                await userRep.CreateUserAsync(value);
                context.Response.Cookies.Append("DeviceId", value);
                context.Items.Add("DeviceId", value);
                await _next(context);
            }
            else
            {
                context.Items.Add("DeviceId", cookie);
                await _next(context);
            }
        }
    }
}
