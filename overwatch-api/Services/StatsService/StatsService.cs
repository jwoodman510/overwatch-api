using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using overwatch_api.Enums;
using overwatch_api.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace overwatch_api.Services
{
    public abstract class StatsService : IStatsService
    {
        public virtual int Ordinal => int.MaxValue;
        public virtual TimeSpan Timout => TimeSpan.FromSeconds(2);

        public string Name => GetType().Name;

        public string Host { get; }

        public bool Disabled => bool.TryParse(_configuration[$"Disabled:{Name}"], out bool disabled) && disabled;

        public abstract Task<PlayerStats> GetAsync(HttpClient httpClient, Platform platform, Region region, string battletag);

        protected readonly ILogger Logger;

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public StatsService(
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory)
        {
            Logger = loggerFactory.CreateLogger(Name);

            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

            Host = configuration[$"Stats:{Name}"];

            if(Host == null)
            {
                throw new ArgumentNullException($"{nameof(IConfiguration)} value not found: {GetType().Name}");
            }
        }

        public async Task<PlayerStats> GetAsync(Platform platform, Region region, string battletag)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.Timeout = Timout;
                httpClient.BaseAddress = new Uri(Host);

                var stats = await GetAsync(httpClient, platform, region, battletag);

                stats.Region = region;
                stats.Name = battletag;
                stats.Platform = platform;

                return stats;
            }
        }
    }
}
