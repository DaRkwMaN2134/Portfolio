using Telegram.Bot;
using Telegram.Bot.Types;
using static ConfigurationLibrary.Interfaces;

namespace BotStart
{
    public partial class Bot
    {
        async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if(callbackQuery.Message == null)
            {
                await _botClient.AnswerCallbackQuery(callbackQuery.Id);
                return;
            }

            var chatId = callbackQuery.Message.Chat.Id;
            var messageId = callbackQuery.Message.MessageId;
            var data = callbackQuery.Data;

            using var scope = _serviceScopeFactory.CreateAsyncScope();
            var botWrite = scope.ServiceProvider.GetRequiredService<IBotWrite>();

            switch (data)
            {
                case "balance":
                    await ShowBalanceAsync(chatId, botWrite, messageId);
                    break;

                case "my_link":
                    await ShowMyLinkAsync(chatId, messageId);
                    break;

                case "services":
                    await ShowServicesAsync(chatId, messageId);
                    break;

                case "help":
                    await ShowHelpAsync(chatId, messageId);
                    break;

                case "settings":
                    await ShowSettingsAsync(chatId, messageId);
                    break;

                case "my_refs":
                    await ShowMyReferralsAsync(chatId, botWrite, messageId);
                    break;
                case "menu_back":
                    await _botClient.EditMessageText(chatId, messageId, "Выберите действие:", replyMarkup: BuildMainMenuKeyboard());
                    break;
            }
            await _botClient.AnswerCallbackQuery(callbackQuery.Id);
        }
    }
}
