using System;
using AI.Talk.Core;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace VoiceroidCLI
{
    class AITalkController : IDisposable
    {
        private struct Config
        {
            public string codeAuthSeed;
            public string lang;
            public int numWaveOut;
            public uint timeout;
            public int waveBufferSize;

            public Config(string seed, string lang)
            {
                this.lang = lang;
                numWaveOut = 10;
                timeout = 3 * 1000;
                waveBufferSize = 512 * 1024;

                codeAuthSeed = seed;
            }
        }

        private Config config;
        private WaveOut[] waveOuts;
        private int waveOutIndex;

        private AITalkResultCode code;
        private string currentSpeaker;

        private IntPtr paramPtr;
        private AITalk_TTtsParam ttsParam;


        public AITalkController(string seed, string speaker, string lang = "standard")
        {
            config = new Config(seed, lang);

            waveOuts = new WaveOut[config.numWaveOut];
            for (int i = 0; i < config.numWaveOut; i++)
            {
                waveOuts[i] = new WaveOut();
            }
            waveOutIndex = 0;


            AITalk_TConfig tConfig = new AITalk_TConfig();
            tConfig.codeAuthSeed = config.codeAuthSeed;
            tConfig.dirVoiceDBS = $@"Voice";
            tConfig.hzVoiceDB = 44100;
            tConfig.msecTimeout = config.timeout;
            tConfig.pathLicense = $@"aitalk.lic";
            code = AITalkAPI.Init(ref tConfig);

            if (code != AITalkResultCode.AITALKERR_SUCCESS) throw new Exception("初期化に失敗");

            // 複数Langは非対応
            // AITalkAPI.LangClear();
            code = AITalkAPI.LangLoad($@"Lang\{config.lang}");
            if (code != AITalkResultCode.AITALKERR_SUCCESS) throw new Exception("言語ロードに失敗");

            SetSpeaker(speaker);

            InitializeTuning();
        }

        private void InitializeTuning()
        {
            int paramPtrSize;
            paramPtr = AITalkMarshal.AllocateTTtsParam(1, out paramPtrSize);
            Marshal.WriteInt32(paramPtr, paramPtrSize);

            uint sz;
            code = AITalkAPI.GetParam(paramPtr, out sz);
            if (code != AITalkResultCode.AITALKERR_SUCCESS) throw new Exception("パラメータ取得に失敗: ");
            ttsParam = AITalkMarshal.IntPtrToTTtsParam(paramPtr);
            if (ttsParam.voiceName != currentSpeaker) throw new Exception("パラメータ取得結果がなんかおかしい");

            return;
        }

        public struct Pause
        {
            public int Long { get; set; }
            public int Middle { get; set; }
            public int Sentence { get; set; }

            public Pause(int pauseLong, int pauseMiddle, int pauseSentence) {
                Long = pauseLong;
                Middle = pauseMiddle;
                Sentence = pauseSentence;
            }

            public override string ToString()
            {
                return string.Join("&",
                    "pauseMiddle=" + Middle.ToString(),
                    "pauseLong=" + Long.ToString(),
                    "pauseSentence=" + Sentence.ToString());
            }
        }

        public struct Tuning
        {
            public Tuning(AITalk_TTtsParam param)
            {
                Volume = param.Speaker[0].volume;
                Pitch = param.Speaker[0].pitch;
                Range = param.Speaker[0].range;
                Speed = param.Speaker[0].speed;
                pause = new Pause(param.Speaker[0].pauseLong, param.Speaker[0].pauseMiddle, param.Speaker[0].pauseSentence);
                Style = param.Speaker[0].styleRate;
            }

            public Tuning(bool invalidTuning)
            {
                Volume = Pitch = Range = Speed = -1;
                pause = new Pause(-1, -1, -1);
                Style = null;
            }

            public void SetValues(Tuning tuning)
            {
                if (tuning.Volume >= 0) Volume = tuning.Volume;
                if (tuning.Pitch >= 0) Pitch = tuning.Pitch;
                if (tuning.Range >= 0) Range = tuning.Range;
                if (tuning.Speed >= 0) Speed = tuning.Speed;

                pause.Long = tuning.pause.Long >= 0 ? tuning.pause.Long : pause.Long;
                pause.Middle = tuning.pause.Middle >= 0 ? tuning.pause.Middle : pause.Middle;
                pause.Sentence = tuning.pause.Sentence >= 0 ? tuning.pause.Sentence : pause.Sentence;

                if (tuning.Style != null) Style = tuning.Style;
            }

            override public string ToString()
            {
                return string.Join("&",
                    "volume=" + Volume.ToString("F2"),
                    "pitch=" + Pitch.ToString("F2"),
                    "range=" + Range.ToString("F2"),
                    "speed=" + Speed.ToString("F2"),
                    "style=" + Style,
                    pause.ToString());
            }

            public float Volume { get; set; }
            public float Pitch { get; set; }
            public float Range { get; set; }
            public float Speed { get; set; }
            public string Style { get; set; }
            public Pause pause;
        }

        public Tuning GetTuning()
        {
            return new Tuning(ttsParam);
        }

        public float Range(float min, float max, float value)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public int Range(int min, int max, int value)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public void SetTuning(Tuning tuning)
        {
            if (tuning.Pitch >= 0) ttsParam.Speaker[0].pitch = Range(0.5f, 2f, tuning.Pitch);
            if (tuning.Volume >= 0) ttsParam.Speaker[0].volume = Range(0f, 5f, tuning.Volume);
            if (tuning.Range >= 0) ttsParam.Speaker[0].range = Range(0f, 2f, tuning.Range);
            if (tuning.Speed >= 0) ttsParam.Speaker[0].speed = Range(0.5f, 4f, tuning.Speed);

            if (tuning.pause.Middle >= 0) ttsParam.Speaker[0].pauseMiddle = Range(80, 500, tuning.pause.Middle);
            if (tuning.pause.Long >= 0) ttsParam.Speaker[0].pauseLong = Range(100, 2000, tuning.pause.Long);
            if (tuning.pause.Sentence >= 0) ttsParam.Speaker[0].pauseSentence = Range(0, 10000, tuning.pause.Sentence);
            if (tuning.Style != null) ttsParam.Speaker[0].styleRate = tuning.Style;

            ttsParam.Speaker[0].pauseLong = Math.Max(ttsParam.Speaker[0].pauseLong, ttsParam.Speaker[0].pauseMiddle);
            ttsParam.Speaker[0].pauseSentence = Math.Max(ttsParam.Speaker[0].pauseSentence, ttsParam.Speaker[0].pauseLong);

            IntPtr ptr = AITalkMarshal.TTtsParamToIntPtr(ref ttsParam);

            // Marshal.WriteInt32(ptr, 500);
            // TTtsParamToIntPtrメソッドは、先頭にサイズを書き込んである。（構造体の定義になってるから当たり前）

            code = AITalkAPI.SetParam(ptr);

            if (code != AITalkResultCode.AITALKERR_SUCCESS) throw new Exception("パラメータ設定に失敗: ");
        }

        public void SetSpeaker(string speaker)
        {
            if (currentSpeaker != speaker)
            {
                if (currentSpeaker != null)
                {
                    code = AITalkAPI.VoiceClear();
                    if (code != AITalkResultCode.AITALKERR_SUCCESS) throw new Exception("話者アンロードに失敗");
                    currentSpeaker = null;
                }
                code = AITalkAPI.VoiceLoad(speaker);
                if (code != AITalkResultCode.AITALKERR_SUCCESS) throw new Exception("話者ロードに失敗");
            }
            currentSpeaker = speaker;
        }

        public void Speech(string speaker, string text)
        {
            SetSpeaker(speaker);
            Speech(text);
        }

        private BufferedWaveProvider Provide(string text)
        {
            int id;
            AITalk_TJobParam param = new AITalk_TJobParam();
            param.modeInOut = AITalkJobInOut.AITALKIOMODE_PLAIN_TO_WAVE;
            param.userData = IntPtr.Zero;

            code = AITalkAPI.TextToSpeech(out id, ref param, text);
            if (code != AITalkResultCode.AITALKERR_SUCCESS) throw new Exception("スピーチ実行に失敗");

            var waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 1));
            byte[] waveBuffer = new byte[config.waveBufferSize];
            short[] buffer = new short[config.waveBufferSize / 2];
            do
            {
                uint count;
                code = AITalkAPI.GetData(id, buffer, (uint)config.waveBufferSize / 2, out count);
                Buffer.BlockCopy(buffer, 0, waveBuffer, 0, (int)count * 2);
                waveProvider.AddSamples(waveBuffer, 0, (int)count * 2);
            } while (code == AITalkResultCode.AITALKERR_SUCCESS);
            if (code != AITalkResultCode.AITALKERR_NOMORE_DATA) throw new Exception("変換データ読み取りに失敗");

            code = AITalkAPI.CloseSpeech(id);
            return waveProvider;
        }

        public byte [] Raw(string text, out double time)
        {
            var w = Provide(text);
            time = w.BufferedDuration.TotalMilliseconds;

            int count = w.BufferedBytes;
            byte[] buffer = new byte[count];
            w.Read(buffer, 0, count);

            return buffer;
        }

        public double Speech(string text)
        {
            var w = Provide(text);

            int k = waveOutIndex;
            waveOutIndex = (waveOutIndex + 1) % config.numWaveOut;

            waveOuts[k].Stop();
            waveOuts[k].Init(w);
            waveOuts[k].Play();

            return w.BufferedDuration.TotalMilliseconds;
        }


        public void Dispose()
        {
            Marshal.FreeCoTaskMem(paramPtr);
            AITalkAPI.End();
        }
    }
}
