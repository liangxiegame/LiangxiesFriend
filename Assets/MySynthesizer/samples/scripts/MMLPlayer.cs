using UnityEngine;
using UnityEngine.UI;

using MySpace.Synthesizer;

namespace MySpace.Sample
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(MyMMLBox))]
    public class MMLPlayer : MonoBehaviour
    {
        [SerializeField]
        private TextAsset sample1 = null;
        [SerializeField]
        private TextAsset sample2 = null;
        [SerializeField]
        private TextAsset sample3 = null;
        [SerializeField]
        private TextAsset sample4 = null;

        private MyMMLPlayer player = null;
        private SampleTools sampleTools = null;
        private GameObject ToneEditorCanvas = null;
        private GameObject ToneEditorCT8Canvas = null;
        private Text playButtonText = null;
        private Text cursolPosText = null;
        private InputField mmlField = null;
        private Text consoleText = null;
        private Text instructionText = null;
        private readonly System.Text.StringBuilder stb = new System.Text.StringBuilder();
        private int lineCount = 0;
        private bool invalid = true;
        private bool playing = false;

        private MyMMLBox mmlBox = null;
        private MyMMLClip mmlClip = new MyMMLClip();

        public void SetMMLText(string mml)
        {
            mmlBox.Play("Click");
            mmlField.text = (mml != null) ? mml : "";
            if (!playing)
            {
                consoleText.text = "";
            }
        }
        public void LoadSample1()
        {
            mmlBox.Play("Click");
            mmlField.text = (sample1 != null) ? sample1.text : "";
            if (!playing)
            {
                consoleText.text = "";
            }
        }
        public void LoadSample2()
        {
            mmlBox.Play("Click");
            mmlField.text = (sample2 != null) ? sample2.text : "";
            if (!playing)
            {
                consoleText.text = "";
            }
        }
        public void LoadSample3()
        {
            mmlBox.Play("Click");
            mmlField.text = (sample3 != null) ? sample3.text : "";
            if (!playing)
            {
                consoleText.text = "";
            }
        }
        public void LoadSample4()
        {
            mmlBox.Play("Click");
            mmlField.text = (sample4 != null) ? sample4.text : "";
            if (!playing)
            {
                consoleText.text = "";
            }
        }
        private void resetStb()
        {
            lock (stb)
            {
                lineCount = 0;
                stb.Remove(0, stb.Length);
            }
        }
        private void appendStb(string str)
        {
            lock (stb)
            {
                if (lineCount >= 100)
                {
                    char[] cbuf = new char[1];
                    for (var i = 0; i < stb.Length; i++)
                    {
                        stb.CopyTo(i, cbuf, 0, 1);
                        if (cbuf[0] == '\n')
                        {
                            stb.Remove(0, i + 1);
                            break;
                        }
                    }
                    lineCount--;
                }
                stb.AppendLine(str);
                lineCount++;
            }
        }

        private void appDataEventFunc(MyMMLSequencer.EventLocation loc, string data)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(data);
            appendStb(sb.ToString());
        }
        private void playingEventFunc(MyMMLSequencer.EventLocation loc, uint step, uint gate, MyMMLSequence.Instruction inst)
        {
            var str = loc.TrackNo.ToString("D2") + " " + loc.MeasureCount.ToString("D3") + ":" + loc.TickCount.ToString("D3") + " " + step.ToString("D3") + ":" + gate.ToString("D3") + " ";
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
            appendStb(str);
        }
        private void Awake()
        {
            player = GameObject.FindObjectOfType<MyMMLPlayer>();
            sampleTools = GameObject.FindObjectOfType<SampleTools>();
            mmlBox = sampleTools.GetComponent<MyMMLBox>();
            player.AppDataEvent += appDataEventFunc;
            player.PlayingEvent += playingEventFunc;
#if false
        {
            string mml = "$@(h_close5) = @pm8[4 7 op1[0 0 15 0 7 0 0 0 0 31 0 0 0 0] op2[0 0 0 0 7 0 0 1 0 21 18 5 15 13] op3[0 0 8 0 3 0 0 0 0 31 0 0 0 14] op4[0 0 15 0 3 0 1 1 5 31 17 15 13 9] lfo[0 127 0 0 31 0 0 0 0] ]\nt0=@(h_close5)a32c32";
            MyMMLClip clip = new MyMMLClip("Click", mml);
            clip.GenerateAudioClip = true;
            mmlBox.Add(clip);
        }
#endif
            invalid = true;
            playing = false;
            ToneEditorCanvas = transform.parent.Find("ToneEditorCanvas").gameObject;
            {
                var button = transform.Find("Panel/ToneEditorButton").GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    mmlBox.Play("Click");
                    sampleTools.MoveCameraTo(ToneEditorCanvas.transform.localPosition);
                    ToneEditorCanvas.GetComponent<CanvasGroup>().interactable = true;
                    this.GetComponent<CanvasGroup>().interactable = false;
                });
            }
            ToneEditorCT8Canvas = transform.parent.Find("ToneEditorCT8Canvas").gameObject;
            {
                var button = transform.Find("Panel/ToneEditorCT8Button").GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    mmlBox.Play("Click");
                    sampleTools.MoveCameraTo(ToneEditorCT8Canvas.transform.localPosition);
                    ToneEditorCT8Canvas.GetComponent<CanvasGroup>().interactable = true;
                    this.GetComponent<CanvasGroup>().interactable = false;
                });
            }
            {
                var button = transform.Find("Panel/PlayButton").GetComponent<Button>();
                cursolPosText = transform.Find("Panel/CursolPosText").GetComponent<Text>();
                mmlField = transform.Find("Panel/MMLField").GetComponent<InputField>();
                consoleText = transform.Find("Panel/ConsolePanel").GetComponentInChildren<Text>();
                instructionText = transform.Find("Panel/InstructionPanel").GetComponentInChildren<Text>();
                playButtonText = button.GetComponentInChildren<Text>();
#if UNITY_5_3_OR_NEWER
                mmlField.onValueChanged.AddListener((str) =>
#else
                mmlField.onValueChange.AddListener((str) =>
#endif
                {
                    invalid = true;
                    if (player.Playing)
                    {
                        player.Stop();
                        playButtonText.text = "Play";
                        consoleText.text = "";
                        playing = false;
                    }
                    else
                    {
                        playButtonText.text = "Play";
                    }
                });
                button.onClick.AddListener(() =>
                {
                    if (invalid)
                    {
                        if (player.Playing && (player.Clip == mmlClip))
                        {
                            player.Stop();
                            playButtonText.text = "Play";
                            consoleText.text = "";
                            playing = false;
                        }
                        else
                        {
                            mmlClip.TextB = mmlField.text;
                            player.Play(mmlClip);
                            resetStb();
                            invalid = false;
                            consoleText.text = "Playing...";
                            playButtonText.text = "Pause";
                            playing = true;
                        }
                    }
                    else
                    {
                        if (player.Playing)
                        {
                            player.Pause();
                            consoleText.text = "";
                            playButtonText.text = "Continue";
                            playing = false;
                        }
                        else
                        {
                            player.Continue();
                            consoleText.text = "Playing...";
                            playButtonText.text = "Pause";
                            playing = true;
                        }
                    }
                });
            }
        }
        private void Update()
        {
            if ((playing && !player.Playing) || (player.Clip != mmlClip))
            {
                invalid = true;
                playing = false;
                playButtonText.text = "Play";
                consoleText.text = mmlClip.Error;
            }
            if (playing)
            {
                string str;
                lock (stb)
                {
                    str = stb.ToString();
                }
                instructionText.text = str;
            }
#if UNITY_5_1_OR_NEWER
            {
                int line = 1;
                int pos = 1;
                for (int i = 0; i < mmlField.selectionFocusPosition; i++)
                {
                    if (mmlField.text[i] == '\n')
                    {
                        line++;
                        pos = 0;
                    }
                    pos++;
                }
                cursolPosText.text = line.ToString() + ":" + pos.ToString();
            }
#else
            cursolPosText.text = "";
#endif
        }
        private void OnDestroy()
        {
            player.AppDataEvent -= appDataEventFunc;
            player.PlayingEvent -= playingEventFunc;
            player = null;
            sampleTools = null;
            mmlBox = null;
        }
    }
}
