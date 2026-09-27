using ConfigurationLibrary;
using DataLibrary;
using FileIOLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using ParserLibrary;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ParserBot
{
    public partial class Bot
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IHtmlParser _htmlParser;
        private readonly IExcelOutput _excelOutput;
        private readonly string _botToken;
        private readonly Configuration _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly StateManager _stateManager;
        private readonly StateStorage _stateStorage;
        public Bot(ILogger logger, IHttpClient httpClient, IHtmlParser htmlParser, IExcelOutput excelOutput, Configuration config, IServiceScopeFactory serviceScopeFactory, StateManager stateManager, StateStorage stateStorage)
        {
            _logger = logger;
            _httpClient = httpClient;
            _htmlParser = htmlParser;
            _excelOutput = excelOutput;
            _botToken = config.TokenLoadConfiguration();
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;
            _stateManager = stateManager;
            _stateStorage = stateStorage;
        }

        private static CancellationTokenSource? _scheduleCts = null;
        private static CancellationTokenSource? _parserCts = null;
        private static bool _isParsing = false;
        private static bool _isScheduleEnabled = false;
        private static DateTime _lastRunTime;
        private static int _lastRunCount;
        private readonly List<string> _categoryNames = new()
        {
            "Душевые трапы",
            "Дозаторы",
            "По сериям",
            "Полотенцесушители",
            "Запчасти",
            "Аксессуары для смесителей",
            "Кухонные мойки",
            "Аксессуары для ванной"
        };
        private readonly Dictionary<string, string> _categoryToUrl = new()
        {
            { "Душевые трапы",            "https://raglo.ru/catalog/dushevye-trapy/" },
            { "Дозаторы",                 "https://raglo.ru/catalog/kukhnya/dozatory-/" },
            { "По сериям",                "https://raglo.ru/catalog/po-seriyam/" },
            { "Полотенцесушители",        "https://raglo.ru/catalog/polotentsesushiteli/" },
            { "Запчасти",                 "https://raglo.ru/catalog/splenka/zapchasti-s/" },
            { "Аксессуары для смесителей","https://raglo.ru/catalog/aksessuary-dlya-smesiteley/" },
            { "Кухонные мойки",           "https://raglo.ru/catalog/kukhonnye-moyki/" },
            { "Аксессуары для ванной",    "https://raglo.ru/catalog/aksessuary-dlya-vannoy-komnaty/" }
        };


        static public async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClient, Http_Client>();
            services.AddSingleton<IHtmlParser, Html_Parser>();
            services.AddSingleton<IExcelOutput, Excel_Output>();
            services.AddSingleton<ILogger, FileLogger>();
            services.AddSingleton<Configuration>();
            services.AddScoped<AppDbContext>();
            services.AddScoped<IBotOutput, BotDataOutput>();
            services.AddSingleton<StateManager>();
            services.AddSingleton<StateStorage>();
            services.AddSingleton<Bot>();


            var serviceProvider = services.BuildServiceProvider();

            var bot = serviceProvider.GetRequiredService<Bot>();
            await bot.runBotAsync();
        }
        async Task runBotAsync()
        {
            var botClient = new TelegramBotClient(_botToken);
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            try
            {
                await botClient.DropPendingUpdates();
            }
            catch(Exception ex)
            {
                await _logger.LogErrorAsync("Произошла ошибка", ex);
            }

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            try
            {
                User me = await botClient.GetMe();
                await _logger.LogAsync($"Бот {me.FirstName} запущен");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Бот", ex);
            }

            Console.ReadLine();
            cts.Cancel();
            await _logger.LogAsync($"Бот выключен");
        }
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            if (update.CallbackQuery is { } callbackQuery)
            {
                await HandleCallbackQueryAsync(botClient, callbackQuery, cancellationToken);
                return;
            }

            if (update.Message is not { } message || message.Text is not { } messageText)
            {
                return;
            }

            var chatId = message.Chat.Id;
            var state = _stateManager.GetUserState(chatId);


            if (_stateStorage.IsAuthorized(chatId) == false)
            {
                if (state == "awaiting_password")
                {
                    if (message.Text == _config.PasswordLoadConfiguration())
                    {
                        await botClient.SendMessage(chatId, "Авторизация успешна");
                        await _stateStorage.Authorize(chatId);
                        _stateManager.RemoveUserState(chatId);
                        await ShowMainMenu(botClient, chatId);
                        return;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Пароль неправильный. Повторите попытку");
                        return;
                    }
                }
                else
                {
                    _stateManager.SetUserState(chatId, "awaiting_password");
                    await botClient.SendMessage(chatId, "Введите пароль");
                    return;
                }
            }
            if (state == "awaiting_interval")
            {
                await schedule_edit_CommandAsync(botClient, chatId, messageText);
                return;
            }


            await _logger.LogAsync($"Получено сообщение: '{messageText}' от пользователя {chatId}");

            if (messageText.StartsWith("/start"))
            {
                await ShowMainMenu(botClient, chatId);
            }
        }

        async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await _logger.LogErrorAsync("Произошла ошибка", exception);
            await Task.CompletedTask;
        }
        private InlineKeyboardMarkup BuildMainMenuKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                    new[] { InlineKeyboardButton.WithCallbackData("▶️Запустить все", "menu_run"),  InlineKeyboardButton.WithCallbackData("📋Выбрать категории", "menu_categories") },
                    new[] { InlineKeyboardButton.WithCallbackData("⏰Расписание", "menu_schedule"), InlineKeyboardButton.WithCallbackData("📊Статус", "menu_status") },
                    new[] { InlineKeyboardButton.WithCallbackData("⏹Остановить", "menu_stop"), InlineKeyboardButton.WithCallbackData("ℹ️Помощь", "menu_help") },
            });
        }
        private InlineKeyboardMarkup BuildScheduleMenuKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                    new[] { InlineKeyboardButton.WithCallbackData("▶️Включить", "schedule_on"),  InlineKeyboardButton.WithCallbackData("⏹Выключить", "schedule_off") },
                    new[] { InlineKeyboardButton.WithCallbackData("✏️ Изменить интервал", "schedule_edit") },
                    new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", "menu_back") },
            });
        }
    }
}