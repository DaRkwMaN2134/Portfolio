using ConfigurationLibrary;
using DataLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ParserBot
{
    public class BotDataOutput : IBotOutput
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Category> _categoryCache = new();
        private readonly HashSet<string> _savedArticles = new();

        public BotDataOutput(AppDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Category> GetOrCreateCategoryAsync(string categoryName)
        {
            if (_categoryCache.TryGetValue(categoryName, out var cachedCategory))
                return cachedCategory;

            if(categoryName == null)
            {
                categoryName = "Без категории";
            }
                var category = await _dbContext.categories.FirstOrDefaultAsync(c => c.Name == categoryName);
            if (category == null)
            {
                category = new Category { Name = categoryName };
                _dbContext.categories.Add(category);
                await _dbContext.SaveChangesAsync();
            }

            _categoryCache[categoryName] = category;
            return category;
        }

        public async Task SaveProductsAsync(List<Card> cards, Category category)
        {
            var articles = cards.Select(c => c.article).ToList();
            var existingArticles = await _dbContext.products
                .Where(p => articles.Contains(p.Article))
                .Select(p => p.Article)
                .ToListAsync();

            foreach (var card in cards)
            {
                if (existingArticles.Contains(card.article))
                {
                    await _logger.LogAsync($"SKIP: {card.article}");
                    continue;
                }

                if (_savedArticles.Add(card.article) == false)
                {
                    continue;
                }

                var product = new Product
                {
                    Name = card.description ?? card.article,
                    Price = decimal.TryParse(card.price, out var price) ? price : 0,
                    CategoryId = category.Id,
                    Article = card.article,
                    Pictureurl = card.pictureurl,
                };

                _dbContext.products.Add(product);
            }
        }
    }
}
