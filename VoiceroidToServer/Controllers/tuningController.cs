using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VoiceroidToServer.Controllers
{
    [Route("v1/tuning")]
    [ApiController]
    public class tuningController : ControllerBase
    {
        [HttpGet("{preset}")]
        public ActionResult<Tuning> Get(int preset)
        {
            Process p = Program.voiceroid[preset];
            p.StandardError.BaseStream.Flush();
            p.StandardInput.WriteLine("${}");

            string result;
            do
            {
                result = p.StandardError.ReadLine();

                if (result[0] == 'E') return BadRequest(result.Substring(2));
            }
            while (!result.StartsWith("${"));

            return new Tuning(result.Substring(2, result.Length - 3));
        }

        [HttpPatch("{preset}")]
        public ActionResult<Tuning> Patch(int preset, Tuning tuning)
        {
            Process p = Program.voiceroid[preset];
            p.StandardError.BaseStream.Flush();
            p.StandardInput.WriteLine($"${{{tuning.ToString()}}}");

            string result;
            do
            {
                result = p.StandardError.ReadLine();
                if (result[0] == 'E') return BadRequest(result.Substring(2));
            }
            while (!result.StartsWith("${"));
            result = result.Substring(2, result.Length - 3);


            Program.UpdateNotify(preset, result);

            return new Tuning(result);
        }
    }
}
