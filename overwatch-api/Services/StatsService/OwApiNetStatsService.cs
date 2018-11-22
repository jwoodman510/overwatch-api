using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using overwatch_api.Enums;
using overwatch_api.Models;

namespace overwatch_api.Services
{
    public class OwApiNetStatsService : StatsService
    {
        private readonly ThrottleLock<OwApiNetStatsService> _throttleLock;

        public OwApiNetStatsService(
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            ThrottleLock<OwApiNetStatsService> throttleLock)
            : base(configuration, loggerFactory, httpClientFactory)
        {
            _throttleLock = throttleLock;
        }

        public override async Task<PlayerStats> GetAsync(HttpClient httpClient, Platform platform, Region region, string battletag)
        {
            httpClient.DefaultRequestHeaders.Add("user-agent", "overwatch-api");

            var response = await httpClient.GetAsync($"{battletag}/stats?platform={platform.ToString().ToLower()}");

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Logger.LogInformation($"{battletag}: Waiting on semaphore...");

                await _throttleLock.Semaphore.WaitAsync();

                Logger.LogInformation($"{battletag}: Semaphore aquired. Delaying...");

                await Task.Delay(3000);

                try
                {
                    response = await httpClient.GetAsync($"{battletag}/stats?platform={platform.ToString().ToLower()}");
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, $"{battletag}: Failed.");

                    throw;
                }
                finally
                {
                    _throttleLock.Semaphore.Release();

                    Logger.LogInformation($"{battletag}: Semaphore released.");
                }
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

            var stats = apiResponse.ForRegion(region)?.Stats?.Competitive?.OverallStats
                ?? apiResponse.ForRegion(region)?.Stats?.Quickplay?.OverallStats;

            return new PlayerStats
            {
                Icon = stats?.Avatar,
                Level = stats?.Level ?? 0,
                Rating = stats?.Comprank ?? 0,
                RatingIcon = stats?.TierImage,
                Endorsement = stats?.EndorsementLevel ?? 0
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

            public int? Comprank { get; set; }

            public string Prestige { get; set; }

            public string Tier { get; set; }

            [JsonProperty("tier_image")]
            public string TierImage { get; set; }

            public string Avatar { get; set; }
        }
    }
}
