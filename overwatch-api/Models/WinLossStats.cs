namespace overwatch_api.Models
{
    public class WinLossStats
    {
        public int Played { get; set; }

        public int Won { get; set; }

        public int Lost => Played - Won;

        public decimal WinPercentage => Won / (decimal) Played;
    }
}
