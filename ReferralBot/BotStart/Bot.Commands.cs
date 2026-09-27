using Telegram.Bot;
using Telegram.Bot.Types;
using static ConfigurationLibrary.Interfaces;

namespace BotStart
{
    public partial class Bot
    {
        async Task ShowMainMenu(long chatId)
        {
            await _botClient.SendMessage(chatId, "Выберите действие", replyMarkup: BuildMainMenuKeyboard());
        }



        async Task ShowBalanceAsync(long chatId, IBotWrite botWrite, int messageId)
        {
            var user = await botWrite.GetUserAsync(chatId);
            if(user == null)
            {
                return;
            }
            await _botClient.EditMessageText(chatId, messageId, $"Ваш баланс равен: {user.Points}", replyMarkup: BuildBalanceKeyboard());
        }

        async Task ShowMyLinkAsync(long chatId, int messageId)
        {
            await _botClient.EditMessageText(chatId, messageId, $"Ваша личная ссылка: t.me/{_botUsername}?start=ref_{chatId}", replyMarkup: BuildLinkKeyboard());
        }

        async Task ShowServicesAsync(long chatId, int messageId)
        {
            await _botClient.EditMessageText(chatId, messageId, "{Здесь скоро появятся услуги. Следите за обновлениями}", replyMarkup: BuildServicesKeyboard());
        }

        async Task ShowHelpAsync(long chatId, int messageId)
        {
            await _botClient.EditMessageText(chatId, messageId, "{Если у вас возникли вопросы — напишите администратору: @админа нет", replyMarkup: BuildHelpKeyboard());
        }

        async Task ShowSettingsAsync(long chatId, int messageId)
        {
            await _botClient.EditMessageText(chatId, messageId, "{Здесь скоро появятся настройки.}", replyMarkup: BuildSettingsKeyboard());
        }

        async Task ShowMyReferralsAsync(long chatId, IBotWrite botWrite, int messageId)
        {
            var refsList = await botWrite.GetReferralsAsync(chatId);
            string answer = string.Empty;
            if (refsList.Any() == false)
            {
                await _botClient.EditMessageText(chatId, messageId, "Список пуст", replyMarkup: BuildReferralsKeyboard());
                return;
            }

            foreach (var refs in refsList)
            {
                answer += $"{refs.InvitedUser.Username ?? refs.InvitedUser.FirstName ?? "Без имени"} {refs.InvitedUser.RegisteredAt.ToString("dd.MM.yyyy")}\n";
            }
            await _botClient.EditMessageText(chatId, messageId, $"Ваши рефералы\n" + answer, replyMarkup: BuildReferralsKeyboard());
        }
    }
}