using Microsoft.Extensions.Configuration;
using overwatch_api.Enums;
using overwatch_api.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace overwatch_api.Services
{
    public abstract class StatsService : IStatsService
    {
        public readonly string Host;

        public abstract Task<PlayerStats> GetAsync(HttpClient httpClient, Platform platform, Region region, string battletag);

        private readonly IHttpClientFactory _httpClientFactory;

        public StatsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            Host = configuration[GetType().Name];

            if(Host == null)
            {
                throw new ArgumentNullException($"{nameof(IConfiguration)} value not found: {GetType().Name}");
            }
        }

        public async Task<PlayerStats> GetAsync(Platform platform, Region region, string battletag)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri(Host);

                return await GetAsync(httpClient, platform, region, battletag);
            }
        }
    }
}
