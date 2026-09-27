using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ParserBot
{
    public partial class Bot
    {
        async Task UpdateCategorySelectionKeyboard(ITelegramBotClient botClient, long chatId, int messageId)
        {
            var categoryList = _categoryNames;
            var selected = _stateManager.GetSelectedCategories(chatId);

            var buttons = categoryList.Select((name, index) =>
            {
                var displayName = selected.Contains(name) ? $"✅ {name}" : name;
                return new[] { InlineKeyboardButton.WithCallbackData(displayName, $"cat_{index}") };
            }).ToList();

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("✅ Запустить выбранные", "run_selected") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌ Отмена", "menu_back") });

            var keyboard = new InlineKeyboardMarkup(buttons);


            try
            {
                await botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup: keyboard);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Произошла ошибка", ex);
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.AnswerCallbackQuery(callbackQuery.Id);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Произошла ошибка", ex);
            }

            var chatId = callbackQuery.Message.Chat.Id;
            var messageId = callbackQuery.Message.MessageId;
            var data = callbackQuery.Data;

            if (_stateStorage.IsAuthorized(chatId) == false)
            {
                await botClient.SendMessage(chatId, "Сначала введите пароль");
                return;
            }


            if (data.StartsWith("cat_"))
            {
                var index = int.Parse(data.Split('_')[1]);
                var categoryName = _categoryNames[index];

                _stateManager.ToggleCategory(chatId, categoryName);

                var msgId = _stateManager.GetCategoryMessageId(chatId) ?? callbackQuery.Message.MessageId;
                await UpdateCategorySelectionKeyboard(botClient, chatId, msgId);
                return;
            }
            else
            {
                try
                {
                    switch (data)
                    {
                        case "menu_run" or "run_all":
                            if (_isParsing)
                            {
                                await botClient.EditMessageText(chatId, messageId, "⚠️ Парсинг уже выполняется", replyMarkup: null);
                                await _logger.LogAsync($"Парсинг уже выполняется, подождите");
                                return;
                            }
                            await botClient.SendMessage(chatId, "Начался парсинг карточек", cancellationToken: cancellationToken);
                            await _logger.LogAsync($"Начался парсинг карточек");

                            _ = Task.Run(() => ParserCommandAsync(botClient, chatId));
                            await botClient.EditMessageText(chatId, messageId, "✅ Парсинг запущен", replyMarkup: null);
                            await _stateStorage.ClearActiveMenu();
                            await _logger.LogAsync($"Парсинг запущен в фоне...");
                            break;
                        case "menu_categories" or "select_categories":
                            await ShowCategorySelection(botClient, chatId, messageId);
                            break;
                        case "menu_schedule":
                            await botClient.EditMessageText(chatId, messageId, "⏰ Расписание", replyMarkup: BuildScheduleMenuKeyboard());
                            break;
                        case "menu_status":
                            await botClient.EditMessageText(chatId, messageId, "✅ Статус открыт", replyMarkup: null);
                            await status_CommandAsync(botClient, chatId);
                            await _stateStorage.ClearActiveMenu();
                            break;
                        case "menu_stop":
                            _parserCts?.Cancel();
                            await botClient.EditMessageText(chatId, messageId, "⏹ Парсинг остановлен", replyMarkup: null);
                            await _stateStorage.ClearActiveMenu();
                            break;
                        case "menu_help":
                            await botClient.EditMessageText(chatId, messageId, "✅ Доступные команды показаны", replyMarkup: null);
                            await botClient.SendMessage(chatId, "Доступные команды: ...");
                            await _stateStorage.ClearActiveMenu();
                            break;
                        case "schedule_on":
                            await botClient.EditMessageText(chatId, messageId, "✅ Расписание включено", replyMarkup: null);
                            await schedule_on_CommandAsync(botClient, chatId);
                            await _stateStorage.ClearActiveMenu();
                            break;
                        case "schedule_off":
                            await botClient.EditMessageText(chatId, messageId, "⏹ Расписание выключено", replyMarkup: null);
                            await schedule_off_CommandAsync(botClient, chatId);
                            await _stateStorage.ClearActiveMenu();
                            break;
                        case "schedule_edit":
                            _stateManager.SetUserState(chatId, "awaiting_interval");
                            var cancelKeyboard = new InlineKeyboardMarkup(new[]
                            {
                            new[] { InlineKeyboardButton.WithCallbackData("❌ Меню расписания", "menu_schedule_back") },
                            new[] { InlineKeyboardButton.WithCallbackData("❌ Главное меню ", "menu_back") }
                            });
                            await botClient.EditMessageText(chatId, messageId, "Введите интервал в чат", replyMarkup: cancelKeyboard);
                            await _stateStorage.ClearActiveMenu();
                            break;
                        case "menu_back":
                            _stateManager.RemoveUserState(chatId);
                            _stateManager.ClearSelectedCategories(chatId);
                            await botClient.EditMessageText(chatId, messageId, "Выберите действие:", replyMarkup: BuildMainMenuKeyboard());
                            break;
                        case "menu_schedule_back":
                            _stateManager.RemoveUserState(chatId);
                            await botClient.EditMessageText(chatId, messageId, "Выберите действие:", replyMarkup: BuildScheduleMenuKeyboard());
                            break;

                        case "run_selected":
                            if (_stateManager.GetSelectedCategories(chatId).Count == 0)
                            {
                                await botClient.EditMessageText(chatId, messageId, "⚠️ Вы не выбрали ни одной категории", replyMarkup: null);
                                return;
                            }
                            await botClient.EditMessageText(chatId, messageId, "✅ Парсинг запущен", replyMarkup: null);
                            var selectedUrls = (_stateManager.GetSelectedCategories(chatId)
                                .Select(name => _categoryToUrl[name])
                                .ToList());
                            _ = Task.Run(() => ParserCommandAsync(botClient, chatId, selectedUrls));
                            _stateManager.ClearSelectedCategories(chatId);
                            await _stateStorage.ClearActiveMenu();
                            break;
                        
                        case "stop_parser":
                            _parserCts?.Cancel();
                            await botClient.EditMessageText(chatId, messageId, "⏹ Парсинг остановлен", replyMarkup: null);
                            break;

                        default:
                            await botClient.SendMessage(chatId, "Неизвестное действие");
                            break;
                    }
                }
                catch(Exception ex)
                {
                    await _logger.LogErrorAsync("Произошла ошибка", ex);
                }
            }
        }
        async Task ShowCategorySelection(ITelegramBotClient botClient, long chatId, int menuMessageId)
        {
            _stateManager.ClearSelectedCategories(chatId);
            await _stateStorage.ClearActiveMenu();
            await botClient.DeleteMessage(chatId, menuMessageId);

            var buttons = _categoryNames.Select((name, index) =>
                new[] { InlineKeyboardButton.WithCallbackData(name, $"cat_{index}") }
            ).ToList();
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("✅ Запустить выбранные", "run_selected") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌ Отмена", "menu_back") });
            var keyboard = new InlineKeyboardMarkup(buttons);
            var message = await botClient.SendMessage(chatId, "Выберите категории (нажмите для выбора/отмены):", replyMarkup: keyboard);
            _stateManager.SetCategoryMessageId(chatId, message.MessageId);
        }

        async Task ShowMainMenu(ITelegramBotClient botClient, long chatId)
        {
            int? activeMenuId = _stateStorage.GetActiveMenuId();
            Message lastMenuMessageId = null;
            if (activeMenuId != null)
            {
                try
                {
                    await botClient.DeleteMessage(chatId, activeMenuId.Value);
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync("Произошла ошибка", ex);
                }
            }
            try
            {
                lastMenuMessageId = await botClient.SendMessage(chatId, "Выберите действие", replyMarkup: BuildMainMenuKeyboard());
            }
            catch(Exception ex)
            {
                await _logger.LogErrorAsync($"Критическая ошибка сохранения: {ex.Message}");
            }
            activeMenuId = lastMenuMessageId.MessageId;
            await _stateStorage.SetActiveMenu(activeMenuId.Value);
        }
    }
}
