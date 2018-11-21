using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using overwatch_api.Enums;
using overwatch_api.Models;

namespace overwatch_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private static Regex BattleTagRegex = new Regex("^.+-[0-9]+$");

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

        [Produces("application/json")]
        [HttpGet("{platform}/{region}/{battletag}")]
        [ProducesResponseType(typeof(PlayerStats), 200)]
        public async Task<IActionResult> GetAsync([FromRoute] Platform platform, [FromRoute] Region region, [FromRoute] string battletag)
        {
            if (!BattleTagRegex.IsMatch(battletag))
            {
                return BadRequest("Invalid battletag.");
            }

            var result = await _cache.GetOrCreateAsync($"{platform}:{region}:{battletag}", x => GetProfileAsync(x, platform, region, battletag));

            result.Region = region;
            result.Platform = platform;

            return Ok(result);
        }

        private async Task<PlayerStats> GetProfileAsync(ICacheEntry cacheEntry, Platform platform, Region region, string battletag)
        {
            var ttl = int.Parse(_configuration["ProfileTTL"]);

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri(_configuration["ApiHost"]);

                var response = await httpClient.GetAsync($"{platform.ToString().ToLower()}/{region.ToString().ToLower()}/{battletag}/profile");

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<PlayerStats>(json);

                cacheEntry.SetValue(result);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(ttl));

                return result;
            }
        }
    }
}
