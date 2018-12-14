using UnityEngine;

using MySpace.Synthesizer;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace MySpace.Sample
{
    [RequireComponent(typeof(MyMMLPlayer))]
    public class MMLTesterSample : MonoBehaviour
    {
        private MyMMLPlayer player = null;

        private List<string> lines = new List<string>();
        private StringBuilder sb = new StringBuilder();
        private Text monitor;
        private Text result;
        private InputField inputField1;
        private InputField inputField2;
        private InputField inputField3;
        private int portNumber;
        private bool loop = false;
        private MyMMLClip clip = null;
        private void Awake()
        {
            player = GetComponent<MyMMLPlayer>();
            inputField1 = transform.Find("Panel/InputField 1").GetComponent<InputField>();
            inputField2 = transform.Find("Panel/InputField 2").GetComponent<InputField>();
            inputField3 = transform.Find("Panel/InputField 3").GetComponent<InputField>();
            transform.Find("Panel/Toggle Port 0").GetComponent<Toggle>().onValueChanged.AddListener(value => { if (value) portNumber = 0; });
            transform.Find("Panel/Toggle Port 1").GetComponent<Toggle>().onValueChanged.AddListener(value => { if (value) portNumber = 1; });
            transform.Find("Panel/Toggle Port 2").GetComponent<Toggle>().onValueChanged.AddListener(value => { if (value) portNumber = 2; });
            transform.Find("Panel/Toggle Port 3").GetComponent<Toggle>().onValueChanged.AddListener(value => { if (value) portNumber = 3; });
            transform.Find("Panel/Toggle Loop").GetComponent<Toggle>().onValueChanged.AddListener(value => loop = value);
            transform.Find("Panel/Slider Volume").GetComponent<Slider>().onValueChanged.AddListener(value =>
            {
                player.Volume = value;
            });
            transform.Find("Panel/Button Play").GetComponent<Button>().onClick.AddListener(()=>
            {
                player.Stop();
                lines.Clear();
                clip = player.Play(portNumber, 0, inputField1.text, inputField2.text, inputField3.text);
                clip.Loop = loop;
            });
            transform.Find("Panel/Button Stop").GetComponent<Button>().onClick.AddListener(() =>
            {
                player.Stop();
                clip = null;
            });
            transform.Find("Panel/Button Sample1").GetComponent<Button>().onClick.AddListener(() =>
            {
                inputField1.text = "c2e2g2c1:e1: g1d2f2a2d1: r4: r2f2.a2r2";
                inputField2.text = "";
                inputField3.text = "/* chord test!*/";
            });
            transform.Find("Panel/Button Sample2").GetComponent<Button>().onClick.AddListener(() =>
            {
                inputField1.text = "/* Froschgesang */!t88[l8b-]fgab|agfr|ab<cd|c>bar|frfrfrfr|l16ffggaabb|lagfr";
                inputField2.text = "[l8b-]r2|r2|fgab|agfr|ab<cd|c>bar|frfrfrfr|l16ffggaabb|lagfr";
                inputField3.text = "[l8b-]r2|r2|r2|r2|fgab|agfr|ab<cd|c>bar|frfrfrfr|l16ffggaabb|lagfr";
            });
            transform.Find("Panel/Button Sample3").GetComponent<Button>().onClick.AddListener(() =>
            {
                //inputField1.text = "@<128,0>v127l8 { n42:n36 n42:n36 n42:n38 n42:n36 n42:n36 n42:n36 n42:n38 n42:n36 }4 n49,1";
                inputField1.text = "@<128,0>v127l8 { n42:n36 n42 n42:n38 n42 n42:n36 n42:n36 n42:n38 n42 }4 n49,1";
                inputField2.text = "/*this is for GM tone set*/";
                inputField3.text = "/* please import any GM compatible sf2 file and set to PresetToneTemplate.mml.txt*/";
            });
            transform.Find("Panel/Button Clear").GetComponent<Button>().onClick.AddListener(() =>
            {
                inputField1.text = "";
                inputField2.text = "";
                inputField3.text = "";
            });
            monitor = transform.Find("Panel/Text Monitor").GetComponentInChildren<Text>();
            result = transform.Find("Panel/Text Result").GetComponentInChildren<Text>();

            player.PlayingEvent += playingEvent;
        }
        private void Update()
        {
            if(clip != null)
            {
                result.text = clip.Error;
            }
            {
                sb.Length = 0;
                lock (lines)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        sb.AppendLine(lines[i]);
                    }
                }
                monitor.text = sb.ToString();
            }
        }
        private uint convertTimeBase(uint tick)
        {
            return tick * 96 / player.TimeBase;
        }
        private void playingEvent(MyMMLSequencer.EventLocation loc, uint step, uint gate, MyMMLSequence.Instruction inst)
        {
            var str =
                /*loc.Port.ToString("D2") + ":" +
                loc.Channel.ToString("D2") + " " +*/
                loc.TrackNo.ToString("D2") + " " +
                loc.MeasureCount.ToString("D3") + ":" +
                convertTimeBase(loc.TickCount).ToString("D3") + " " +
                convertTimeBase(step).ToString("D3") + ":" +
                convertTimeBase(gate).ToString("D3") + " ";
            if ((int)inst.N == 0)
            {
                str += inst.N.ToString();
            }
            else if ((int)inst.N < 128)
            {
                char c0 = "CCDDEFFGGAAB"[(int)inst.N % 12];
                char c1 = " # #  # # # "[(int)inst.N % 12];
                int oct = ((int)inst.N / 12) - 2;
                str += c0.ToString() + c1.ToString() + oct.ToString() + " " + inst.V.ToString("D3");
            }
            else
            {
                str += inst.N.ToString() + " <0x" + (inst.V | ((int)inst.Q << 8)).ToString("X4") + ">";
            }
            lock (lines)
            {
                lines.Add(str);
                if (lines.Count > 16)
                {
                    lines.RemoveAt(0);
                }
            }
        }
        private void OnDestroy()
        {
            player.PlayingEvent -= playingEvent;
            player = null;
        }
    }
}
