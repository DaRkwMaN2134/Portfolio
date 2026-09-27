using ConfigurationLibrary;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;

namespace ParserBot
{
    public partial class Bot
    {
        async Task schedule_edit_CommandAsync(ITelegramBotClient botClient, long chatId, string messageText)
        {
            if (int.TryParse(messageText, out int interval) == false)
            {
                await botClient.SendMessage(chatId, $"Ошибка. Введите число");
                return;
            }
            else
            {
                _config.editIntervalLoadConfiguration(interval);
                await botClient.SendMessage(chatId, $"✅ Интервал обновлён: {interval} минут.");
            }
            _stateManager.RemoveUserState(chatId);

        }


        async Task status_CommandAsync(ITelegramBotClient botClient, long chatId)
        {

            await botClient.SendMessage(chatId, $"Статус расписания:{_isScheduleEnabled}");
            await botClient.SendMessage(chatId, $"Статус расписания:{_config.IntervalLoadConfiguration()}");
            await botClient.SendMessage(chatId, $"Последнее количество товаров:{_lastRunCount}");
            if(_lastRunTime == new DateTime(1, 1, 1, 0, 0, 0))
            {
                await botClient.SendMessage(chatId, "Никогда");
            }
            await botClient.SendMessage(chatId, $"🕒Последний запуск::{_lastRunTime.ToString("HH:mm:ss dd.MM.yyyy")}");

        }

        async Task schedule_on_CommandAsync(ITelegramBotClient botClient, long chatId)
        {
            var interval = TimeSpan.FromMinutes(_config.IntervalLoadConfiguration());

            if (_isScheduleEnabled)
            {
                await botClient.SendMessage(chatId, "⚠️ Расписание уже включено.");
                return;
            }
            _isScheduleEnabled = true;
            _scheduleCts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                using var timer = new PeriodicTimer(interval);
                while (_isScheduleEnabled)
                {
                    try
                    {
                        await timer.WaitForNextTickAsync(_scheduleCts.Token);
                        await ParserCommandAsync(botClient, chatId);
                        _lastRunTime = DateTime.Now;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync("Произошла ошибка", ex);
                    }
                }
                _isScheduleEnabled = false;
                _scheduleCts?.Cancel();
                _scheduleCts?.Dispose();
            });
        }

        async Task schedule_off_CommandAsync(ITelegramBotClient botClient, long chatId)
        {
            if (!_isScheduleEnabled)
            {
                await botClient.SendMessage(chatId, "⚠️ Расписание уже выключено.");

                return;
            }

            _isScheduleEnabled = false;
            _scheduleCts?.Cancel();
            _scheduleCts?.Dispose();
            _scheduleCts = null;
            await botClient.SendMessage(chatId, "⏹ Расписание отключается...");
        }
    }
}
