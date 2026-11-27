using DeviceDetectorNET;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Serilog;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Interfaces;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using ShortUrl.Infrastructure;
using ShortUrl.Infrastructure.BackgroundServices;
using ShortUrl.Infrastructure.Repository;
using ShortUrl.Infrastructure.Services;
using ShortUrl.Presentation;
using ShortUrl.Presentation.ExceptionHandlers;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using UAParser;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ShortUrl_";
});



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
        b => b.MigrationsAssembly("ShortUrl.Infrastructure")
    ));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton(typeof(IBackgroundChannelTask<>), typeof(ChannelControl<>));
builder.Services.AddScoped<IChannelHandler<RedirectDTO>, ClickChannelHandler>();
builder.Services.AddHostedService<ChannelSenderWorker<RedirectDTO>>();

builder.Services.AddProblemDetails();


builder.Services.AddScoped<IClientInfoParser, ClientInfoParser>();
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
builder.Services.AddHostedService<CleanupAnonymousUserService>();
builder.Services.AddHostedService<CleanupDetectorCacheService>();
var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthorization();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.MapControllers();
app.UseStaticFiles();

app.UseMiddleware<Middleware>();
app.MapLinkEndpoints();
app.MapAnalyticsEndpoints();
app.MapUpdateEndpoints();


app.Run();

public record UrlRequest(string LongUrl);