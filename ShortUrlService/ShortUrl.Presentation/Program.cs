using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShortUrl.Infrastructure;
using ShortUrl.Domain;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Infrastructure.Repository;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
            b => b.MigrationsAssembly("ShortUrl.Infrastructure")
    ));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILinkRepository, LinkRepository>();


var app = builder.Build();

/* app.Services.CreateScope();
var dbContext = app.Services.GetRequiredService<AppDbContext>();
var userep = new UserRepository(dbContext);
var linkrep = new LinkRepository(dbContext);
await userep.CreateUserAsync("123e");
var user = await userep.GetUserAsync("123e", null, null);
await linkrep.CreateLinkAsync("хувалв", "ху", user); */


app.Run();
