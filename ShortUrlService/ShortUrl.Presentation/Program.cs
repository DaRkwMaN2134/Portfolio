using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using ShortUrl.Infrastructure;
using ShortUrl.Infrastructure.Repository;
using System.Net;
using System.Text.Json;

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



app.MapGet("/s/{shortcode}", async (string shortcode, ClickService clickservice, HttpContext context, ILinkRepository linkrep) =>
{
    var currentLink = await linkrep.GetLongUrlAsync(shortcode);  // потом создать систему создания deviceid каждому юзеру

    if (currentLink == null)
    {
        return Results.NotFound();
    }
    else
    {
        Console.WriteLine($"[Запрос] TraceId: {context.TraceIdentifier}");

        Click clickinfo = new Click //потом следует доработка методов поиска аналитики
        {
            CreatedAt = DateTime.UtcNow,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Device = context.Request.Headers.UserAgent.ToString(), // надо распарсить
            OS = context.Request.Headers.UserAgent.ToString(), // надо распарсить
            Resolution = "2560x1440", // JS
            Referer = context.Request.Headers.Referer,
            Language = context.Request.Headers.AcceptLanguage.ToString(),
            Browser = context.Request.Headers.UserAgent.ToString(), // надо распарсить  
            TimeZone = "Europe/Moscow", // JS
            DeviceLocalTime = DateTime.UtcNow // JS
        };
        await clickservice.CollectionInfoForClickAsync(currentLink.Id, clickinfo);
        // ниже html + js нейрокод, решение через отправку чистого html для избежания дублирования в бд из-за повторного запроса
        var safeShortcode = JsonSerializer.Serialize(shortcode);
        var safeLongUrl = JsonSerializer.Serialize(currentLink.LongUrl);

        return Results.Content($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Перенаправление...</title>
    <style>
        body {{ font-family: sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; background: #f5f5f5; }}
        .loader {{ border: 4px solid #f3f3f3; border-top: 4px solid #3498db; border-radius: 50%; width: 30px; height: 30px; animation: spin 1s linear infinite; }}
        @keyframes spin {{ 0% {{ transform: rotate(0deg); }} 100% {{ transform: rotate(360deg); }} }}
    </style>
</head>
<body>
    <div class='loader'></div>
    <script>
        const data = {{
            shortCode: {safeShortcode},
            resolution: window.screen.width + 'x' + window.screen.height,
            timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
            deviceLocalTime: new Date().toISOString()
        }};

        const blob = new Blob([JSON.stringify(data)], {{ type: 'application/json' }});
        navigator.sendBeacon('/api/click/update', blob);

        window.location.replace({safeLongUrl});
    </script>
</body>
</html>", "text/html");
    }
});

app.MapPost("/api/click/update", async (ClickUpdateRequest data, ClickService clickservice, ILinkRepository linkrep) =>
{
    var currentLink = await linkrep.GetLongUrlAsync(data.Shortcode);
    if (currentLink != null)
    {
        Console.WriteLine($"[ТУТ МЕГА ССЫЛКА]  {currentLink.Id}");
        await clickservice.UpdateExternalInfoForClick(currentLink.Id, data.Resolution, data.TimeZone, data.DeviceLocalTime);
        return Results.Ok();
    }
    else
    {
        Console.WriteLine($"[МЕГА ССЫЛКА НЕ НАЙДЕНА]");
        return Results.NotFound();
    }
});

app.MapPost("/shorten", async (UrlRequest request, LinkService linkservice, IUserRepository userrep) =>
{
    var user = await userrep.GetUserAsync("123e", null);
    if (user == null)
    {
        return Results.NotFound();
    }

    var shortcode = await linkservice.ShortenLinkAsync(request.LongUrl, user);

    if (!string.IsNullOrEmpty(shortcode))
    {
        return Results.Ok(new { ShortUrl = $"http://localhost:5000/s/{shortcode}" });
    }

    return Results.BadRequest();
});


app.MapGet("/b/buffer", async (string deviceid, LinkService linkservice, IUserRepository userrep) =>
{
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


app.Run();

public record UrlRequest(string LongUrl);
public record ClickUpdateRequest(string Shortcode, string Resolution, string TimeZone, DateTime? DeviceLocalTime);