using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace VoiceroidToServer
{
    public class Program
    {
        public static Dictionary<int, Process> voiceroid;
        public static List<LaunchInfo> presets;

        private static int id;
        private static IEnumerable<string> voices;
        private static IEnumerable<string> langs;
        private static string seed, path;

        public struct LaunchInfo
        {
            public int? id;
            public string voice;
            public string lang;
        }

        private static Process iconProcess;

        public static void Main(string[] args)
        {
            seed = null;
            path = null;
            foreach (var a in args)
            {
                if (!a.StartsWith("--")) continue;
                var n = a.Split("=");
                switch (n[0])
                {
                    case "--seed":
                        seed = n[1];
                        break;
                    case "--path":
                        path = n[1];
                        break;
                }
            }

            foreach (var n in new object[] { seed, path })
            {
                if (n == null)
                {
                    Console.Error.WriteLine("It is required ARGS:");
                    Console.Error.WriteLine(" --seed=AITalk License authorization seed");
                    Console.Error.WriteLine(@" --path=VOICEROID2 install directory (normaly, ""C:\Program Files(x86)\AHS\VOICEROID2""");
                    return;
                }
            }

            Console.WriteLine("Launch args is checked");

            Console.WriteLine("Notifyicon Starting...");

            ProcessStartInfo psi = new ProcessStartInfo("VoiceroidNotifyIcon.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            iconProcess = Process.Start(psi);

            Console.WriteLine("Done");

            voiceroid = new Dictionary<int, Process>();
            id = 2000;
            presets = new List<LaunchInfo>();

            voices = Directory.GetDirectories($@"{path}\Voice").Select(x => Path.GetFileName(x));
            langs = Directory.GetDirectories($@"{path}\Lang").Select(x => Path.GetFileName(x));

            foreach (string voice in voices)
            {
                foreach (string lang in langs)
                {
                    Console.WriteLine($"Launch: {voice}, {lang}");
                    LaunchInfo info = new LaunchInfo() { voice = voice, lang = lang, };

                    if (LaunchProcess(ref info)) presets.Add(info);
                }
            }

            id = 3000;

            Console.WriteLine("Prepare is all done.");

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            IWebHost w = CreateWebHostBuilder(args).Build();
            w.RunAsync(tokenSource.Token);

            string cmd = iconProcess.StandardOutput.ReadLine();
            Console.WriteLine("Message is " + cmd);
            while(cmd != null)
            {
                switch(cmd)
                {
                    case "Exit":
                        tokenSource.Cancel();
                        break;
                }
                cmd = iconProcess.StandardOutput.ReadLine();
            }

            Console.WriteLine("--- Notifyicon Message ---");
            Console.WriteLine(iconProcess.StandardError.ReadToEnd());

            Console.WriteLine("Complete");

            /*
            NotifyIcon icon = new NotifyIcon(new System.ComponentModel.Container());
            icon.Icon = new System.Drawing.Icon(@"icon.ico");
            icon.Visible = true;
            icon.BalloonTipTitle = "Voiceroid To Server";
            menu = new VoiceroidMenu(icon);
            icon.ContextMenu = menu;

            /*
            icon.BalloonTipText = "Listening";
            icon.BalloonTipIcon = ToolTipIcon.Info;
            icon.ShowBalloonTip(2000);
            */

        }

        public static bool LaunchProcess(ref LaunchInfo info)
        {
            if (!voices.Contains(info.voice)) return false;
            if (!langs.Contains(info.lang)) return false;

            ProcessStartInfo psi = new ProcessStartInfo($@"{path}\VoiceroidCLI.exe", $"--seed={seed} --voice={info.voice} --lang={info.lang}");
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.CreateNoWindow = true;
            psi.WorkingDirectory = path;

            voiceroid.Add(++id, Process.Start(psi));
            info.id = id;

            iconProcess.StandardInput.WriteLine($"Add,{id},{info.voice},{info.lang}");

            return true;
        }

        public static void UpdateNotify(int id, string parameter)
        {
            iconProcess.StandardInput.WriteLine($"Update,{id},{parameter}");
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
