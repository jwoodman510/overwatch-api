using overwatch_api.Enums;
using overwatch_api.Models;
using System.Threading.Tasks;

namespace overwatch_api.Services
{
    public interface IStatsService
    {
        string Host { get; }

        Task<PlayerStats> GetAsync(Platform platform, Region region, string battletag);
    }
}
