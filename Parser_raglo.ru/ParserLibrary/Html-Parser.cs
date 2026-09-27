using ConfigurationLibrary;
using DataLibrary;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace ParserLibrary
{
    public class Html_Parser: IHtmlParser
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        public Html_Parser(IHttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }



        public async Task<(List<Card> Cards, string Title)> ParseCategoryAsync(string html, string baseUrl, CancellationTokenSource token)
        {
            string categoryName = null;

            var mainDoc = new HtmlDocument();
            mainDoc.LoadHtml(html);
            var allCardNode = mainDoc.DocumentNode.SelectNodes("//div[contains(@class, 'product-items-block')]//div[contains(@class, 'item-wrap col')]");
            if (allCardNode != null)
            {

                var categoryNameNone = mainDoc.DocumentNode.SelectSingleNode("/html/head/title");
                categoryName = categoryNameNone?.InnerText?.Trim();
                categoryName = categoryName?.Replace("Основной каталог", "").Replace("Raglo", "").Trim() ?? "";

                var cards = new ConcurrentBag<Card>();

                var options = new ParallelOptions { MaxDegreeOfParallelism = 20 };
                await Parallel.ForEachAsync(allCardNode, options, async (cardNode, cancellationToken) =>
                {
                    try
                    {
                        string article = "";
                        string urlimage = "";
                        string price = "";
                        string description = "";
                        string currentCardHtml = "";


                        var articleNode = cardNode.SelectSingleNode(".//div[contains(@class, 'product-info')]/a");
                        var pictureUrlNode = cardNode.SelectSingleNode(".//div[contains(@class, 'product-image-block')]//a//div");
                        var priceNode = cardNode.SelectSingleNode(".//div[contains(@class, 'product-info')]//div[contains(@class, 'price-block')]//div[(@class='price font-body bold')]");

                        if (articleNode != null)
                        {
                            article = articleNode.GetAttributeValue("href", "");
                            article = article.Trim('/').Split('/').LastOrDefault() ?? "";

                        }
                        else
                        {
                            await _logger.LogErrorAsync($"Парсер - Артикль на карточке пуст");
                            article = "-";
                        }


                        if (pictureUrlNode != null)
                        {
                            urlimage = "https://raglo.ru" + pictureUrlNode.GetAttributeValue("data-src", "");
                        }
                        else
                        {
                            await _logger.LogErrorAsync($"Парсер - Изображение на карточке пусто");
                            urlimage = "-";
                        }

                        if (priceNode != null)
                        {
                            price = priceNode?.InnerText?.Trim();
                            var numberMatch = System.Text.RegularExpressions.Regex.Match(price.Replace(" ", "").Replace("&nbsp;", ""), @"\d+[\d,.]*");
                            price = numberMatch.ToString();
                        }
                        else
                        {
                            await _logger.LogErrorAsync($"Парсер - Цена на карточке пуст");
                            price = "-";
                        }

                        if (article != "-")
                        {
                            var cardUrl = baseUrl + article + "/";
                            try
                            {
                                currentCardHtml = await _httpClient.HttpRequestAsync(cardUrl, token.Token);
                            }
                            catch (Exception ex)
                            {
                                await _logger.LogErrorAsync($"Парсер", ex);
                            }
                            if (currentCardHtml != null)
                            {
                                var cardDoc = new HtmlDocument();
                                cardDoc.LoadHtml(currentCardHtml);


                                var currentCardNode = cardDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'text-block')]");
                                if (currentCardNode != null)
                                {
                                    var descriptionNode = currentCardNode.SelectSingleNode(".//div[@itemprop='description']");

                                    if (descriptionNode != null)
                                    {
                                        description = descriptionNode?.InnerText?.Trim();
                                    }
                                    else
                                    {
                                        await _logger.LogErrorAsync($"Парсер - Описание на карточке пусто");
                                        description = "-";
                                    }
                                }
                                else
                                {
                                    await _logger.LogErrorAsync($"Парсер - Описание на карточке пусто");
                                    description = "-";
                                }
                            }
                        }
                        else
                        {
                            description = "-";
                        }

                        Card currentCard = new Card(categoryName, article, urlimage, price, description);
                        cards.Add(currentCard);
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync($"Парсер", ex);
                    }
                });
                return (cards.ToList(), categoryName);
            }
            else
            {
                return (new List<Card>(), categoryName);
            }
        }

        public string ParseUrl(string html, string url)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            string nextpageUrl = " ";
            var nextNode = doc.DocumentNode.SelectSingleNode("//div[@class='page-item ']//a");

            if (nextNode != null)
            {
                string nexPageReference = nextNode.GetAttributeValue("href", "");
                Uri fullUri = new Uri(new Uri(url), nexPageReference);
                nextpageUrl = fullUri.ToString();
                return nextpageUrl;
            }
            else
            {
                return null;
            }
        }
    }
}
