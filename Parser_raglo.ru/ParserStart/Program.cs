using ConfigurationLibrary;
using DataLibrary;
using FileIOLibrary;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using ParserLibrary;

class Program
{
    private readonly ILogger _logger;
    private readonly IHttpClient _httpClient;
    private readonly IHtmlParser _htmlParser;
    private readonly IExcelOutput _excelOutput;
    private static CancellationTokenSource? _parserCts = null;
    public Program(ILogger logger, IHttpClient httpClient, IHtmlParser htmlParser, IExcelOutput excelOutput)
    {
        _logger = logger;
        _httpClient = httpClient;
        _htmlParser = htmlParser;
        _excelOutput = excelOutput;
    }
    static CancellationTokenSource token = new CancellationTokenSource();
    static async Task Main(string[] args)
    {

        var services = new ServiceCollection();
        services.AddSingleton<IHttpClient, Http_Client>();
        services.AddSingleton<IHtmlParser, Html_Parser>();
        services.AddSingleton<IExcelOutput, Excel_Output>();
        services.AddSingleton<ILogger, FileLogger>();
        services.AddSingleton<Program>();


        var serviceProvider = services.BuildServiceProvider();

        var start = serviceProvider.GetRequiredService<Program>();

        await start.StartParser();

    }


     async Task StartParser()
    {
        var allCards = new List<Card>();
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

        foreach (var categoryUrl in categories)
        {
            string url = categoryUrl;
            while (!string.IsNullOrEmpty(url))
            {
                var html = await _httpClient.HttpRequestAsync(url, token);
                (var cards, var title) = await _htmlParser.ParseCategoryAsync(html, categoryUrl, token);
                allCards.AddRange(cards);
                Console.Write($"Обработано карточек - {allCards.Count}\n");
                url = _htmlParser.ParseUrl(html, url);
            }
        }
        await _excelOutput.ExcelOutput(allCards);

        using (var db = new AppDbContext())
        {
            if (!db.categories.Any())
            {
                var category = new Category { Name = "Душевые трапы" };
                category.products.Add(new Product { Name = "Трап 10x10", Price = 3000, Tags = new List<string> { "нержавейка", "новинка" } });
                db.categories.Add(category);
                await db.SaveChangesAsync();
            }
        }
    }
   
}
