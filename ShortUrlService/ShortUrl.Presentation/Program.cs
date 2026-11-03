using DeviceDetectorNET;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using ShortUrl.Infrastructure;
using ShortUrl.Infrastructure.Repository;
using ShortUrl.Presentation;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using UAParser;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
        b => b.MigrationsAssembly("ShortUrl.Infrastructure")
    ));


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILinkRepository, LinkRepository>();
builder.Services.AddScoped<IClickRepository, ClickRepository>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<ClickService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();

app.UseMiddleware<Middleware>();

app.MapGet("/s/{shortcode}", async (string shortcode, ClickService clickservice, UserService userservice, HttpContext context, ILinkRepository linkrep, IUserRepository userep) =>
{
    var currentLink = await linkrep.GetLongUrlAsync(shortcode);  // потом создать систему создания deviceid каждому юзеру
    if (currentLink == null)
    {
        return Results.NotFound();
    }
    else
    {
        Console.WriteLine($"[Запрос] TraceId: {context.TraceIdentifier}");
        var parser = Parser.GetDefault();
        var headers = context.Request.Headers.ToDictionary(
            a => a.Key,
            a => a.Value.ToArray().FirstOrDefault());

        var clientHints = ClientHints.Factory(headers);


        var uainfo = new DeviceDetector(context.Request.Headers.UserAgent.ToString(), clientHints);
        uainfo.Parse();


        Click clickinfo = new Click
        {
            CreatedAt = DateTime.UtcNow,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Device = uainfo.GetDeviceName(),
            OS = uainfo.GetOs().Match.Family.ToString() + " " + uainfo.GetOs().Match.Version,
            Resolution = null, // JS
            Referer = context.Request.Headers.Referer,
            Language = context.Request.Headers.AcceptLanguage.ToString(),
            Browser = uainfo.GetBrowserClient().Match.Family + " " + uainfo.GetBrowserClient().Match.Version,
            TimeZone = null, // JS
            DeviceLocalTime = null // JS
        };
        await clickservice.CollectionInfoForClickAsync(currentLink.Id, clickinfo);

        string encodedLongUrl = WebUtility.UrlEncode(currentLink.LongUrl);
        string encodedShortcode = WebUtility.UrlEncode(shortcode);

        string redirectUrl = $"/redirect.html?url={encodedLongUrl}&code={encodedShortcode}";
        return Results.Redirect(redirectUrl);

    }
});

app.MapPost("/shorten", async (UrlRequest request, LinkService linkservice, IUserRepository userrep, HttpContext context) =>
{

    var deviceid = context.Items["DeviceId"].ToString();
    Console.WriteLine($"[ТУТ МЕГА DEVICEID]  {deviceid}");
    var user = await userrep.GetUserAsync(deviceid, null);
    if (user == null)
    {
        return Results.NotFound();
    }

    var shortcode = await linkservice.ShortenLinkAsync(request.LongUrl, user);
    if (!string.IsNullOrEmpty(shortcode))
    {
        return Results.Ok(new { ShortUrl = $"http://localhost:5000/s/{shortcode}" });
    }
    else
    {
        return Results.BadRequest();
    }
});


app.MapGet("/b/buffer", async (LinkService linkservice, IUserRepository userrep, HttpContext context) =>
{
    var deviceid = context.Items["DeviceId"].ToString();
    var user = await userrep.GetUserAsync(deviceid, null); //сделать проверку на null
    var links = await userrep.GetUserBufferAsync(user);
    var result = links.Select(l => new
    {
        l.Id,
        l.CreatedAt,
        l.LongUrl,
        l.ShortCode,
        l.CreatorId
    });
    return Results.Ok(result);
});


app.MapGet("/shortcode/analytics", async (string shortcode, ClickService clickservice, ILinkRepository linkrep) =>
{
    var linkid = await linkrep.GetLongUrlAsync(shortcode);

    if (linkid == null) return Results.BadRequest();

    var clicks = await clickservice.ClickAnalitycsAsync(linkid.Id);
    return Results.Ok(clicks);

});



app.MapPost("/api/click/update", async (ClickUpdateRequest data, ClickService clickservice, ILinkRepository linkrep, IUserRepository userep) =>
{
    var currentLink = await linkrep.GetLongUrlAsync(data.Shortcode);
    Console.WriteLine($"[ТУТ МЕГА ССЫЛКА]  {currentLink.Id}");
    if (currentLink != null)
    {
        await clickservice.UpdateExternalInfoForClick(currentLink.Id, data.Resolution, data.TimeZone, data.DeviceLocalTime);
        return Results.Ok();
    }
    else
    {
        Console.WriteLine($"[МЕГА ССЫЛКА НЕ НАЙДЕНА]");
        return Results.NotFound();
    }
});

app.MapPost("/api/user/update", async (UserUpdateRequest data, IUserRepository userep, HttpContext context) =>
{
    var deviceid = context.Items["DeviceId"].ToString();
    var currentUser = await userep.GetUserAsync(deviceid, null);
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
});




app.Run();

public record UrlRequest(string LongUrl);
public record ClickUpdateRequest(string Shortcode, string Resolution, string TimeZone, DateTime? DeviceLocalTime);
public record UserUpdateRequest(string Resolution);