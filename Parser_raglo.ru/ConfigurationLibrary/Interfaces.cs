using System;
using System.Collections.Generic;
using System.Text;
using OfficeOpenXml;
using DataLibrary;

namespace ConfigurationLibrary
{
    public interface ILogger
    {
        Task LogAsync(string message, string level = "INFO");
        Task LogErrorAsync(string message, Exception ex = null, string level = "ERROR:");
    }

    public interface IHttpClient
    {
        Task<string> HttpRequestAsync(string url, CancellationToken cancellationToken = default);
    }

    public interface IHtmlParser
    {
        Task<(List<Card> Cards, string Title)> ParseCategoryAsync(string html, string baseUrl, CancellationTokenSource cancellationToken);
        string ParseUrl(string html, string url);
    }

    public interface IExcelOutput
    {
        Task ExcelOutput(List<Card> cards);
        Task<int> AppendCardsAsync(ExcelWorksheet sheet, List<Card> cards, int startRow);
    }

    public interface IBotOutput
    {
        Task<Category> GetOrCreateCategoryAsync(string categoryName);
        Task SaveProductsAsync(List<Card> cards, Category category);
    }

}
