using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using overwatch_api.Enums;
using overwatch_api.Models;

namespace overwatch_api.Services
{
    public class OwApiNetStatsService : StatsService
    {
        public OwApiNetStatsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        public override async Task<PlayerStats> GetAsync(HttpClient httpClient, Platform platform, Region region, string battletag)
        {
            httpClient.DefaultRequestHeaders.Add("user-agent", "overwatch-api");

            var response = await httpClient.GetAsync($"{battletag}/stats?platform={platform.ToString().ToLower()}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

            var stats = apiResponse.ForRegion(region).Stats.Competitive.OverallStats;

            return new PlayerStats
            {
                Region = region,
                Platform = platform,
                Icon = stats.Avatar,
                Name = battletag,
                Level = stats.Level,
                Prestige = int.TryParse(stats.Prestige, out int x) ? x : 0,
                Rating = stats.Comprank,
                RatingIcon = stats.TierImage,
                Endorsement = stats.EndorsementLevel
            };
        }

        private class ApiResponse
        {
            public RegionStats Eu { get; set; }
            public RegionStats Kr { get; set; }
            public RegionStats Us { get; set; }

            public RegionStats ForRegion(Region region)
            {
                return region == Region.Asia
                    ? Kr
                    : region == Region.Eu
                        ? Eu
                        : Us;
            }
        }

        private class RegionStats
        {
            public Stats Stats { get; set; }
        }

        private class Stats
        {
            public GameModeStats Quickplay { get; set; }

            public GameModeStats Competitive { get; set; }
        }

        private class GameModeStats
        {
            [JsonProperty("overall_stats")]
            public OverallStats OverallStats { get; set; }
        }

        private class OverallStats
        {
            public int Level { get; set; }

            [JsonProperty("endorsement_level")]
            public int EndorsementLevel { get; set; }

            public int Comprank { get; set; }

            public string Prestige { get; set; }

            public string Tier { get; set; }

            [JsonProperty("tier_image")]
            public string TierImage { get; set; }

            public string Avatar { get; set; }
        }
    }
}
