using Microsoft.Extensions.Logging;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Interfaces;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Web;


namespace ShortUrl.Application.Services
{
    public class LinkService
    {
        private readonly ILinkRepository _linkRepository;
        private readonly ClickService _clickService;
        private readonly IUserRepository _userRepository;
        private readonly IClickRepository _clickRepository;
        private readonly IClientInfoParser _infoParser;
        private readonly ILogger<LinkService> _logger;
        private readonly IBackgroundChannelTask<RedirectDTO> _channelTask;
        public LinkService(ILinkRepository linkRepository, ClickService clickservice, IUserRepository userRepository, 
            IClientInfoParser infoparser, ILogger<LinkService> logger, IBackgroundChannelTask<RedirectDTO> channelTask, IClickRepository clickRepository)
        {
            _linkRepository = linkRepository;
            _clickService = clickservice;
            _userRepository = userRepository;
            _infoParser = infoparser;
            _logger = logger;
            _channelTask = channelTask;
            _clickRepository = clickRepository;
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
                _logger.LogInformation("[МЕТОД ShortenLinkAsync] НАЧАЛОСЬ СОЗДАНИЕ КОРОТКОГО КОДА");
                var shortlink = CreateShortenLink(longurl, null);
                var link = await _linkRepository.CreateLinkAsync(longurl, shortlink, creator);

                if (link != null)
                {
                    shortcode = shortlink;
                    _logger.LogInformation("[МЕТОД ShortenLinkAsync] КОРОТКИЙ КОД {ShortCode} УСПЕШНО СОЗДАН", shortcode);
                }
                else
                {
                    for (int i = 0; i<=5; i++)
                    {
                        _logger.LogInformation("[МЕТОД ShortenLinkAsync] НАЧАЛОСЬ СОЗДАНИЕ С СОЛЬЮ. ПОПЫТКА {Attempt}", i+1);
                        var salt = Guid.NewGuid().ToString();
                        var shortlinksalt = CreateShortenLink(longurl, salt);
                        var linksalt = await _linkRepository.CreateLinkAsync(longurl, shortlinksalt, creator);
                        if (i == 5)
                        {
                            _logger.LogError("[МЕТОД ShortenLinkAsync] НЕ УДАЛОСЬ СОЗДАТЬ КОРОТКИЙ КОД ДЛЯ ССЫЛКИ {LongUrl} ЗА 5 ПОПЫТОК", longurl);
                            throw new InvalidOperationException("Не удалось сгенерировать уникальный код за 5 попыток");
                        }
                        if (linksalt == null)
                        {
                            continue;
                        }
                        else
                        {
                            _logger.LogInformation("[МЕТОД ShortenLinkAsync] КОРОТКИЙ КОД С СОЛЬЮ УСПЕШНО СОЗДАН");
                            shortcode = shortlinksalt;
                            break;
                        }
                    }
                }
            }
            return shortcode;
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

        public async Task<AnalyticsResponseDTO?> GetAnalyticsAsync(string shortcode)
        {
            if (string.IsNullOrEmpty(shortcode)) return null;

            var linkid = await _linkRepository.GetLinkIdByShortCodeAsync(shortcode);
            if (linkid == null) return null;
            var analytics = await _clickService.ClickAnalitycsAsync(linkid.Value);
            return analytics;
        }

        public async Task<List<BufferDTO>> GetBufferAsync(string deviceid)
        {
            var links = await _linkRepository.GetUserBufferAsync(deviceid); // вынести в BufferDTO/LinkDTO


            var buffer = links.Select(l => new BufferDTO
            {
                Id = l.Id,
                CreatedAt = l.CreatedAt,
                LongUrl = l.LongUrl,
                ShortCode = l.ShortCode,
                CreatorId = l.CreatorId
            }).ToList();

            return buffer;
        }

        public async Task<string> GetShortenAsync(string deviceid, string longurl)
        {
            var user = await _userRepository.GetByDeviceIdAsync(deviceid);

            if (user == null)
            {
                return string.Empty;
            }

            var shortcode = await ShortenLinkAsync(longurl, deviceid, user); // TODO: убрать дублирование deviceid, брать из user.DeviceId

            if (string.IsNullOrEmpty(shortcode)) return string.Empty;

            return shortcode;
        }

        public async Task<string> MakeRedirectAsync(string shortcode, HeadersDTO headersDTO, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(shortcode)) return string.Empty;

            var link = await _linkRepository.GetLinkByShortCodeAsync(shortcode);
            if (link == null) return string.Empty;

            var click = await _clickRepository.CreateEmptyClickByLinkIdAsync(link.Id);

            var redirectDTO = new RedirectDTO(
                headersDTO, 
                click
            );

            await _channelTask.ChannelWriteAsync(redirectDTO, cancellationToken);

            var uriBuilder = new UriBuilder(link.LongUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if(headersDTO.clientContext.UtmSource != null) query["utm_source"] = headersDTO.clientContext.UtmSource;
            
            if(headersDTO.clientContext.UtmMedium != null) query["utm_medium"] = headersDTO.clientContext.UtmMedium;

            if(headersDTO.clientContext.UtmCampaign != null) query["utm_campaign"] = headersDTO.clientContext.UtmCampaign;

            uriBuilder.Query = query.ToString();
            var longurl = uriBuilder.ToString();

            string encodedLongUrl = WebUtility.UrlEncode(longurl);
            string encodedShortcode = WebUtility.UrlEncode(shortcode);


            string redirectUrl = $"/redirect.html?longurl={encodedLongUrl}&shortcode={encodedShortcode}&clickid={click.Id}";
            return redirectUrl;
        }
    }
}
