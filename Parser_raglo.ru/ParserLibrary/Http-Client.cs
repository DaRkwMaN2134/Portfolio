using ConfigurationLibrary;
using System.Net;
using DataLibrary;

namespace ParserLibrary
{
    public class Http_Client : IHttpClient
    {
        private static readonly HttpClient _client;
        private readonly ILogger _logger;
        //private static readonly Proxy _proxy;


        public Http_Client(ILogger logger)
        {
            _logger = logger;
        }

        static Http_Client()
        {

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            /*var proxySettings = Configuration.GetProxySettings();
            if (proxySettings != null && !string.IsNullOrEmpty(proxySettings.address))
            {
                var proxy = new WebProxy(proxySettings.address, true);
                if (!string.IsNullOrEmpty(proxySettings.username)) 
                { 
                    proxy.Credentials = new NetworkCredential(proxySettings.username, proxySettings.password);   // можно добавить свой прокси
                }
                handler.Proxy = proxy;
            }*/

            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0...");
            _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> HttpRequestAsync(string url, CancellationToken token)
        {
            int maxRetries = 4;
            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    var responce = await _client.SendAsync(request, token);
                    if (responce.IsSuccessStatusCode)
                    {
                        return await responce.Content.ReadAsStringAsync();
                    }

                    if (responce.StatusCode >= HttpStatusCode.InternalServerError || responce.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        attempt++;
                        if (attempt >= maxRetries)
                        {
                            throw new HttpRequestException($"Не удалось получить ответ после {maxRetries} попыток");

                        }
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // 2, 4, 8 секунд
                        continue;
                    }
                    else
                    {
                        throw new HttpRequestException($"Ошибка запроса: {responce.StatusCode}");
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    await _logger.LogAsync("Программа остановлена");
                    throw;
                }
                catch (Exception ex) when (attempt < maxRetries - 1)
                {
                    await _logger.LogErrorAsync($"HTTP-Client", ex);
                    attempt++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), token);
                }
                catch(Exception ex)
                {
                    await _logger.LogErrorAsync($"HTTP-Client", ex);
                    throw;
                }
            }
            throw new Exception("Превышено число попыток");
        }
    }
}