
using System;

using UnityEngine;
using UnityEngine.UI;


namespace MySpace.Sample
{
    using ToneParamCT8 = MySpace.Synthesizer.MySynthesizerCT8.ToneParam;

    public class ToneEditorCT8 : MonoBehaviour
    {
        private MySyntheStation syntheStation = null;
        private SampleTools sampleTools = null;
        private MyMMLBox mmlBox = null;
        private ToneParamCT8 param = null;
        private InputField inputField = null;

        private bool disabledEvent = false;

        [SerializeField]
        private int portNo = 2;
        [SerializeField]
        private int chNo = 15;


        private void setSliderValue(GameObject obj, string name, string label, float min, float max, int val)
        {
            var text = obj.transform.Find(name + "/Text").GetComponent<Text>();
            var slider = obj.transform.Find(name + "/Slider").GetComponent<Slider>();
            disabledEvent = true;
            text.text = label + val;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = val;
            disabledEvent = false;
        }
        private void setupSlider(GameObject obj, string name, string label, Action<float> apply)
        {
            var text = obj.transform.Find(name + "/Text").GetComponent<Text>();
            var slider = obj.transform.Find(name + "/Slider").GetComponent<Slider>();
            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener((value) =>
            {
                if (!disabledEvent)
                {
                    text.text = label + (int)value;
                    apply(value);
                }
            });
        }
        private void updateParam()
        {
            var ts = new Synthesizer.ToneSet();
            ts.Add(param.Clone());
            syntheStation.Synthesizers[portNo].Channel[chNo].ProgramChange(ts);
            inputField.text = param.ToString();
        }

        private void loadTone(ToneParamCT8 tone)
        {
            param = (ToneParamCT8)tone.Clone();
            {
                var obj = transform.Find("Panel/LFO").gameObject;
                setSliderValue(obj, "WaveForm", "lw:", 0, 7, param.Lfo.WS);
                setSliderValue(obj, "Frequency", "lf:", 0, 127, param.Lfo.LF);
                setSliderValue(obj, "PMPower", "lp:", 0, 127, param.Lfo.LP);
                setSliderValue(obj, "AMPower", "la:", 0, 127, param.Lfo.LA);
                if (param.Extended)
                {
                    setSliderValue(obj, "AttackRate", "ar:", 0, 127, param.Lfo.Env.ExAR);
                    setSliderValue(obj, "DecayRate", "dr:", 0, 127, param.Lfo.Env.ExDR);
                    setSliderValue(obj, "SustainLevel", "sl:", 0, 127, param.Lfo.Env.ExSL);
                    setSliderValue(obj, "SustainRate", "sr:", 0, 127, param.Lfo.Env.ExSR);
                    setSliderValue(obj, "ReleaseRate", "rr:", 0, 127, param.Lfo.Env.ExRR);
                }
                else
                {
                    setSliderValue(obj, "AttackRate", "ar:", 0, 31, param.Lfo.Env.AR);
                    setSliderValue(obj, "DecayRate", "dr:", 0, 31, param.Lfo.Env.DR);
                    setSliderValue(obj, "SustainLevel", "sl:", 0, 15, param.Lfo.Env.SL);
                    setSliderValue(obj, "SustainRate", "sr:", 0, 31, param.Lfo.Env.SR);
                    setSliderValue(obj, "ReleaseRate", "rr:", 0, 15, param.Lfo.Env.RR);
                }
            }
            {
                var obj = transform.Find("Panel/ENV").gameObject;
                setSliderValue(obj, "WaveStyle", "ws:", 0, 7, param.WS);
                setSliderValue(obj, "KeyScale", "ks:", 0, 3, param.Env.KS);
                setSliderValue(obj, "VelocitySense", "vs:", 0, 7, param.Env.VS);
                setSliderValue(obj, "TotalLevel", "tl:", 0, 127, param.Env.TL);
                if (param.Extended)
                {
                    setSliderValue(obj, "AttackRate", "ar:", 0, 127, param.Env.ExAR);
                    setSliderValue(obj, "DecayRate", "dr:", 0, 127, param.Env.ExDR);
                    setSliderValue(obj, "SustainLevel", "sl:", 0, 127, param.Env.ExSL);
                    setSliderValue(obj, "SustainRate", "sr:", 0, 127, param.Env.ExSR);
                    setSliderValue(obj, "ReleaseRate", "rr:", 0, 127, param.Env.ExRR);
                }
                else
                {
                    setSliderValue(obj, "AttackRate", "ar:", 0, 31, param.Env.AR);
                    setSliderValue(obj, "DecayRate", "dr:", 0, 31, param.Env.DR);
                    setSliderValue(obj, "SustainLevel", "sl:", 0, 15, param.Env.SL);
                    setSliderValue(obj, "SustainRate", "sr:", 0, 31, param.Env.SR);
                    setSliderValue(obj, "ReleaseRate", "rr:", 0, 15, param.Env.RR);
                }
            }
            {
                var wt = transform.Find("Panel/WaveTable").GetComponent<WaveTable>();
                for (int i = 0; i < 32; i++)
                {
                    wt.WT[i] = param.WT[i];
                }
            }
            updateParam();
        }
        private void Awake()
        {
            syntheStation = GameObject.FindObjectOfType<MySyntheStation>();
            sampleTools = GameObject.FindObjectOfType<SampleTools>();
            mmlBox = sampleTools.GetComponent<MyMMLBox>();
            {
                var extend = transform.Find("Panel/Extend").GetComponent<Button>();
                extend.onClick.AddListener(() =>
                {
                    param.Extended = !param.Extended;
                    updateParam();
                    loadTone(param);
                });
            }
            {
                inputField = transform.Find("Panel/InputField").GetComponent<InputField>();
                inputField.onEndEdit.AddListener((v) =>
                {
                    var tone = new ToneParamCT8();
                    if (tone.LoadFromString(v))
                    {
                        loadTone(tone);
                    }
                });

                var presets = GetComponentsInChildren<PresetButton>();
                foreach (var pb in presets)
                {
                    var btn = pb.GetComponent<Button>();
                    var tp = new ToneParamCT8(pb.ToneData);
                    btn.onClick.AddListener(() =>
                    {
                        loadTone(tp);
                    });
                }
            }
            {
                var obj = transform.Find("Panel/LFO").gameObject;
                setupSlider(obj, "WaveForm", "lw:", (val) => { param.Lfo.WS = (Byte)val; updateParam(); });
                setupSlider(obj, "Frequency", "lf:", (val) => { param.Lfo.LF = (Byte)val; updateParam(); });
                setupSlider(obj, "PMPower", "lp:", (val) => { param.Lfo.LP = (Byte)val; updateParam(); });
                setupSlider(obj, "AMPower", "la:", (val) => { param.Lfo.LA = (Byte)val; updateParam(); });
                setupSlider(obj, "AttackRate", "ar:", (val) => { param.Lfo.Env.AR = (Byte)val; updateParam(); });
                setupSlider(obj, "DecayRate", "dr:", (val) => { param.Lfo.Env.DR = (Byte)val; updateParam(); });
                setupSlider(obj, "SustainLevel", "sl:", (val) => { param.Lfo.Env.SL = (Byte)val; updateParam(); });
                setupSlider(obj, "SustainRate", "sr:", (val) => { param.Lfo.Env.SR = (Byte)val; updateParam(); });
                setupSlider(obj, "ReleaseRate", "rr:", (val) => { param.Lfo.Env.RR = (Byte)val; updateParam(); });
            }
            {
                var obj = transform.Find("Panel/ENV").gameObject;
                setupSlider(obj, "WaveStyle", "ws:", (val) => { param.WS = (Byte)val; updateParam(); });
                setupSlider(obj, "KeyScale", "ks:", (val) => { param.Env.KS = (Byte)val; updateParam(); });
                setupSlider(obj, "VelocitySense", "vs:", (val) => { param.Env.VS = (Byte)val; updateParam(); });
                setupSlider(obj, "TotalLevel", "tl:", (val) => { param.Env.TL = (Byte)val; updateParam(); });
                setupSlider(obj, "AttackRate", "ar:", (val) => { param.Env.AR = (Byte)val; updateParam(); });
                setupSlider(obj, "DecayRate", "dr:", (val) => { param.Env.DR = (Byte)val; updateParam(); });
                setupSlider(obj, "SustainLevel", "sl:", (val) => { param.Env.SL = (Byte)val; updateParam(); });
                setupSlider(obj, "SustainRate", "sr:", (val) => { param.Env.SR = (Byte)val; updateParam(); });
                setupSlider(obj, "ReleaseRate", "rr:", (val) => { param.Env.RR = (Byte)val; updateParam(); });
            }
            {
                var wt = transform.Find("Panel/WaveTable").GetComponent<WaveTable>();
                wt.OnUpdateAction += (idx) =>
                {
                    param.WT[idx] = wt.WT[idx];
                    updateParam();
                };
            }
            {
                var keyboard = transform.Find("Panel/Keyboard").GetComponent<Keyboard>();
                keyboard.ChNo = chNo;
                keyboard.PortNo = portNo;
            }
            {
                var mmlEditorCanvas = transform.parent.Find("MMLPlayerCanvas");
                var button = transform.Find("Panel/MMLEditorButton").GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    mmlBox.Play("Click");
                    sampleTools.MoveCameraTo(mmlEditorCanvas.localPosition);
                    mmlEditorCanvas.GetComponent<CanvasGroup>().interactable = true;
                    this.GetComponent<CanvasGroup>().interactable = false;
                });
            }
        }
        private void Start()
        {
            loadTone(new ToneParamCT8());
            this.GetComponent<CanvasGroup>().interactable = false;
        }
    }

}
