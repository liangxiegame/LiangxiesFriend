using UnityEngine;

using MySpace.Synthesizer;

namespace MySpace.Sample
{
    [RequireComponent(typeof(MyMMLBox))]
    public class MMLBoxSample : MonoBehaviour
    {
        private MyMMLBox mmlBox = null;

        private void debugOut(string str)
        {
            DebugConsole.WriteLine(str);
        }
        private void playingEvent(MyMMLSequencer.EventLocation loc, uint step, uint gate, MyMMLSequence.Instruction inst)
        {
            var str = loc.Port.ToString("D2") + ":" + loc.Channel.ToString("D2") + " " + loc.TrackNo.ToString("D2") + " " + loc.MeasureCount.ToString("D3") + ":" + loc.TickCount.ToString("D3") + " " + step.ToString("D3") + ":" + gate.ToString("D3") + " ";
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
            debugOut(str);
        }
        private void OnEnable()
        {
            mmlBox = GetComponent<MyMMLBox>();
            mmlBox.Player.PlayingEvent += playingEvent;
            MySyntheStation.DebugOut += debugOut;
        }
        private void OnDisable()
        {
            MySyntheStation.DebugOut -= debugOut;
            mmlBox.Player.PlayingEvent -= playingEvent;
            mmlBox = null;
        }

        private void OnGUI()
        {
            var l = 0.5f;
            var r = 0.9f;
            var t = 0.1f;
            var b = 0.9f;
            GUILayout.BeginArea(new Rect(Screen.width * l, Screen.height * t, Screen.width * (r - l), Screen.height * (b - t)));
            GUILayout.Label("(AC): generate audio clip");
            var guiEnabled = GUI.enabled;
            foreach (var ic in mmlBox)
            {
                GUI.enabled = ic.Clip.Valid;
                var name = ic.Clip.Name;
                if (ic.Clip.GenerateAudioClip)
                {
                    name += " (AC)";
                }
                if (GUILayout.Button(name))
                {
                    mmlBox.Play(ic.Index);
                }
            }
            GUI.enabled = guiEnabled;
            if (GUILayout.Button("RandomMiaow"))
            {
                mmlBox.Play("Miaow");
            }
            if (GUILayout.Button("Stop"))
            {
                mmlBox.Stop();
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            GUILayout.Label("NOTICE!!\nUnityWebGL can't get audio data.\nYou can not play Miaow, Flohwalzer.");
#endif
            GUILayout.EndArea();
        }
    }
}
