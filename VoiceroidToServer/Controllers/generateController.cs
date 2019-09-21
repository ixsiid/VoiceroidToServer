using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VoiceroidToServer.Controllers
{
    [Route("v1/generate")]
    [ApiController]
    public class generateController : ControllerBase
    {
        // GET api/values
        [HttpPost("play/{preset}")]
        public ActionResult<PlayResult> PostPlay(int preset, GenerateBody body)
        {
            Process p = Program.voiceroid[preset];
            p.StandardInput.WriteLine("${mode=0}");
            p.StandardInput.WriteLine(body.text);

            string result;
            do
            {
                result = p.StandardError.ReadLine();
                if (result[0] == 'E') return BadRequest(result.Substring(2));
            }
            while (result.StartsWith("${"));

            return new PlayResult() { success = true, estimate = int.Parse(result) };
        }

        // GET api/values
        [HttpPost("wave/{preset}")]
        public ActionResult<WaveResult> PostWave(int preset, GenerateBody body)
        {
            Process p = Program.voiceroid[preset];
            p.StandardInput.WriteLine("${mode=1}");
            p.StandardOutput.BaseStream.Flush();
            p.StandardInput.WriteLine(body.text);

            string result;
            do
            {
                result = p.StandardError.ReadLine();
                if (result[0] == 'E') return BadRequest(result.Substring(2));
            }
            while (result.StartsWith("${"));

            string [] res = result.Split(',');

            int count = int.Parse(res[1]);
            char[] buffer = new char[count];
            p.StandardOutput.ReadBlock(buffer, 0, count);

            short[] wave = new short[count / 2];
            for (int i = 0; i < count; i += 2)
            {
                wave[i / 2] = (short)(buffer[i] << 8 | (int)buffer[i + 1]);
            }

            return new WaveResult() { success = true, estimate = int.Parse(res[0]), wave = wave };
        }
    }
}
