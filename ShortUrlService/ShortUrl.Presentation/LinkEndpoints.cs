using DeviceDetectorNET;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using ShortUrl.Application.DTO;
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
            app.MapGet("/s/{shortcode}", RedirectEndpoint);
        }


        private static async Task<IResult> BufferEndpoint(LinkService linkservice, HttpContext context)
        {
            if (context.Items["DeviceId"] == null) return Results.BadRequest();
            var deviceid = context.Items["DeviceId"].ToString();


            var buffer = await linkservice.GetBufferAsync(deviceid); // вынести в BufferDTO/LinkDTO

            return Results.Ok(buffer);
        }

        private static async Task<IResult> ShortenEndpoint(UrlRequest request, LinkService linkservice, HttpContext context)
        {
            if (context.Items["DeviceId"] == null) return Results.BadRequest();
            var deviceid = context.Items["DeviceId"].ToString();

            var shortcode = await linkservice.GetShortenAsync(deviceid, request.LongUrl);
            if (string.IsNullOrEmpty(shortcode)) return Results.Problem();
            
            return Results.Ok(new { ShortUrl = $"http://localhost:5000/s/{shortcode}" });

        }

        private static async Task<IResult> RedirectEndpoint(string shortcode, LinkService linkservice, HttpContext context)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var headers = context.Request.Headers.ToDictionary(
                a => a.Key,
                a => a.Value.ToArray().FirstOrDefault());

            var clientContext = new ClientContext(
                context.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                context.Request.Headers.Referer.ToString(),
                context.Request.Headers.AcceptLanguage.ToString()
            );

            var redirectUrl = await linkservice.MakeRedirectAsync(shortcode, userAgent, headers, clientContext);
            if (string.IsNullOrEmpty(redirectUrl)) return Results.NotFound();
            return Results.Redirect(redirectUrl);
        }
    }
}
