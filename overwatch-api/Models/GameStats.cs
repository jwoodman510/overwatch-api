namespace overwatch_api.Models
{
    public class GameStats
    {
        public int ElimintationsAvg { get; set; }

        public int DeathsAvg { get; set; }

        public WinLossStats Games { get; set; }
    }
}
