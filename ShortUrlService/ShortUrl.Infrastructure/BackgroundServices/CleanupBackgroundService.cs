using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShortUrl.Infrastructure.BackgroundServices
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public CleanupBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
                    Console.WriteLine($"Ошибка очистки: {ex.Message}");
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
