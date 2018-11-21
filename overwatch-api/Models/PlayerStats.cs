using overwatch_api.Enums;

namespace overwatch_api.Models
{
    public class PlayerStats
    {
        public Region Region { get; set; }
        
        public Platform Platform { get; set; }

        public string Icon { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public string LevelIcon { get; set; }

        public int Prestige { get; set; }

        public int Rating { get; set; }

        public string RatingIcon { get; set; }

        public int Endorsement { get; set; }

        public string EndorsementIcon { get; set; }
    }
}
