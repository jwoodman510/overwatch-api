using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using overwatch_api.Enums;
using overwatch_api.Models;
using overwatch_api.Services;

namespace overwatch_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private static Regex BattleTagRegex = new Regex("^.+-[0-9]+$");

        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StatsController> _logger;
        private readonly IEnumerable<IStatsService> _statsServices;

        public StatsController(
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<StatsController> logger,
            IEnumerable<IStatsService> statsServices)
        {
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
            _statsServices = statsServices;
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

            foreach(var service in _statsServices)
            {
                try
                {
                    var result = await service.GetAsync(platform, region, battletag);

                    cacheEntry.SetValue(result);
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(ttl));

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(GetProfileAsync)} Failed for service: {service.Host}");
                }
            }

            throw new ApplicationException("All service calls failed.");
        }
    }
}
