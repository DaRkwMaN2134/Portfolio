using DataLibrary;
using DTOLibrary;
using Telegram.Bot;
using Telegram.Bot.Types;
using static ConfigurationLibrary.Interfaces;

namespace BotStart
{

    public partial class Bot
    {
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            long chatId = 0;
            try
            {
                if (update.MyChatMember is { } mychatmember)
                {
                    return;
                }
                if (update.CallbackQuery is { } callbackQuery)
                {
                    await HandleCallbackQueryAsync(callbackQuery, cancellationToken);
                    return;
                }
                if (update.Message is not { } message || message.Text is not { } messageText)
                {
                    return;
                }

                chatId = message.Chat.Id;
                using (var scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    var botWrite = scope.ServiceProvider.GetRequiredService<IBotWrite>();
                    var user = await botWrite.GetUserAsync(chatId);
                    if (user != null && user.IsBlocked)
                    {
                        await botClient.SendMessage(chatId, "Вы заблокированы");
                        return;
                    }

                    if (messageText.StartsWith("/start"))
                    {
                        var registration = new UserRegistrationDto
                        {
                            ChatId = message.Chat.Id,
                            Username = message.Chat.Username,
                            FirstName = message.Chat.FirstName
                        };

                        long refererrId = 0;
                        await botWrite.CheckOrCreateUserAsync(registration);
                        if (messageText.Contains("ref_"))
                        {
                            string[]? parse = messageText.Split('_');
                            if (long.TryParse(parse[1], out long result))
                            {
                                refererrId = result;
                            }

                            if (refererrId == chatId)
                            {
                                await _botClient.SendMessage(chatId, "Нельзя пригласить себя");
                                return;
                            }
                            else if (refererrId == 0)
                            {
                                return;
                            }
                            else
                            {
                                var referral = await botWrite.GetReferrerAsync(chatId, refererrId);
                                if (referral.Status == ReferralStatus.Pending)
                                {
                                    await botWrite.ConfirmReferralAsync(referral);
                                }
                            }

                        }
                    }
                }
                await ShowMainMenu(chatId);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке update от {ChatId}", chatId);
            }
        }
    }
}
