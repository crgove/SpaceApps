using Microsoft.AspNetCore.Mvc;
using WebAppiSpaceApps.Models;
using WebAppiSpaceApps.Repositories;

namespace WebAppiSpaceApps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DronController : ControllerBase
    {
        [HttpPost("status")]
        public ResultsDron Post([FromBody] ParamsDron paramDron)
        {
            return DronRepository.Shared.RefreshState(paramDron);
        }

        [HttpPut("dir")]
        public void Put([FromBody] DirectionDron directionDron)
        {
            DronRepository.Shared.SetDronDir(directionDron);
            DronRepository.Shared.StartFly();
        }
    }
}