using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using ShortUrl.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Infrastructure.BackgroundServices
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanupBackgroundService> _logger;
        public CleanupBackgroundService(IServiceScopeFactory scopeFactory, ILogger<CleanupBackgroundService> logger)
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
                    _logger.LogError(ex, "[МЕТОД ExecuteAsync] ОШИБКА АВТООЧИСТКИ");
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
