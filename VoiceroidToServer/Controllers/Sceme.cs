using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceroidToServer.Controllers
{
    public struct SimpleResult
    {
        public bool success;
        public string message;
    }

    public struct PlayResult
    {
        public bool success;
        public string message;
        public float estimate;
    }

    public struct WaveResult
    {
        public bool success;
        public string message;
        public float estimate;
        public short[] wave;
    }

    public struct Pause
    {
        public int? Middle;
        public int? Long;
        public int? Sentence;
    }

    public struct Tuning
    {
        public double? volume;
        public double? pitch;
        public double? range;
        public double? speed;
        public string style;
        public Pause pause;

        public Tuning(string str)
        {
            volume = pitch = range = speed = null;
            style = null;
            pause = new Pause() { Middle = null, Long = null, Sentence = null };
            foreach(string n in str.Split('&'))
            {
                string[] kv = n.Split('=');
                switch(kv[0])
                {
                    case "volume":
                        volume = double.Parse(kv[1]);
                        break;
                    case "pitch":
                        pitch = double.Parse(kv[1]);
                        break;
                    case "range":
                        range = double.Parse(kv[1]);
                        break;
                    case "speed":
                        speed = double.Parse(kv[1]);
                        break;
                    case "pauseMiddle":
                        pause.Middle = int.Parse(kv[1]);
                        break;
                    case "pauseLong":
                        pause.Long = int.Parse(kv[1]);
                        break;
                    case "pauseSentence":
                        pause.Sentence = int.Parse(kv[1]);
                        break;
                    case "style":
                        style = kv[1];
                        break;
                }
            }
        }

        public override string ToString()
        {
            List<string> p = new List<string>();

            if (volume != null) p.Add($"volume={volume}");
            if (pitch != null) p.Add($"pitch={pitch}");
            if (range != null) p.Add($"range={range}");
            if (speed != null) p.Add($"speed={speed}");

            if (pause.Middle != null) p.Add($"pauseMiddle={pause.Middle}");
            if (pause.Long != null) p.Add($"pauseLong={pause.Long}");
            if (pause.Sentence != null) p.Add($"pauseSentence={pause.Sentence}");

            if (style != null) p.Add($"style={style}");

            return string.Join('&', p);
        }
    }

    public struct GenerateBody
    {
        public string text;
        public string type;
    }
}
