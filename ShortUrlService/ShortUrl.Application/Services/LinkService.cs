using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Reflection.Metadata;

namespace ShortUrl.Application.Services
{
    public class LinkService
    {
        private readonly ILinkRepository _linkRepository;
        public LinkService(ILinkRepository linkRepository)
        {
            _linkRepository = linkRepository;
        }

        public async Task<string> ShortenLinkAsync(string longurl, string deviceid, User creator)
        {
            if (longurl == null || deviceid == null || creator == null) return string.Empty; // добавил проверку на входные данные

            string shortcode = string.Empty;
            var existlink = await _linkRepository.GetUserLinkAsync(longurl, deviceid);
            if (existlink != null)
            {
                shortcode = existlink.ShortCode;
            }
            else
            {
                var shortlink = CreateShortenLink(longurl, null);
                var link = await _linkRepository.CreateLinkAsync(longurl, shortlink, creator);

                if (link != null)
                {
                    shortcode = shortlink;
                }
                else
                {
                    for(int i = 0; i<=5; i++)
                    {
                        var salt = Guid.NewGuid().ToString();
                        var shortlinksalt = CreateShortenLink(longurl, salt);
                        var linksalt = await _linkRepository.CreateLinkAsync(longurl, shortlinksalt, creator);
                        if (i == 5)
                        {
                            throw new InvalidOperationException("Не удалось сгенерировать уникальный код за 5 попыток");
                        }
                        if (linksalt == null)
                        {
                            continue;
                        }
                        else
                        {
                            shortcode = shortlinksalt;
                            break;
                        }
                    }
                }
            }
            return shortcode;
        }

        public async Task<string> RedirectLinkAsync(string shortcode)
        {
            if(shortcode == null) return string.Empty; // добавил проверку на входные данные

            var longurl = await _linkRepository.GetLongUrlAsync(shortcode);
            if (string.IsNullOrEmpty(longurl))
            {
                Console.WriteLine("Ссылка не найдена");
                return string.Empty;
            }
            else
            {
                return longurl;
            }
        }


        public string CreateShortenLink(string longurl, string? salt)
        {
            if (longurl == null) return string.Empty; // добавил проверку на входные данные

            if (salt != null)
            {
                longurl = longurl + "|" + salt;
            }
            string base62 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] utf8Url = Encoding.UTF8.GetBytes(longurl);
            utf8Url = SHA256.HashData(utf8Url);
            ReadOnlySpan<byte> bytes = utf8Url.AsSpan(0, 7);

            BigInteger number = new BigInteger(bytes, isUnsigned: true);
            BigInteger q = 0;
            BigInteger r = 0;

            string shorturl = string.Empty;
            while (number > 0)
            {
                r = number % 62;
                q = number / 62;
                shorturl += base62[(int)r];
                number = q;
            }
            string reversed = new string(shorturl.Reverse().ToArray());
            return reversed;
        }

        public async Task<long?> GetIdFromLinkAsync(string shortcode)
        {
            if (string.IsNullOrEmpty(shortcode)) return null;

            var linkid = await _linkRepository.GetLinkIdByShortCodeAsync(shortcode);
            return linkid;
        }
    }
}
