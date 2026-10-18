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
        public async Task<string> ShortenLinkAsync(string longurl, User creator)
        {
            string shortcode = string.Empty;

            var existlink = await _linkRepository.GetUserLinkAsync(longurl, creator.DeviceId);
            if (existlink != null)
            {
                shortcode = existlink.ShortCode;
            }
            else
            {
                var shortlink = await CreateShortenLinkAsync(longurl, null);
                var link = await _linkRepository.CreateLinkAsync(longurl, shortlink, creator);

                if(link)
                {
                    shortcode = shortlink;
                }
                else
                {
                    while (true)
                    {
                        var salt = Guid.NewGuid().ToString();
                        var shortlinksalt = await CreateShortenLinkAsync(longurl, salt);
                        var linksalt = await _linkRepository.CreateLinkAsync(longurl, shortlinksalt, creator);
                        if (linksalt == false)
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

        public async Task GetLinkByUserAndUrlAsync(string longurl, string deviceid)
        {
            var link = await _linkRepository.GetLongUrlAsync(longurl);
            var creator = await _linkRepository.GetUserLinkAsync(longurl, deviceid);
        }



        public async Task<string> RedirectLinkAsync(string shortcode)
        {
            var url = await _linkRepository.GetLongUrlAsync(shortcode);
            if(url == null)
            {
                Console.WriteLine("Ссылка не найдена");
                return null;
            }
            else
            {
                return url;
            }    
        }


        public async Task<string> CreateShortenLinkAsync(string longurl, string? salt)
        {
            if(salt != null)
            {
                longurl = longurl + "|" + salt;
            }
            string base62 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] utf8Url = Encoding.UTF8.GetBytes(longurl);
            utf8Url = SHA256.Create().ComputeHash(utf8Url);
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
    }
}
