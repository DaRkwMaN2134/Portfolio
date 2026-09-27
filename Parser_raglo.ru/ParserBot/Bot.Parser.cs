using ConfigurationLibrary;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
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
        async Task ParserCommandAsync(ITelegramBotClient botClient, long chatId, List<string>? selectedUrls = null)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Learning");

            List<Card> batch = new List<Card>();

            var stopButton = new[] { new[] { InlineKeyboardButton.WithCallbackData("⏹ Остановить", "stop_parser") } };
            var stopKeyboard = new InlineKeyboardMarkup(stopButton);
            var messageIdToStop = await botClient.SendMessage(chatId, "Парсинг запущен. Нажмите кнопку, чтобы остановить.", replyMarkup: stopKeyboard);
            var messageId = messageIdToStop.Id;

            string categoryName = null;

            int batchSize = 50;
            int currentRow = 2;
            int totalProcessed = 0;
            int notifyStep = 100;

            var categories = new List<string>
        {
            "https://raglo.ru/catalog/dushevye-trapy/",
            "https://raglo.ru/catalog/kukhnya/dozatory-/",
            "https://raglo.ru/catalog/po-seriyam/",
            "https://raglo.ru/catalog/polotentsesushiteli/",
            "https://raglo.ru/catalog/splenka/zapchasti-s/",
            "https://raglo.ru/catalog/aksessuary-dlya-smesiteley/",
            "https://raglo.ru/catalog/kukhonnye-moyki/",
            "https://raglo.ru/catalog/aksessuary-dlya-vannoy-komnaty/"
        };

            _parserCts = new CancellationTokenSource();


            if (File.Exists("Card.xlsx"))
            {
                File.Delete("Card.xlsx");
            }


            if (selectedUrls != null)
            {
                categories = selectedUrls;
            }
            else
            {
                categories = _categoryNames.Select(n => _categoryToUrl[n]).ToList();
            }
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var _botOutput = scope.ServiceProvider.GetRequiredService<IBotOutput>();

                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Карточки");
                List<string> headeades = new List<string>{
                "Имя категории",
                "Артикль",
                "Url-картинки",
                "Цена",
                "Описание"};

                for (int i = 0; i < headeades.Count; i++)
                {
                    sheet.Cells[1, i + 1].Value = headeades[i];
                }
                sheet.View.FreezePanes(2, 1);

                if (_isParsing)
                {
                    return;
                }

                try
                {
                    _isParsing = true;
                    foreach (var categoryUrl in categories)
                    {
                        string url = categoryUrl;
                        while (!string.IsNullOrEmpty(url))
                        {
                            var html = await _httpClient.HttpRequestAsync(url, _parserCts.Token);
                            var (cards, categoryNameTask) = await _htmlParser.ParseCategoryAsync(html, categoryUrl, _parserCts);

                            categoryName = categoryNameTask ?? "Без категории";

                            var category = await _botOutput.GetOrCreateCategoryAsync(categoryName);
                            await _botOutput.SaveProductsAsync(cards, category);

                            batch.AddRange(cards);
                            totalProcessed += cards.Count;

                            if (batch.Count >= batchSize)
                            {
                                await _excelOutput.AppendCardsAsync(sheet, batch, currentRow);
                                currentRow += batch.Count;
                                batch.Clear();
                            }
                            if (totalProcessed % notifyStep < cards.Count)
                            {
                                await _logger.LogAsync($"Обработано карточек - {totalProcessed}");
                                await botClient.SendMessage(chatId, $"⏳ Обработано {totalProcessed} товаров...");
                            }
                            url = _htmlParser.ParseUrl(html, url);
                        }
                    }
                    if (batch.Count > 0)
                    {
                        await _excelOutput.AppendCardsAsync(sheet, batch, currentRow);
                        currentRow += batch.Count;
                        batch.Clear();
                    }

                    await package.SaveAsAsync(new FileInfo("Card.xlsx"));
                    try
                    {
                        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
                        await executionStrategy.ExecuteAsync(async () =>
                        {
                            using var transaction = await _dbContext.Database.BeginTransactionAsync();
                            try
                            {
                                var pending = _dbContext.ChangeTracker.Entries<Product>().Count(e => e.State == EntityState.Added);
                                await _logger.LogAsync($"Перед финальным SaveChangesAsync: {pending} продуктов в очереди");
                                await _dbContext.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }
                            catch
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        });
                        await _logger.LogAsync($"Сохранено {totalProcessed} товаров");
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync($"Критическая ошибка сохранения: {ex.Message}");
                        throw;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync(ex.Message);
                }
                await _logger.LogAsync($"Карточки спарсены");
                await botClient.SendMessage(chatId, $"Всего спарсено карточек {totalProcessed}", cancellationToken: _parserCts.Token);
                _lastRunCount = totalProcessed;
                _lastRunTime = DateTime.Now;
                await SendFileAsync(botClient, chatId);
                await botClient.EditMessageText(chatId, messageId, "✅ Парсинг успешен", replyMarkup: null);
            }
            catch (OperationCanceledException)
            {
                await _logger.LogAsync("Парсинг был остановлен пользователем.");
                return;
            }
            finally
            {
                _parserCts?.Cancel();
                _parserCts?.Dispose();
                _isParsing = false;
            }
        }

        async Task SendFileAsync(ITelegramBotClient botClient, long chatId)
        {
            if (!File.Exists("Card.xlsx"))
            {
                await botClient.SendMessage(chatId, "Файл не создан, проверьте парсинг.");
                return;
            }
            try
            {
                await using var stream = File.OpenRead("Card.xlsx");
                await botClient.SendDocument(chatId, stream);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Произошла ошибка", ex);
            }
        }
    }


}
