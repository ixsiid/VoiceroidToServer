using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace VoiceroidNotifyIcon
{
    public class VoiceroidIcon
    {
        static private NotifyIcon icon;
        static private Font headerFont;
        static private Dictionary<int, ToolStripMenuItem> menus;
        static private TextInfo textInfo;
        static public NotifyIcon Build()
        {
            textInfo = CultureInfo.CurrentCulture.TextInfo;
            headerFont = new Font(SystemInformation.MenuFont, FontStyle.Bold);
            menus = new Dictionary<int, ToolStripMenuItem>();

            icon = new NotifyIcon(new System.ComponentModel.Container());
            icon.Visible = true;
            icon.Icon = new Icon(@"image\icon.ico");

            icon.ContextMenuStrip = new ContextMenuStrip();

            ToolStripItem exit = new ToolStripMenuItem("Exit", null, (s, e) =>
            {
                Console.WriteLine("Exit");
                icon.Visible = false;
                Application.Exit();
            });

            icon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripSeparator(),
                exit,
            });

            return icon;
        }

        static public void Add(int id, string voice, string lang)
        {
            ToolStripMenuItem menu = new ToolStripMenuItem($"{id}: {voice} - {lang}", null);
            menu.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

            menu.DropDownItems.AddRange(new ToolStripItem[]
            {
                CreateHeader("Base"),
                CreateValue(voice),
                CreateValue(lang),
                CreateHeader("Parameter"),
                CreateValue("Volume: 1.00"),
                CreateValue("Pitch: 1.00"),
                CreateValue("Range: 1.00"),
                CreateValue("Speed: 1.00"),
                CreateHeader("Pause"),
                CreateValue("Middle: 150"),
                CreateValue("Long: 370"),
                CreateValue("Sentence: 800"),
                CreateHeader("Style"),
                CreateValue(" "),
                /*
                new ToolStripSeparator(),
                new ToolStripButton("button"),
                new ToolStripComboBox("combo"),
                new ToolStripProgressBar("progressbar"),
                new ToolStripSplitButton("split button"),
                new ToolStripStatusLabel("status label"),
                new ToolStripTextBox("textbox"),
                */
            }); ;
            icon.ContextMenuStrip.Items.Insert(0, menu);

            menus.Add(id, menu);
        }

        static private ToolStripItem CreateHeader(string text)
        {
            var label = new ToolStripLabel(text);
            label.Font = headerFont;
            return label;
        }

        static private ToolStripItem CreateValue(string text)
        {
            var label = new ToolStripStatusLabel(text);
            label.Anchor = AnchorStyles.Right;
            return label;
        }

        static private readonly string[] paramIndex = new string[] {
            "Base",
            "Voice",
            "Lang",
            "Parameter",
            "volume",
            "pitch",
            "range",
            "speed",
            "Pause",
            "pauseMiddle",
            "pauseLong",
            "pauseSentence",
            "Style",
            "style",
        };
        static public void Update(int id, string parameterText)
        {
            if (!menus.ContainsKey(id)) return;
            var menu = menus[id];
            foreach(string s in parameterText.Split('&'))
            {
                string [] kv = s.Split('=');
                int index = Array.IndexOf(paramIndex, kv[0]);
                if (index < 3) continue;
                else if (index < 8) menu.DropDownItems[index].Text = textInfo.ToTitleCase(kv[0]) + ": " + double.Parse(kv[1]).ToString("F2");
                else if (index < 12) menu.DropDownItems[index].Text = kv[0].Substring(5) + ": " + int.Parse(kv[1]);
                else if (index < paramIndex.Length) menu.DropDownItems[index].Text = " " + kv[1];
            }
        }
    }
}


