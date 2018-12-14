#if UNITY_WEBGL && !UNITY_EDITOR
#   define DISABLE_TASK
#   define USE_WEBGL_AUDIO_STREAMING
#endif
#if WINDOWS_UWP || (UNITY_2017_OR_NEWER && !(NET_2_0 || NET_2_0_SUBSET))
#   define USE_SYSTEM_THREADING_TASKS
#endif

using UnityEngine;
using System;
//using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
#if USE_SYSTEM_THREADING_TASKS
using System.Threading.Tasks;
#else
using MySpace.Tasks;
#endif
using MySpace.Synthesizer;

using Conditional = System.Diagnostics.ConditionalAttribute;

namespace MySpace
{
    public class MyMMLSequenceUnit
    {
        private ToneMap toneMap;
        private MyMMLSequence sequence;
        private string error;
        public MyMMLSequenceUnit(ToneMap toneMap, MyMMLSequence sequence)
        {
            this.toneMap = toneMap;
            this.sequence = sequence;
            this.error = null;
        }
        public MyMMLSequenceUnit(string error)
        {
            this.toneMap = null;
            this.sequence = null;
            this.error = error;
        }
        public ToneMap ToneMap
        {
            get
            {
                return toneMap;
            }
        }
        public MyMMLSequence Sequence
        {
            get
            {
                return sequence;
            }
        }
        public string Error
        {
            get
            {
                return (error != null) ? error : "";
            }
        }
    }
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(AudioListener))]
    [DisallowMultipleComponent]
    [AddComponentMenu("MySynthesizer/MySyntheStation")]
    public class MySyntheStation : MonoBehaviour
    {
        public enum SyntheTypes
        {
            PM8A,
            SS8A,
            CT8A,
            SF2A,
        }
        [SerializeField]
        private uint numVoices = 8;
        [SerializeField]
        private uint baseFrequency = 31250;
        [SerializeField]
        private uint tickFrequency = 90;
        [SerializeField]
        private uint mixingBufferLength = 200;
        [SerializeField, Range(0.0f, 1.0f)]
        private float masterVolume = 1.0f;

        [SerializeField]
        private TextAsset PresetTone = null;

        private IEnumerator Provider(LinkedList<IEnumerator> jobs)
        {
            for (;;)
            {
                if (jobs.Count != 0)
                {
#if true
#if UNITY_5_6_OR_NEWER
                    yield return jobs.First.Value;
#else
                    var ie = jobs.First.Value;
                    while (ie.MoveNext())
                    {
                        yield return ie.Current;
                    }
#endif
                    jobs.RemoveFirst();
#else
                    var ie = jobs.First.Value;
                    for (;;)
                    {
                        try
                        {
                            if (!ie.MoveNext())
                            {
                                break;
                            }
                        }
                        catch (Exception ec)
                        {
                            UnityEngine.Debug.LogException(ec);
                            break;
                        }
                        yield return ie.Current;
                    }
                    jobs.RemoveFirst();
#endif
                }
                yield return null;
            }
        }
        private struct AudioClipGeneratorState
        {
            public const int frequency = 44100;
            public const int numChannels = 2;
            public volatile MyMixer mixer;
            public List<MySynthesizer> synthesizers;
            public MyMMLSequencer sequencer;
        }
        private MyMixer mixer = null;
        private List<MyMMLSequencer> sequencersCache = null;
        private ReadOnlyCollection<MySynthesizer> readonlySynthesizers;
        private readonly List<MySynthesizer> synthesizers = new List<MySynthesizer>();
        private readonly List<MyMMLSequencer> sequencers = new List<MyMMLSequencer>();
        private readonly LinkedList<IEnumerator> jobListA = new LinkedList<IEnumerator>();
        private readonly LinkedList<IEnumerator> jobListB = new LinkedList<IEnumerator>();
        private IEnumerator coProviderA = null;
        private IEnumerator coProviderB = null;
        private AudioClipGeneratorState acgState;
        private volatile bool seqChanged = false;

#if USE_WEBGL_AUDIO_STREAMING
        private MyWebAudioStreamer myWebAudioFilter = null;
#endif
        private void Tick()
        {
#if UNITY_EDITOR
            mixer.MasterVolume = masterVolume;
#endif
            if (seqChanged)
            {
                sequencersCache.Clear();
                lock (sequencers)
                {
                    seqChanged = false;
                    for (int i = 0; i < sequencers.Count; i++)
                    {
                        sequencersCache.Add(sequencers[i]);
                    }
                }
            }
            for (int i = 0; i < sequencersCache.Count; i++)
            {
                sequencersCache[i].Tick();
            }
        }
        public void AddSequencer(MyMMLSequencer sequencer)
        {
            lock (sequencer)
            {
                sequencers.Add(sequencer);
            }
            seqChanged = true;
        }
        public void RemoveSequencer(MyMMLSequencer sequencer)
        {
            lock (sequencers)
            {
                sequencers.Remove(sequencer);
            }
            seqChanged = true;
        }
        public ReadOnlyCollection<MySynthesizer> Synthesizers
        {
            get
            {
                return readonlySynthesizers;
            }
        }
        public float MixerVolume
        {
            get
            {
                return masterVolume;
            }
            set
            {
                masterVolume = value;
                if (mixer != null)
                {
                    mixer.MasterVolume = value;
                }
            }
        }
        public uint TickFrequency
        {
            get
            {
                return tickFrequency;
            }
        }
#if UNITY_EDITOR
        public bool LivingDead
        {
            get;
            private set;
        }
#endif
        public static event Action<string> DebugOut = null;

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void DebugLog(string str)
        {
            if (DebugOut != null)
            {
                DebugOut(str);
            }
            //Debug.Log(str);
        }

        private void SetupPresetTones()
        {
            if ((PresetTone == null) || string.IsNullOrEmpty(PresetTone.text))
            {
                return;
            }
            var clip = new MyMMLClip();
            clip.Name = PresetTone.name;
            clip.TextA = PresetTone;
            PrepareClip(clip, (c) =>
            {
                if (c.Unit == null)
                {
                    return;
                }
                var toneMap = c.Unit.ToneMap;
                if (toneMap == null)
                {
                    return;
                }
                if (toneMap.Count != 0)
                {
                    foreach (var syn in synthesizers)
                    {
                        syn.AddToneMap(toneMap);
                    }
                    foreach (var syn in acgState.synthesizers)
                    {
                        syn.AddToneMap(toneMap);
                    }
                }
            });
        }
        private void OnEnable()
        {
            MyDebugOutput.Output += Debug.Log;
            //UnityEngine.Debug.Log("OnEnable");
            sequencersCache = new List<MyMMLSequencer>();
            mixer = new MyMixer((uint)AudioSettings.outputSampleRate, false, mixingBufferLength, baseFrequency, tickFrequency);
            mixer.TickCallback += Tick;
            mixer.MasterVolume = masterVolume;
            readonlySynthesizers = synthesizers.AsReadOnly();

            acgState.mixer = new MyMixer(AudioClipGeneratorState.frequency, true, mixingBufferLength, baseFrequency, tickFrequency);
            acgState.sequencer = new MyMMLSequencer(acgState.mixer.TickFrequency);
            acgState.mixer.TickCallback += acgState.sequencer.Tick;
            acgState.mixer.MasterVolume = 1.0f;
            acgState.synthesizers = new List<MySynthesizer>();

            foreach (var i in Enum.GetValues(typeof(SyntheTypes)))
            {
                switch ((SyntheTypes)i)
                {
                    case SyntheTypes.PM8A:
                        synthesizers.Add(new MySynthesizerPM8(mixer, numVoices));
                        acgState.synthesizers.Add(new MySynthesizerPM8(acgState.mixer, numVoices));
                        break;
                    case SyntheTypes.SS8A:
                        synthesizers.Add(new MySynthesizerSS8(mixer, numVoices));
                        acgState.synthesizers.Add(new MySynthesizerSS8(acgState.mixer, numVoices));
                        break;
                    case SyntheTypes.CT8A:
                        synthesizers.Add(new MySynthesizerCT8(mixer, numVoices));
                        acgState.synthesizers.Add(new MySynthesizerCT8(acgState.mixer, numVoices));
                        break;
                    case SyntheTypes.SF2A:
                        synthesizers.Add(new MySynthesizerSF2(mixer, numVoices));
                        acgState.synthesizers.Add(new MySynthesizerSF2(acgState.mixer, numVoices));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            coProviderA = Provider(jobListA);
            this.StartCoroutineEx(coProviderA);
            coProviderB = Provider(jobListB);
            this.StartCoroutineEx(coProviderB);

#if !DISABLE_TASK
            StartUpdateLoopTask();
#endif
#if UNITY_EDITOR
            LivingDead = true;
#endif

#if USE_WEBGL_AUDIO_STREAMING
            int bufferLength;
            int numBuffers;
            AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
#if true
            if(numBuffers < 4)
            {
                numBuffers = 4;
            }
            if(bufferLength < 1024)
            {
                bufferLength = 1024;
            }
#endif
            myWebAudioFilter = new MyWebAudioStreamer(bufferLength, numBuffers, AudioSettings.outputSampleRate, OnAudioFilterRead);
#endif
            SetupPresetTones();
        }
        private void OnDisable()
        {
#if USE_WEBGL_AUDIO_STREAMING
            myWebAudioFilter.Dispose();
            myWebAudioFilter = null;
#endif
            //UnityEngine.Debug.Log("OnDisable");
            this.StopCoroutineEx(coProviderA);
            this.StopCoroutineEx(coProviderB);
            jobListA.Clear();
            jobListB.Clear();
            foreach (var ss in synthesizers)
            {
                ss.Terminate();
            }
            synthesizers.Clear();
            mixer.TickCallback -= Tick;
            mixer.Terminate();
            mixer = null;
            foreach (var ss in acgState.synthesizers)
            {
                ss.Terminate();
            }
            acgState.mixer.TickCallback -= acgState.sequencer.Tick;
            acgState.synthesizers = null;
            acgState.mixer.Terminate();
            acgState.mixer = null;
            acgState.sequencer = null;
#if !DISABLE_TASK
            StopUpdateLoopTask();
#endif
            //UnityEngine.Debug.Log("OnDisable: done");
        }
#if !DISABLE_TASK
        private System.Threading.AutoResetEvent updateEvent;
        private Task updateLoopTask;
        private volatile bool exitUpdateLoopTask;
        private void StartUpdateLoopTask()
        {
            exitUpdateLoopTask = false;
            updateEvent = new System.Threading.AutoResetEvent(false);
            updateLoopTask = Task.Run((Action)UpdateLoopTaskAction);
        }
        private void StopUpdateLoopTask()
        {
            exitUpdateLoopTask = true;
            updateEvent.Set();
            updateLoopTask.Wait();
            updateEvent = null;
        }
        private void UpdateLoopTaskAction()
        {
            for (;;)
            {
                updateEvent.WaitOne();
                if (exitUpdateLoopTask)
                {
                    break;
                }
                var mix = mixer;
                if (mix != null)
                {
                    mix.Update();
                }
            }
        }
#endif
#if DISABLE_TASK || USE_WEBGL_AUDIO_STREAMING
        private void Update()
        {
            var m = mixer;
            if (m != null)
            {
#if DISABLE_TASK
                m.Update();
#else
                updateEvent.Set();
#endif
#if USE_WEBGL_AUDIO_STREAMING
                myWebAudioFilter.Update();
#endif
            }
        }
#endif
        private void OnAudioFilterRead(float[] data, int channels)
        {
            var m = mixer;
            if (m != null)
            {
                m.Output(data, channels, data.Length / channels);
#if !DISABLE_TASK
                var ue = updateEvent;
                if (ue != null)
                {
                    ue.Set();
                }
#endif
            }
#if UNITY_EDITOR
            LivingDead = false;
#endif
        }

        public void PrepareClip(MyMMLClip clip, Action<MyMMLClip> onFinished = null, bool dontGenerate = false)
        {
            if (!clip.Dirty && ((dontGenerate && clip.Ready) || clip.Valid))
            {
                return;
            }
            DebugLog("PrepareClip:" + clip.Name);
            var job = new ClipPreparingJob(this, clip, null, onFinished, dontGenerate);
            jobListA.AddLast(job.Prepare());
        }
        public void PrepareClip(List<AssetBundle> bundles, MyMMLClip clip, Action<MyMMLClip> onFinished = null, bool dontGenerate = false)
        {
            if (!clip.Dirty && ((dontGenerate && clip.Ready) || clip.Valid))
            {
                return;
            }
            DebugLog("PrepareClip:" + clip.Name);
            var job = new ClipPreparingJob(this, clip, bundles, onFinished, dontGenerate);
            jobListA.AddLast(job.Prepare());
        }
        private class ClipPreparingJob
        {
            private string mmlText = null;
            private MyMMLSequence mml = null;
            private ToneMap toneMap = null;

            private readonly MySyntheStation station = null;
            private readonly MyMMLClip clip = null;
            private readonly List<AssetBundle> bundles = null;
            private readonly Action<MyMMLClip> onFinished = null;
            private readonly bool dontGenerate = false;

            public ClipPreparingJob(MySyntheStation station, MyMMLClip clip, List<AssetBundle> bundles, Action<MyMMLClip> onFinished, bool dontGenerate)
            {
                this.station = station;
                this.clip = clip;
                this.bundles = bundles;
                this.onFinished = onFinished;
                this.dontGenerate = dontGenerate;
                mmlText = clip.Text;
                clip.Dirty = false;
                clip.Unit = null;
            }
            private string ConvertMid2Mml(byte[] mid, string name)
            {
                try
                {
                    var seq = MySMFSequence.Parse(new MemoryStream(mid, false));
                    return MySMF2MML.Convert(seq, System.Text.Encoding.ASCII, false, 0);
                }
                catch (Exception ec)
                {
                    DebugLog(ec.ToString());
                    return null;
                }
            }
            public IEnumerator Prepare()
            {
                if (string.IsNullOrEmpty(clip.Name))
                {
                    if ((clip.TextC != null) && !string.IsNullOrEmpty(clip.TextC.name))
                    {
                        clip.Name = clip.TextC.name;
                    }
                    else
                    if ((clip.TextA != null) && !string.IsNullOrEmpty(clip.TextA.name))
                    {
                        clip.Name = clip.TextA.name;
                    }
                }
                var name = string.IsNullOrEmpty(clip.Name) ? "NamelessClip" : clip.Name;
                DebugLog(name + ": start preparing clip");
                mml = null;
#if false
                byte[] midABytes = null;
                string midAName = null;
                if ((clip.TextA != null) && (clip.TextA.name.EndsWith(".mid")))
                {
                    midABytes = clip.TextA.bytes;
                    midAName = clip.TextA.name;
                }
#endif
                byte[] midCBytes = null;
                string midCName = null;
                if ((clip.TextC != null) && (clip.TextC.name.EndsWith(".mid")))
                {
                    midCBytes = clip.TextC.bytes;
                    midCName = clip.TextC.name;
                }
                {
#if !DISABLE_TASK
                    var task = Task.Run(() =>
                    {
#if false
                        if (midABytes != null)
                        {
                            var cm = ConvertMid2Mml(midABytes, midAName);
                            if (!string.IsNullOrEmpty(cm))
                            {
                                mmlText = cm + mmlText;
                            }
                        }
#endif
                        if (midCBytes != null)
                        {
                            var cm = ConvertMid2Mml(midCBytes, midCName);
                            if (!string.IsNullOrEmpty(cm))
                            {
                                mmlText = mmlText + cm;
                            }
                        }
                        mml = MyMMLSequence.Parse(mmlText);
                    });
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }
#else
                    mml = MyMMLSequence.Parse(mmlText);
                    yield return null;
#endif
                }
                DebugLog(name + ": parsing mml done.");
                //Debug.Assert(mml != null);
                if (mml.ErrorLine != 0)
                {
                    clip.Unit = new MyMMLSequenceUnit("Failed: parsing mml < " + mml.ErrorLine.ToString() + ":" + mml.ErrorPosition.ToString() + " > : " + mml.ErrorString + " <<<< " + mml.ErrorMessage);
                    DebugLog(clip.Unit.Error);
                    if (onFinished != null)
                    {
                        onFinished(clip);
                    }
                    yield break;
                }

                toneMap = new ToneMap();
                {
#if !DISABLE_TASK
                    var task = Task.Run(() =>
                    {
                        toneMap = SetupToneMap(toneMap, mml.ToneInfos);
                    });
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }
#else
                    toneMap = SetupToneMap(toneMap, mml.ToneInfos);
                    yield return null;
#endif
                }

                {
                    var ie = SetupSF2Tone(toneMap, bundles, mml);
                    while (ie.MoveNext())
                    {
#if false
                        if (ie.Current != null)
                        {
                            clip.Unit = new MyMMLSequenceUnit(ie.Current as string);
                            debugLog(clip.Unit.Error);
                            if (onFinished != null)
                            {
                                onFinished(clip);
                            }
                            yield break;
                        }
#endif
                        yield return null;
                    }
                }
                {
                    var ie = SetupSS8Tone(toneMap, bundles);
                    while (ie.MoveNext())
                    {
#if false
                        if (ie.Current != null)
                        {
                            clip.Unit = new MyMMLSequenceUnit(ie.Current as string);
                            debugLog(clip.Unit.Error);
                            if (onFinished != null)
                            {
                                onFinished(clip);
                            }
                            yield break;
                        }
#endif
                        yield return null;
                    }
                }

                DebugLog(name + ": tone setup finished.");
                clip.Unit = new MyMMLSequenceUnit(toneMap, mml);
                if (dontGenerate || !clip.GenerateAudioClip)
                {
                    DebugLog(name + ": clip prepared.");
                    if (onFinished != null)
                    {
                        onFinished(clip);
                    }
                    yield break;
                }
                station.jobListB.AddLast(Generate());
                yield break;
            }
            private static ToneMap SetupToneMap(ToneMap toneMap, List<MyMMLSequence.ToneInfo> toneInfos)
            {
                foreach (var toneInfo in toneInfos)
                {
                    var ts = MySynthesizer.CreateToneSet(toneInfo.Data);
                    if (ts == null)
                    {
                        continue;
                    }
                    ts.Name = toneInfo.Name;
                    ToneSet toneSet;
                    toneMap.TryGetValue(toneInfo.PresetNo, out toneSet);
                    if (toneSet == null)
                    {
                        toneSet = ts;
                    }
                    else
                    {
                        ts.AddRange(toneSet);
                        toneSet = ts;
                    }
                    if (toneSet.Count != 0)
                    {
                        toneMap[toneInfo.PresetNo] = toneSet;
                    }
                }
                return toneMap;
            }
            private static IEnumerator SetupSF2Tone(ToneMap toneMap, List<AssetBundle> bundles, MyMMLSequence seq)
            {
                string sf2prop;
                if (seq.Property.TryGetValue("SoundFont2", out sf2prop))
                {
                    var ss = sf2prop.Split('\n');
                    foreach (var sf2 in ss)
                    {
                        TextAsset ta = null;
                        var ie = LoadAssetAsync<TextAsset>(bundles, sf2);
                        while (ie.MoveNext())
                        {
                            if (ie.Current != null)
                            {
                                ta = ie.Current as TextAsset;
                            }
                            yield return null;
                        }
                        if (ta == null)
                        {
                            DebugLog("Failed: LoadAssetAsync(" + sf2 + "):");
                            continue;
                        }
                        var sf2Image = ta.bytes;
#if !DISABLE_TASK
                        var task = Task.Run(() =>
#endif
                        {
                            var sf2Bank = MySoundFont2.Parse(sf2Image);
                            if (sf2Bank == null)
                            {
                                DebugLog("Failed: MySoundFont2.Parse(): " + sf2);
#if !DISABLE_TASK
                                return;
#else
                                yield break;
#endif
                            }
                            var sf2ToneMap = MySynthesizerSF2.CreateToneMap(sf2Bank);
                            if (sf2ToneMap == null)
                            {
                                DebugLog("Failed: MySynthesizerSF2.CreateToneMap(): " + sf2);
#if !DISABLE_TASK
                                return;
#else
                                yield break;
#endif
                            }
                            toneMap.Merge(sf2ToneMap);
                        }
#if !DISABLE_TASK
                        );
                        while (!task.IsCompleted)
#endif
                        {
                            yield return null;
                        }
                        DebugLog("SetupToneMap(" + sf2 + "): loaded");
                    }
                }
            }
            private static IEnumerator SetupSS8Tone(ToneMap toneMap, List<AssetBundle> bundles)
            {
                var resource = new Dictionary<string, float[]>();
                foreach (var it in toneMap)
                {
                    var toneSet = it.Value;
                    for (int i = 0; i < toneSet.Count; i++)
                    {
                        var tone = toneSet[i];
                        if (tone is MySynthesizerSS8.ToneParam)
                        {
                            var ssTone = tone as MySynthesizerSS8.ToneParam;
                            float[] samples;
                            if (!resource.TryGetValue(ssTone.ResourceName, out samples))
                            {
                                AudioClip ac = null;
                                DebugLog(ssTone.Name + ": load resource " + ssTone.ResourceName);
                                var ie = LoadAssetAsync<AudioClip>(bundles, ssTone.ResourceName);
                                while (ie.MoveNext())
                                {
                                    if (ie.Current != null)
                                    {
                                        ac = ie.Current as AudioClip;
                                    }
                                    yield return null;
                                }
                                if (ac == null)
                                {
                                    yield return "Failed: LoadAssetAsync(" + ssTone.ResourceName + "): toneData[" + ssTone.Name + "]";
                                    yield break;
                                }
                                while (ac.loadState == AudioDataLoadState.Loading)
                                {
                                    yield return null;
                                }
                                samples = new float[ac.samples * ac.channels];
                                if (!ac.GetData(samples, 0))
                                {
                                    yield return "Failed: AudioClip.GetData(): toneData[" + ssTone.Name[i] + "]";
                                    yield break;
                                }
                                if (ac.channels != 1)
                                {
                                    var mix = new float[ac.samples];
                                    for (int k = 0; k < ac.samples; k++)
                                    {
                                        float s = 0.0f;
                                        for (int l = 0; l < ac.channels; l++)
                                        {
                                            s += samples[k * ac.channels + l];
                                        }
                                        mix[k] = s;
                                    }
                                    samples = mix;
                                }
                                resource.Add(ssTone.ResourceName, samples);
                            }
                            ssTone.SetSamples(samples);
                        }
                    }
                }
            }
            private IEnumerator Generate()
            {
                var name = string.IsNullOrEmpty(clip.Name) ? "NamelessClip" : clip.Name;
                DebugLog(name + ": start generating audioclip.");
                LinkedList<float[]> samples = null;
                {
#if !DISABLE_TASK
                    var task = Task.Run(() =>
                    {
                        var ie = GenerateAudioSamples();
                        while (ie.MoveNext())
                        {
                            if (ie.Current != null)
                            {
                                samples = ie.Current;
                                break;
                            }
                        }
                    });
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }
#else
                    var ie = GenerateAudioSamples();
#if true
                    var t0 = 0.0f;
                    while (ie.MoveNext())
                    {
                        if(ie.Current != null)
                        {
                            samples = ie.Current;
                            break;
                        }
                        t0 += Time.deltaTime;
                        if (t0 > 0.02f)
                        {
                            t0 = 0.0f;
                            yield return null;
                        }
                    }
#else
                    while (ie.MoveNext())
                    {
                        if(ie.Current != null)
                        {
                            samples = ie.Current;
                            break;
                        }
                        yield return null;
                    }
#endif
#endif
                }
                if (samples != null)
                {
                    int totalSamples = 0;
                    for (var i = samples.First; i != null; i = i.Next)
                    {
                        var block = i.Value;
                        totalSamples += block.Length / AudioClipGeneratorState.numChannels;
                    }
                    var ac = AudioClip.Create(clip.Name, totalSamples, AudioClipGeneratorState.numChannels, AudioClipGeneratorState.frequency, false);
                    int pos = 0;
#if UNITY_WEBGL
                    var data = new float[totalSamples * AudioClipGeneratorState.numChannels];
                    for (var i = samples.First; i != null; i = i.Next)
                    {
                        var block = i.Value;
                        for (int j = 0; j < block.Length; j++)
                        {
                            data[pos++] = block[j];
                        }
                    }
                    ac.SetData(data, 0);
#else
                    for (var i = samples.First; i != null; i = i.Next)
                    {
                        var block = i.Value;
                        ac.SetData(block, pos);
                        pos += block.Length / AudioClipGeneratorState.numChannels;
                    }
#endif
                    clip.AudioClip = ac;
                }
                DebugLog(name + ": audioclip generated.");
                if (onFinished != null)
                {
                    onFinished(clip);
                }
                yield break;
            }
            private IEnumerator<LinkedList<float[]>> GenerateAudioSamples()
            {
                var seq = station.acgState.sequencer;
                var mix = station.acgState.mixer;
                var sss = station.acgState.synthesizers;
                if ((seq == null) || (mix == null) || (sss == null))
                {
                    yield break;
                }
                mix.Reset();
                for (int j = 0; j < sss.Count; j++)
                {
                    seq.SetSynthesizer(j, sss[j], 0xffffffffU);
                }
                seq.KeyShift = clip.Key;
                seq.VolumeScale = clip.Volume;
                seq.TempoScale = clip.TempoScale;
                seq.Play(mml, toneMap, 0.0f, false);

                const int workSize = 4096;
                var work = new float[workSize * AudioClipGeneratorState.numChannels];
                var temp = new LinkedList<float[]>();
                bool zeroCross = false;
                for (;;)
                {
                    if (seq.Playing)
                    {
                        mix.Update();
                    }
                    Array.Clear(work, 0, work.Length);
                    int numSamples = mix.Output(work, AudioClipGeneratorState.numChannels, workSize);
                    if (numSamples == 0)
                    {
                        if (!zeroCross)
                        {
                            mix.Update();
                            continue;
                        }
                        break;
                    }
                    {
                        var block = new float[numSamples * AudioClipGeneratorState.numChannels];
                        Array.Copy(work, block, numSamples * AudioClipGeneratorState.numChannels);
                        temp.AddLast(block);
                        var v0 = work[(numSamples - 1) * AudioClipGeneratorState.numChannels + 0];
                        var v1 = work[(numSamples - 1) * AudioClipGeneratorState.numChannels + 1];
                        zeroCross = (v0 == 0.0f) && (v1 == 0.0f);
                    }
                    yield return null;
                }
                yield return temp;
            }
            private static IEnumerator LoadAssetAsync<T>(List<AssetBundle> bundles, string resourceName)
            {
                if (bundles != null)
                {
                    for (int i = 0; i < bundles.Count; i++)
                    {
                        var bundle = bundles[i];
                        if (bundle == null)
                        {
                            continue;
                        }
                        var request = bundle.LoadAssetAsync(resourceName, typeof(T));
                        if (request == null)
                        {
                            continue;
                        }
                        while (!request.isDone && (request.progress != 1))
                        {
                            yield return null;
                        }
                        yield return request.asset;
                        yield break;
                    }
                }
                {
                    var request = Resources.LoadAsync(resourceName, typeof(T));
                    if (request == null)
                    {
                        yield break;
                    }
                    while (!request.isDone && (request.progress != 1))
                    {
                        yield return null;
                    }
                    yield return request.asset;
                    yield break;
                }
            }
        }
    }
}
