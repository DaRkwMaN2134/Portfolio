using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<LinkService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


var dbContext = app.Services.GetRequiredService<AppDbContext>();
var userep = new UserRepository(dbContext);



app.MapGet("/s/{shortcode}", async (string shortcode, LinkService linkservice) =>
{
    var longurl = await linkservice.RedirectLinkAsync(shortcode);
    if (longurl == null)
    {
        return Results.NotFound();
    }
    return Results.Redirect(longurl);
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


app.MapGet("/b/buffer", async (LinkService linkservice, IUserRepository userrep) =>
{
    var user = await userrep.GetUserAsync("123d", null); //сделать проверку на null
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