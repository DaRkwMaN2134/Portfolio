using ConfigurationLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;

namespace ParserBot
{
    public class StateStorage
    {
        private bool _isAuthorized = false;
        private long _authorizedChatId = 0;
        private int _activeMenuMessageId = 0;
        private readonly string path = "state.json";
        private readonly string sample = "{\r\n  \"ChatId\": 0,\r\n  \"IsAuthorized\": false,\r\n  \"ActiveMenuMessageId\": 0 \r\n}";
        private readonly ILogger _logger;
        private static readonly SemaphoreSlim _fileLock = new(1, 1);
        public StateStorage(ILogger logger)
        {
            _logger = logger;
            LoadState();
        }
        public async Task LoadState()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (File.Exists(path) == true)
                {
                    try
                    {
                        string jsonString = File.ReadAllText(path);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        BotState json = JsonSerializer.Deserialize<BotState>(jsonString, options);
                        _authorizedChatId = json.ChatId;
                        _isAuthorized = json.IsAuthorized;
                        _activeMenuMessageId = json.ActiveMenuMessageId;
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync("Произошла ошибка", ex);
                    }
                }
                else
                {
                    File.Create(path).Close();
                    File.WriteAllText(path, sample);
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task SaveStateAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                BotState state = new BotState
                {
                    ChatId = _authorizedChatId,
                    IsAuthorized = _isAuthorized,
                    ActiveMenuMessageId = _activeMenuMessageId
                };

                string json = JsonSerializer.Serialize(state, options);
                await File.WriteAllTextAsync(path, json);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public bool IsAuthorized(long chatId)
        {
            return _isAuthorized && chatId == _authorizedChatId;
        }

        public async Task Authorize(long chatId)
        {
            _authorizedChatId = chatId;
            _isAuthorized = true;
            await SaveStateAsync();
        }

        public int GetActiveMenuId()
        {
            return _activeMenuMessageId;
        }

        public async Task SetActiveMenu(int activeMenuMessageId)
        {
            _activeMenuMessageId = activeMenuMessageId;
            await SaveStateAsync();
        }

        public async Task ClearActiveMenu()
        {
            _activeMenuMessageId = 0;
            await SaveStateAsync();
        }
    }
}
