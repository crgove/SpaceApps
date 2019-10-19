using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppiSpaceApps.Models;
using WebAppiSpaceApps.Repositories;

namespace WebAppiSpaceApps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DronController : ControllerBase
    {
        [HttpPost]
        public void Post([FromBody] ParamsDron paramDron)
        {
            var SolicitudDron = new DronRepository();


        }
    }
}