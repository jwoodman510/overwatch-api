using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using overwatch_api.Enums;
using overwatch_api.Models;
using overwatch_api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace overwatch_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IEnumerable<IStatsService> _statsServices;

        public HealthController(
            ILogger<HealthController> logger,
            IEnumerable<IStatsService> statsServices)
        {
            _logger = logger;
            _statsServices = statsServices;
        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ServiceStatus), 200)]
        public OkObjectResult GetHealth()
        {
            return Ok(new ServiceStatus(Health.Healthy));
        }

        [HttpGet("Stats")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ServiceStatus>), 200)]
        public async Task<OkObjectResult> GetServicesHealth([FromQuery] bool deep = false)
        {
            var statuses = new List<ServiceStatus>();

            foreach(var service in _statsServices)
            {
                statuses.Add(await GetStatsServiceStatus(service, deep));
            }

            return Ok(statuses);
        }

        private async Task<ServiceStatus> GetStatsServiceStatus(IStatsService service, bool deep)
        {
            var status = new ServiceStatus(Health.Healthy, service.Name);

            try
            {
                if(deep)
                {
                    await service.GetAsync(Platform.Pc, Region.Na, "woodman-11497");

                    service.Enable();
                }
                else if (service.Disabled)
                {
                    status.Health = Health.Unavailable;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{service.Name} is unavailable.");

                service.Disable();

                status.Health = Health.Unavailable;
            }

            return status;
        }
    }
}
