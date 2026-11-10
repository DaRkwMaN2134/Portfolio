using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Presentation
{
    public static class AnalyticsEndpoint
    {
        public static void MapAnalyticsEndpoints(this WebApplication app)
        {
            app.MapGet("/shortcode/analytics", AnalitycsEndpoint);
        }

        private static async Task<IResult> AnalitycsEndpoint(string shortcode, LinkService linkservice)
        {
            if (shortcode == null) return Results.NotFound(); // добавил проверку на входные данные

            var analytics = await linkservice.GetAnalyticsAsync(shortcode);

            if (analytics == null) return Results.NotFound();

            return Results.Ok(analytics);
        }
    }
}
