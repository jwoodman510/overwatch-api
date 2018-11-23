using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using overwatch_api.Enums;
using overwatch_api.Models;

namespace overwatch_api.Services
{
    public class OwHerokuStatsService : StatsService
    {
        public override int Ordinal => 1;
        public override TimeSpan Timout => TimeSpan.FromSeconds(5);

        public OwHerokuStatsService(
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory)
            : base(configuration, loggerFactory, httpClientFactory)
        {
        }

        public override async Task<PlayerStats> GetAsync(HttpClient httpClient, Platform platform, Region region, string battletag)
        {
            var response = await httpClient.GetAsync($"{platform.ToString().ToLower()}/{GetRegion(region)}/{battletag}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<Stats>(json);

            return new PlayerStats
            {
                Icon = apiResponse.Portrait,
                Level = apiResponse.Level ?? 0,
                LevelIcon = apiResponse.LevelIcon,
                PrestigeIcon = apiResponse.PrestigeIcon,
                Rating = apiResponse.Competitive?.Rank ?? 0,
                RatingIcon = apiResponse.Competitive?.RankIcon,
                Endorsement = apiResponse.Endorsement?.Level ?? 0,
                EndorsementIcon = apiResponse.Endorsement?.Frame
            };
        }

        private string GetRegion(Region region)
        {
            return region == Region.Asia
                ? "asia"
                : region == Region.Eu
                    ? "eu"
                    : "usa";
        }

        private class Stats
        {
            public int? Level { get; set; }

            public string Portrait { get; set; }

            [JsonProperty("levelFrame")]
            public string LevelIcon { get; set; }

            [JsonProperty("star")]
            public string PrestigeIcon { get; set; }

            public Endorsement Endorsement { get; set; }

            public Competitive Competitive { get; set; }
        }

        public class Endorsement
        {
            public int? Level { get; set; }

            public string Frame { get; set; }
        }

        public class Competitive
        {
            public int? Rank { get; set; }

            [JsonProperty("rank_img")]
            public string RankIcon { get; set; }
        }
    }
}
