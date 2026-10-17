using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShortUrl.Infrastructure;
using ShortUrl.Domain;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Infrastructure.Repository;
using ShortUrl.Application.Services;
using System.Linq.Expressions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
            b => b.MigrationsAssembly("ShortUrl.Infrastructure")
    ));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILinkRepository, LinkRepository>();
builder.Services.AddScoped<LinkService>();



var app = builder.Build();

app.Services.CreateScope();
var dbContext = app.Services.GetRequiredService<AppDbContext>();
var linkrep = new LinkRepository(dbContext);
var service = new LinkService(linkrep);
var url = await service.CreateShortenLinkAsync("https://web.telegram.org/k/");
Console.WriteLine(url);  



app.Run();
