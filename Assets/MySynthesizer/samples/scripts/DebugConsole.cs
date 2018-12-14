
using UnityEngine;

using System;
using System.Text;
using System.Collections.Generic;

namespace MySpace.Sample
{
    public class DebugConsole : MonoBehaviour
    {
        public const int MaxNumLines = 100;
        private static readonly List<string> lines = new List<string>();
        private static DebugConsole instance = null;
        private static string temporary = "";
        [SerializeField, Tooltip("disable debug console")]
        private bool disabled = true;
        [MyConditional("disabled", false, false)]
        [SerializeField, Tooltip("enable timestamp")]
        private bool timestamp = true;
        [MyConditional("disabled", false, false)]
        [SerializeField, Tooltip("cover ratio of screen")]
        private Rect rect = new Rect(0.03f, 0.03f, 0.94f, 0.94f);
        [MyConditional("disabled", false, false)]
        [SerializeField, Tooltip("text color")]
        private Color color = new Color(0.0f, 1.0f, 0.0f);
        [MyConditional("disabled", false, false)]
        [SerializeField, Tooltip("font scale 0~1.0")]
        private float fontScale = 0.03f;

        /// <summary>
        /// output hook
        /// </summary>
        public static event Action<string> Output;

        /// <summary>
        /// disable console
        /// </summary>
        public static bool Disabled
        {
            get
            {
                var ins = instance;
                if (ins == null)
                {
                    return true;
                }
                return ins.disabled;
            }
            set
            {
                var ins = instance;
                if (ins == null)
                {
                    return;
                }
                ins.disabled = value;
            }
        }
        /// <summary>
        /// enable timestamp
        /// </summary>
        public static bool Timestamp
        {
            get
            {
                var ins = instance;
                if (ins == null)
                {
                    return true;
                }
                return ins.timestamp;
            }
            set
            {
                var ins = instance;
                if (ins == null)
                {
                    return;
                }
                ins.timestamp = value;
            }
        }

        private static string getTimestamp()
        {
            uint t = (uint)(DateTime.UtcNow.Ticks / 10000);  // 100nsec -> msec
            uint min = (t / (60 * 1000)) % 60;
            uint sec = (t / (1000)) % 60;
            uint msec = (t % 1000);
            return min.ToString("d2") + ":" + sec.ToString("d2") + ":" + msec.ToString("d3") + " ";
        }
        private static void output(string str)
        {
            if (Output != null)
            {
                Output.Invoke(str);
            }
            lock (lines)
            {
                lines.Add(str);
                if (lines.Count > MaxNumLines)
                {
                    lines.RemoveAt(0);
                }
            }
        }
        private static void outputLines(string[] lines, int count)
        {
            if (count > lines.Length)
            {
                count = lines.Length;
            }
            if (count <= 0)
            {
                return;
            }
#if true//DEBUG
            if (Timestamp)
            {
                string t = getTimestamp();
                for (int i = 0; i < count; i++)
                {
                    output(t + lines[i]);
                }
            }
            else
#endif
            {
                for (int i = 0; i < count; i++)
                {
                    output(lines[i]);
                }
            }
        }
        /// <summary>
        /// write line
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="args"></param>
        public static void WriteLine(string frm, params object[] args)
        {
            if (frm == null)
            {
                return;
            }
            {
                var builder = MyFountain<StringBuilder>.Get(() => new StringBuilder());
                builder.AppendFormat(frm, args);
                var str = builder.ToString();
                builder.Length = 0;
                MyFountain<StringBuilder>.Put(builder);

                string[] lines;
                lock (temporary)
                {
                    str = temporary + str;
                    lines = str.Split('\n');
                    temporary = "";
                }
                outputLines(lines, lines.Length);
            }
        }
        /// <summary>
        /// write
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="args"></param>
        public static void Write(string frm, params object[] args)
        {
            if (frm == null)
            {
                return;
            }
            {
                var builder = MyFountain<StringBuilder>.Get(() => new StringBuilder());
                builder.AppendFormat(frm, args);
                var str = builder.ToString();
                builder.Length = 0;
                MyFountain<StringBuilder>.Put(builder);

                string[] lines;
                lock (temporary)
                {
                    str = temporary + str;
                    lines = str.Split('\n');
                    temporary = lines[lines.Length - 1];
                }
                outputLines(lines, lines.Length - 1);
            }
        }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
        }
        private void OnGUI()
        {
            if (Disabled)
            {
                return;
            }
            var style = GUI.skin.GetStyle("label");
            var oldFontSize = style.fontSize;
            style.fontSize = (int)(Screen.height * fontScale);
            var fh = (int)Mathf.Ceil(style.lineHeight);
            var h = ((int)(Screen.height * rect.height) - fh + 1) / fh;

            string str;
            {
                var builder = MyFountain<StringBuilder>.Get(() => new StringBuilder());
                lock (lines)
                {
                    var end = lines.Count;
                    var begin = end - h;
                    if (begin < 0)
                    {
                        begin = 0;
                    }
                    for (int i = begin; i < end; i++)
                    {
                        builder.AppendLine(lines[i]);
                    }
                }
                str = builder.ToString();
                builder.Length = 0;
                MyFountain<StringBuilder>.Put(builder);
            }

            var r = new Rect(Screen.width * rect.xMin, Screen.height * rect.yMin, Screen.width * rect.width, Screen.height * rect.height);
            var prevColor = GUI.color;
            GUI.color = color;
            GUI.Label(r, str);
            GUI.color = prevColor;
            style.fontSize = oldFontSize;
        }
    }
}
