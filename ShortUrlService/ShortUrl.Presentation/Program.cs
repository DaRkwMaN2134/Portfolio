using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using ShortUrl.Infrastructure;
using ShortUrl.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


var dbContext = app.Services.GetRequiredService<AppDbContext>();
var userep = new UserRepository(dbContext);
var linkrep = new LinkRepository(dbContext);
var clickrep = new ClickRepository(dbContext);


app.MapGet("/s/{shortcode}", async (string shortcode, ClickService clickservice, HttpContext context) =>
{
    var currentLink = await linkrep.GetLongUrlAsync(shortcode);  // потом создать систему создания deviceid каждому юзеру
    if (currentLink == null)
    {
        return Results.NotFound();
    }

    Click click = new Click //потом следует доработка методов поиска аналитики
    {
        CreatedAt = DateTime.UtcNow,
        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
        Device = "Desktop",
        OS = context.Request.Headers.UserAgent.ToString(),
        Resolution = "2560x1440",
        Referer = "https://github.com",
        Language = context.Request.Headers.AcceptLanguage.ToString(),
        Browser = context.Request.Headers.UserAgent.ToString(),
        TimeZone = "Europe/Moscow",
        DeviceLocalTime = DateTime.UtcNow
    };

    await clickservice.CollectionInfoForClickAsync(currentLink.Id, click);
    return Results.Redirect(currentLink.LongUrl);
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