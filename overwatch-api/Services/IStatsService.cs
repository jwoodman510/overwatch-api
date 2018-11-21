using overwatch_api.Enums;
using overwatch_api.Models;
using System.Threading.Tasks;

namespace overwatch_api.Services
{
    public interface IStatsService
    {
        Task<PlayerStats> GetAsync(Platform platform, Region region, string battletag);
    }
}
