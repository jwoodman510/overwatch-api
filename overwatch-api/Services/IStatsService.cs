using overwatch_api.Enums;
using overwatch_api.Models;
using System.Threading.Tasks;

namespace overwatch_api.Services
{
    public interface IStatsService
    {
        int Ordinal { get; }

        string Name { get; }

        string Host { get; }

        bool Disabled { get; }

        Task<PlayerStats> GetAsync(Platform platform, Region region, string battletag);
    }
}
