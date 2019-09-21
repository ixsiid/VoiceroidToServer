using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static VoiceroidToServer.Program;

namespace VoiceroidToServer.Controllers
{
    [Route("v1")]
    [ApiController]
    public class infoController : ControllerBase
    {
        [HttpGet("list")]
        public ActionResult<LaunchInfo[]> Get()
        {
            return Program.presets.ToArray();
        }

        [HttpPost("add")]
        public ActionResult<LaunchInfo> Post(LaunchInfo info)
        {
            info.id = null;
            if (Program.LaunchProcess(ref info)) presets.Add(info);
            else return BadRequest();
            return info;
        }

    }
}
