using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using overwatch_api.Enums;
using overwatch_api.Models;

namespace overwatch_api.Services
{
    public class OwApiStatsService : StatsService
    {
        public override int Ordinal => 2;

        public OwApiStatsService(
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory)
            : base(configuration, loggerFactory, httpClientFactory) { }

        public override async Task<PlayerStats> GetAsync(HttpClient httpClient, Platform platform, Region region, string battletag)
        {
            var response = await httpClient.GetAsync($"{platform.ToString().ToLower()}/{region.ToString().ToLower()}/{battletag}/profile");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PlayerStats>(json);
        }
    }
}
