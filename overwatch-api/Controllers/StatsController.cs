using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace overwatch_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private static readonly string[] Regions = { "na", "eu", "asia" };
        private static readonly string[] Platforms = { "pc", "psn", "xbox" };

        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public StatsController(
            IMemoryCache cache,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{platform}/{region}/{battletag}")]
        public async Task<IActionResult> GetAsync([FromRoute] string platform, [FromRoute] string region, [FromRoute] string battletag)
        {
            if (!Platforms.Contains(platform))
            {
                return NotFound();
            }

            if (!Regions.Contains(region))
            {
                return NotFound();
            }

            return Ok(await _cache.GetOrCreateAsync($"{platform}:{region}:{battletag}", x => GetProfileAsync(x, platform, region, battletag)));
        }

        private async Task<object> GetProfileAsync(ICacheEntry cacheEntry, string platform, string region, string battletag)
        {
            var ttl = int.Parse(_configuration["ProfileTTL"]);

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri(_configuration["ApiHost"]);

                var response = await httpClient.GetAsync($"{platform}/{region}/{battletag}/profile");

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject(json);

                cacheEntry.SetValue(result);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(ttl));

                return result;
            }
        }
    }
}
