using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace VoiceroidNotifyIcon
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            NotifyIcon icon = VoiceroidIcon.Build();


            new Task(() =>
            {
                string text = Console.ReadLine();
                while (text != null)
                {
                    string[] p = text.Split(',');
                    switch (p[0])
                    {
                        case "Add":
                            VoiceroidIcon.Add(int.Parse(p[1]), p[2], p[3]);
                            break;
                        case "Update":
                            VoiceroidIcon.Update(int.Parse(p[1]), p[2]);
                            break;
                    }

                    text = Console.ReadLine();
                }

                icon.Visible = false;
                Application.Exit();
            }).Start();

            Application.Run();
        }
    }
}

