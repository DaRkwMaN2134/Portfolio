using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShortUrl.Application.Interfaces;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using ShortUrl.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Infrastructure.BackgroundServices
{
    public class CleanupAnonymousUserService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanupAnonymousUserService> _logger;
        public CleanupAnonymousUserService(IServiceScopeFactory scopeFactory, ILogger<CleanupAnonymousUserService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateAsyncScope();
                    var userrep = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    await userrep.CleanupAnonymousUsersAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "[СЕРВИС CleanupAnonymousUserService] ОШИБКА АВТООЧИСТКИ");
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }

    public class CleanupDetectorCacheService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanupDetectorCacheService> _logger;
        public CleanupDetectorCacheService(IServiceScopeFactory scopeFactory, ILogger<CleanupDetectorCacheService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateAsyncScope();
                    var clientinfoparser = scope.ServiceProvider.GetRequiredService<IClientInfoParser>();
                    clientinfoparser.CleanupDetectorCache();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[СЕРВИС CleanupDetectorCacheService] ОШИБКА АВТООЧИСТКИ");
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
