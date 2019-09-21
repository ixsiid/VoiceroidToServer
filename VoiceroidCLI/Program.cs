using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceroidCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            string seed = null, voice = null, lang="standard";
            foreach (var a in args)
            {
                if (!a.StartsWith("--")) continue;
                var n = a.Split(new char[] { '=' });
                switch (n[0])
                {
                    case "--seed":
                        seed = n[1];
                        break;
                    case "--voice":
                        voice = n[1];
                        break;
                    case "--lang":
                        lang = n[1];
                        break;
                }
            }

            foreach (var n in new object[] { seed, voice })
            {
                if (n == null)
                {
                    Console.Error.WriteLine("Require option --seed, --voice");
                    return;
                }
            }

            using (var voiceroid = new AITalkController(seed, voice, lang))
            {
                string text;
                bool speech = true;
                double time;
                do
                {
                    text = Console.ReadLine();

                    try
                    {
                        if (text.StartsWith("${") && text.EndsWith("}"))
                        {
                            string[] param = text.Substring(2, text.Length - 3).Split(new char[] { '&' });
                            bool setTTtsParam = false;
                            AITalkController.Tuning tuning = new AITalkController.Tuning(true);
                            foreach (var p in param)
                            {
                                string[] kv = p.Split(new char[] { '=' });
                                switch (kv[0])
                                {
                                    case "volume":
                                        setTTtsParam = true;
                                        tuning.Volume = float.Parse(kv[1]);
                                        break;
                                    case "pitch":
                                        setTTtsParam = true;
                                        tuning.Pitch = float.Parse(kv[1]);
                                        break;
                                    case "range":
                                        setTTtsParam = true;
                                        tuning.Range = float.Parse(kv[1]);
                                        break;
                                    case "speed":
                                        setTTtsParam = true;
                                        tuning.Speed = float.Parse(kv[1]);
                                        break;
                                    case "pauseMiddle":
                                        setTTtsParam = true;
                                        tuning.pause.Middle = int.Parse(kv[1]);
                                        break;
                                    case "pauseLong":
                                        setTTtsParam = true;
                                        tuning.pause.Long = int.Parse(kv[1]);
                                        break;
                                    case "pauseSentence":
                                        setTTtsParam = true;
                                        tuning.pause.Sentence = int.Parse(kv[1]);
                                        break;
                                    case "style":
                                        setTTtsParam = true;
                                        tuning.Style = kv[1];
                                        break;
                                    case "mode":
                                        speech = kv[1][0] == '0';
                                        break;
                                }
                            }
                            if (setTTtsParam)
                            {
                                voiceroid.SetTuning(tuning);
                            }
                            tuning = voiceroid.GetTuning();
                            Console.Error.WriteLine("${" + tuning.ToString() + "&mode=" + (speech ? "speech" : "raw") + "}");
                        }
                        else if (speech)
                        {
                            time = voiceroid.Speech(text);
                            Console.Error.WriteLine(time);
                        }
                        else
                        {
                            byte[] buffer = voiceroid.Raw(text, out time);
                            Console.Error.WriteLine($"{time},{buffer.Length}");
                            Console.Write(buffer);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("E:" + e.Message);
                    }
                } while (text != null);
            }
        }
    }
}