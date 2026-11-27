using Castle.Core.Logging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using ShortUrl.Application.DTO;
using ShortUrl.Application.Interfaces;
using ShortUrl.Application.Services;
using ShortUrl.Domain.Interfaces;
using ShortUrl.Domain.Models;
using Xunit.Abstractions;

namespace ShortUrl.Tests
{
    public class LinkService_Test
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILinkRepository> _linkRepoMock;
        private readonly Mock<IClickRepository> _clickRepoMock;
        private readonly Mock<ClickService> _clickServiceMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IClientInfoParser> _clientParserMock;
        private readonly LinkService _linkService;
        private readonly Mock<ILogger<LinkService>> _loggerLinkMock;
        private readonly Mock<ILogger<ClickService>> _loggerClickMock;
        private readonly Mock<IBackgroundChannelTask<RedirectDTO>> _channelTaskMock;
        public LinkService_Test(ITestOutputHelper output)
        {
            _output = output;

            _linkRepoMock = new Mock<ILinkRepository>();
            _clickRepoMock = new Mock<IClickRepository>();
            _loggerLinkMock = new Mock<ILogger<LinkService>>();
            _loggerClickMock = new Mock<ILogger<ClickService>>();
            _clickServiceMock = new Mock<ClickService>(_clickRepoMock.Object, _linkRepoMock.Object, _loggerClickMock.Object);
            _userRepoMock = new Mock<IUserRepository>();
            _clientParserMock = new Mock<IClientInfoParser>();
            _channelTaskMock = new Mock<IBackgroundChannelTask<RedirectDTO>>();


            _linkService = new LinkService(
                _linkRepoMock.Object,
                _clickServiceMock.Object,
                _userRepoMock.Object,
                _clientParserMock.Object,
                _loggerLinkMock.Object,
                _channelTaskMock.Object,
                _clickRepoMock.Object);
        }


        [Theory]
        [InlineData("https://example.com")]
        [InlineData("https://github.com")]
        [InlineData("https://google.com")]
        [InlineData("https://stackoverflow.com")]
        [InlineData("https://microsoft.com")]
        [InlineData("https://service.local")]
        [InlineData("http://test-domain.org")]
        [InlineData("https://domain.net")]
        [InlineData("https://localhost:5001/swagger/index.html")]
        [InlineData("https://ya.ru")]
        public void CreateShortenLinkWithoutSalt_Test(string longurl)
        {
            var result = _linkService.CreateShortenLink(longurl, null);


            _output.WriteLine($"URL: {longurl}");
            _output.WriteLine($"Соль: {result}");
            _output.WriteLine("-----------------------------------");


            Assert.True(result.Length >= 8 && result.Length <= 10);
        }


        [Theory]
        [InlineData("https://example.com", "1000001", "2000001")]
        [InlineData("https://github.com", "1000002", "2000002")]
        [InlineData("https://google.com", "1000003", "2000003")]
        [InlineData("https://stackoverflow.com", "1000004", "2000004")]
        [InlineData("https://microsoft.com", "1000005", "2000005")]
        [InlineData("https://service.local", "1000006", "2000006")]
        [InlineData("http://test-domain.org", "1000007", "2000007")]
        [InlineData("https://domain.net", "1000008", "2000008")]
        [InlineData("https://localhost:5001/swagger/index.html", "1000009", "2000009")]
        [InlineData("https://ya.ru", "1000010", "2000010")]
        public void CreateShortenLinkWithSalt_Test(string longurl, string salt, string differentsalt)
        {
            var resultWithoutSalt = _linkService.CreateShortenLink(longurl, null);
            var resultWithSalt = _linkService.CreateShortenLink(longurl, salt);
            var resultWithDifferentSalt = _linkService.CreateShortenLink(longurl, differentsalt);


            _output.WriteLine($"URL: {longurl}");
            _output.WriteLine($"Без соли: {resultWithoutSalt}");
            _output.WriteLine($"С солью: {resultWithSalt}");
            _output.WriteLine($"С другой солью: {resultWithDifferentSalt}");

            _output.WriteLine("-----------------------------------");


            Assert.NotEqual(resultWithoutSalt, resultWithSalt);
            Assert.NotEqual(resultWithSalt, resultWithDifferentSalt);
        }
    }
}
