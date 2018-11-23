using Newtonsoft.Json;

namespace overwatch_api.Models
{
    public enum Health
    {
        Healthy,
        Unavailable
    }

    public class ServiceStatus
    {
        public string Name { get; set; }

        [JsonIgnore]
        public Health Health { get; set; }

        public string Status => Health == Health.Healthy ? "wearegoodtogo" : "unavailable";

        public ServiceStatus(Health health, string name = null)
        {
            Health = health;
            Name = name;
        }

        public bool ShouldSerializeName() => !string.IsNullOrEmpty(Name);
    }
}
