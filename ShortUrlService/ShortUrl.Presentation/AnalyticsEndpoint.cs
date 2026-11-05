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

        private static async Task<IResult> AnalitycsEndpoint(string shortcode, ClickService clickservice, ILinkRepository linkrep)
        {
            var linkid = await linkrep.GetLongUrlAsync(shortcode);

            if (linkid == null) return Results.BadRequest();

            var clicks = await clickservice.ClickAnalitycsAsync(linkid.Id);
            return Results.Ok(clicks);
        }
    }
}
