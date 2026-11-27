using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShortUrl.Application.Interfaces;
using ShortUrl.Application.Services;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using ShortUrl.Application.DTO;

namespace ShortUrl.Infrastructure.BackgroundServices
{
    public class ChannelSenderWorker<T> : BackgroundService
    {
        private readonly IBackgroundChannelTask<T> _task;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChannelSenderWorker<T>> _logger;

        public ChannelSenderWorker(IBackgroundChannelTask<T> task, IServiceScopeFactory scopeFactory, ILogger<ChannelSenderWorker<T>> logger)
        {
            _task = task;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var data in _task.ChannelReadAsync(stoppingToken))
                {
                    try
                    {
                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var handler = scope.ServiceProvider.GetRequiredService<IChannelHandler<T>>();
                        await handler.HandleAsync(data, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при фоновой обработке сообщения типа {Type}", typeof(T).Name);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Игнорируем отмену при штатном завершении работы хоста
            }
        }
    }
    public class ClickChannelHandler : IChannelHandler<RedirectDTO>
    {
        private readonly IClientInfoParser _infoParser;
        private readonly ClickService _clickService;
        private readonly ILogger<ClickChannelHandler> _logger;

        public ClickChannelHandler(IClientInfoParser infoParser, ClickService clickService, ILogger<ClickChannelHandler> logger)
        {
            _infoParser = infoParser;
            _clickService = clickService;
            _logger = logger;
        }

        public async Task HandleAsync(RedirectDTO redirectDTO, CancellationToken cancellationToken)
        {
            var info = await _infoParser.GetInfoAsync(redirectDTO.HeadersDTO);
            await _clickService.CollectionInfoForClickAsync(redirectDTO.click, info, redirectDTO.HeadersDTO.clientContext);
        }
    }
}
