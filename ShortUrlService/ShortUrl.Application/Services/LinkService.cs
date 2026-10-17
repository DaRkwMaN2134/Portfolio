using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace ShortUrl.Application.Services
{
    public class LinkService
    {
        private readonly ILinkRepository _linkRepository;
        public LinkService(ILinkRepository linkRepository)
        {
            _linkRepository = linkRepository;
        }
        public async Task<string> ShortenLinkAsync(string longurl, User creator)
        {
            var shortcode = await CreateShortenLinkAsync(longurl);
            await _linkRepository.CreateLinkAsync(longurl, shortcode, creator);
            return shortcode;
        }

        public async Task<string> CreateShortenLinkAsync(string longurl) // метод будет доработан позже
        {
            string base62 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] utf8Url = Encoding.UTF8.GetBytes(longurl);
            utf8Url = SHA256.Create().ComputeHash(utf8Url);
            ReadOnlySpan<byte> bytes = utf8Url.AsSpan(0, 6);
            
            BigInteger number = new BigInteger(bytes);
            BigInteger q = number / 62;
            BigInteger r = number - (62 * q);
            BigInteger result = 0;


            while (number > 0)
            {
                number = 62 * q + r;
                q = number / 62;
                r = number - (62 * q);
                Console.WriteLine(number);
            }
            
            return result.ToString();

            return number.ToString();

            //var shortcode = ;
            //return shortcode;
            /* формула для перевода в base62 = X = base62 * q + r
            
            number

            string shortcode = "J";
            return shortcode;
            */
        }
    }
}
