# RagloParser

Telegram-бот с парсером интернет-магазина raglo.ru. Собирает товары из 8 категорий с пагинацией, сохраняет в PostgreSQL и выгружает в форматированный Excel-файл.

## Стек технологий
- C# / .NET (net10.0)
- Telegram.Bot
- HtmlAgilityPack
- EPPlus
- EF Core
- PostgreSQL
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration

## Архитектура

- Program.cs - точка входа, настройка DI
- ParserBot/ - Telegram-бот, разбитый на partial-классы:
  - Bot.cs — ядро, конструктор, запуск
  - Bot.Commands.cs - обработка команд
  - Bot.Callbacks.cs - обработка inline-кнопок
  - Bot.Parser.cs - запуск и остановка парсинга
  - StateManager.cs - состояние в памяти
  - StateStorage.cs - сохранение в state.json
  - BotState.cs - модель состояния
  - BotDataOutput.cs - сохранение товаров
- ConfigurationLibrary/ - работа с БД:
  - AppDbContext.cs - EF Core контекст
  - FileLogger.cs — логирование в файл
- Models/ - модели данных
- ParserLibrary/ - сетевой слой:
  - Http_Client.cs - HTTP-запросы с retry
  - Html_Parser.cs - парсинг HTML
- FileIOLibrary/ 
  - Excel_Output.cs — выгрузка в Excel


## Функционал

### Парсинг
- 8 категорий товаров (душевые трапы, дозаторы, смесители, полотенцесушители и др.)
- Пагинация - автоматический переход по страницам
- Параллельная загрузка (до 20 потоков через Parallel.ForEachAsync)
- Retry HTTP с экспоненциальной задержкой (4 попытки)
- Дедупликация товаров по артикулу (через HashSet)
- Отмена парсинга через CancellationTokenSource

### Данные
- Сохранение в PostgreSQL через EF Core
- Транзакции с ExecutionStrategy (retry при временных ошибках)
- Выгрузка в Excel с форматированием (заголовки, границы, автофильтр, закрепление шапки)

### Telegram-бот
- Авторизация по паролю
- Inline-меню 2×3 (Запустить все / Выбрать категории / Расписание / Статус / Остановить / Помощь)
- Парсинг по расписанию (PeriodicTimer)
- Сохранение состояния между перезапусками (state.json)


## Как запустить

1. Установите .NET SDK
2. Клонируйте репозиторий.
3. Откройте решение в Visual Studio или Rider.
4. Создайте бота в Telegram через @BotFather, скопируйте API-токен.
5. Установите PostgreSQL. По умолчанию есть пользователь postgres. Создайте базу данных (например, raglo_db).
6. Создайте appsettings.json и укажите:
    ```json
    {
      "Bot_Token": { "Token": "YOUR_TOKEN" },
      "ConnectionStrings": { "Postgres": "Host=localhost;Port=5432;Username=postgres;Password=***;Database=raglo_db" },
      "Bot_Password": "YOUR_PASSWORD"
    }
    ```
7. Примените миграции к базе данных:
    ```bash
    dotnet ef database update
    ```

8. Запустите проект. В Telegram отправьте боту /start и введите пароль.


## Пример результата
![Excel-файл с товарами:](README/screenshot-excel.png)