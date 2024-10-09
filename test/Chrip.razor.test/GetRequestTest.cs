using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chrip.razor.test
{
    public class GetRequestTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _fixture;
        private readonly HttpClient _client;

        public GetRequestTest(WebApplicationFactory<Program> fixture)
        {
            _fixture = fixture;
            _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true, HandleCookies = true });
        }

        [Fact]
        public async Task CanSeePublicTimeline()
        {
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Chirp!", content);
            Assert.Contains("Public Timeline", content);
        }

        [Theory]
        [InlineData("Helge")]
        [InlineData("Adrian")]
        public async Task CanSeePrivateTimeline(string author)
        {
            var response = await _client.GetAsync($"/{author}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Chirp!", content);
            Assert.Contains($"{author}'s Timeline", content);
        }
    }
}