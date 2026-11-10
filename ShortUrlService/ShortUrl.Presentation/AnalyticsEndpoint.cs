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

        private static async Task<IResult> AnalitycsEndpoint(string shortcode, ClickService clickservice, LinkService linkservice)
        {
            if (shortcode == null) return Results.NotFound(); // добавил проверку на входные данные

            var linkid = await linkservice.GetIdFromLinkAsync(shortcode);

            if (linkid == null) return Results.BadRequest();

            var clicks = await clickservice.ClickAnalitycsAsync(linkid);

            if(clicks == null) return Results.NotFound(); // добавил проверку

            return Results.Ok(clicks);
        }
    }
}
