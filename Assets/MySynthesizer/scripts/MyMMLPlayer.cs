
using UnityEngine;

using System;

using MySpace.Synthesizer;

namespace MySpace
{

    [AddComponentMenu("MySynthesizer/MyMMLPlayer")]
    public class MyMMLPlayer : MonoBehaviour
    {
        private MySyntheStation syntheStation;
        private MyMMLSequencer sequencer;
        private MyMMLClip clip;
        private enum PlayState
        {
            Stopped,
            Prepareing,
            Playing,
            Paused,
            Suspended,
        }
        private PlayState state;

        [SerializeField]
        private float fadeInTime = 0.0f;
        [SerializeField]
        private float playOutTime = 0.0f;
        [SerializeField]
        private float fadeOutTime = 0.0f;
        [SerializeField]
        private float volume = 1.0f;
        [SerializeField]
        private int keyShift = 0;
        [SerializeField]
        private float tempoScale = 1.0f;

        public float FadeInTime
        {
            get
            {
                return fadeInTime;
            }
            set
            {
                fadeInTime = value;
            }
        }
        public float PlayOutTime
        {
            get
            {
                return playOutTime;
            }
            set
            {
                playOutTime = value;
            }
        }
        public float FadeOutTime
        {
            get
            {
                return fadeOutTime;
            }
            set
            {
                fadeOutTime = value;
            }
        }
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
            }
        }
        /// <summary>+12 ~ -12</summary>
        public int KeyShift
        {
            get
            {
                return keyShift;
            }
            set
            {
                keyShift = value;
            }
        }
        /// <summary>
        /// tempo scale 0.0f ~ 2.0f
        /// </summary>
        public float TempoScale
        {
            get
            {
                return tempoScale;
            }
            set
            {
                tempoScale = value;
            }
        }
        public uint TimeBase
        {
            get
            {
                return sequencer.TimeBase;
            }
        }
        public bool Playing
        {
            get
            {
                if (sequencer == null)
                {
                    return false;
                }
                return (sequencer.Playing) || (state == PlayState.Prepareing);
            }
        }
        public MyMMLClip Clip
        {
            get
            {
                return clip;
            }
        }
        public event MyMMLSequencer.AppDataEventFunc AppDataEvent;
        public event MyMMLSequencer.PlayingEventFunc PlayingEvent;
        public event MyMMLSequencer.NextSectionEventFunc NextSectionEvent;
        public MyMMLClip Play(int port, int channel, params string[] mml)
        {
            return Play(port, channel, 1.0f, 1.0f, mml);
        }
        public MyMMLClip Play(int port, int channel, float tempoScale, float volume, params string[] mml)
        {
            if (sequencer == null)
            {
                return null;
            }
            sequencer.Stop(playOutTime);
            if ((port < 0) || (port >= MyMMLSequence.MaxNumPorts))
            {
                return null;
            }
            if ((channel < 0) || (channel >= MySynthesizer.NumChannels))
            {
                return null;
            }
            if ((mml.Length == 0) || (mml.Length >= MyMMLSequence.MaxNumTracks))
            {
                return null;
            }
            clip = new MyMMLClip();
            clip.Name = "Impromptu";
            clip.TempoScale = tempoScale;
            clip.Volume = volume;
            var sb = MyFountain<System.Text.StringBuilder>.Get(() => new System.Text.StringBuilder());
            sb.Length = 0;
            for (int i = 0; i < mml.Length; i++)
            {
                if (mml[i] == null)
                {
                    continue;
                }
                sb.Append("$t" + i.ToString("d2") + "=" + port.ToString() + ":" + channel.ToString() + "\n");
            }
            for (int i = 0; i < mml.Length; i++)
            {
                if (mml[i] == null)
                {
                    continue;
                }
                sb.Append("t" + i.ToString("d2") + "=" + mml[i] + "\n");
            }
            clip.TextB = sb.ToString();
            sb.Length = 0;
            MyFountain<System.Text.StringBuilder>.Put(sb);
            state = PlayState.Prepareing;
            return clip;
        }
        public void Play(MyMMLClip clip)
        {
            if (sequencer == null)
            {
                return;
            }
            sequencer.Stop(playOutTime);
            this.clip = clip;
            state = PlayState.Prepareing;
        }
        public void Stop()
        {
            if (state == PlayState.Stopped)
            {
                return;
            }
            sequencer.Stop(fadeOutTime);
            clip = null;
            state = PlayState.Stopped;
        }
        public void Pause()
        {
            if (state == PlayState.Stopped)
            {
                return;
            }
            if (state == PlayState.Playing)
            {
                sequencer.Stop(fadeOutTime);
                state = PlayState.Paused;
            }
            else if (state == PlayState.Prepareing)
            {
                state = PlayState.Suspended;
            }
        }
        public void Continue()
        {
            if (state == PlayState.Stopped)
            {
                return;
            }
            if (state == PlayState.Paused)
            {
                sequencer.Continue(fadeInTime);
                state = PlayState.Playing;
            }
            else if (state == PlayState.Suspended)
            {
                state = PlayState.Prepareing;
            }
        }
        private void appDataEvent(MyMMLSequencer.EventLocation loc, string data)
        {
            if (AppDataEvent != null)
            {
                AppDataEvent(loc, data);
            }
        }
        private void playingEvent(MyMMLSequencer.EventLocation loc, UInt32 step, UInt32 gate, MyMMLSequence.Instruction inst)
        {
            if (PlayingEvent != null)
            {
                PlayingEvent(loc, step, gate, inst);
            }
        }
        private int nextSectionEvent(MyMMLSequencer.EventLocation loc, int nextSection)
        {
            if (NextSectionEvent != null)
            {
                return NextSectionEvent(loc, nextSection);
            }
            return nextSection;
        }
        private void OnEnable()
        {
            syntheStation = GameObject.FindObjectOfType<MySyntheStation>();
            if (syntheStation == null)
            {
                return;
            }
            sequencer = new MyMMLSequencer(syntheStation.TickFrequency);
            syntheStation.AddSequencer(sequencer);
            sequencer.AppDataEvent += appDataEvent;
            sequencer.PlayingEvent += playingEvent;
            sequencer.NextSectionEvent += nextSectionEvent;
            state = PlayState.Stopped;
            //Debug.Log("start:" + Application.isPlaying);
        }
        private void OnDisable()
        {
            //Debug.Log("OnDisable:" + Application.isPlaying);
            if (sequencer == null)
            {
                return;
            }
            if (sequencer.Playing)
            {
                sequencer.Stop(0.0f);
            }
            if (syntheStation != null)
            {
                syntheStation.RemoveSequencer(sequencer);
            }
            sequencer.AppDataEvent -= appDataEvent;
            sequencer.PlayingEvent -= playingEvent;
            sequencer.NextSectionEvent -= nextSectionEvent;
            clip = null;
            sequencer = null;
            state = PlayState.Stopped;
            syntheStation = null;
        }
        private void Update()
        {
            if (state == PlayState.Stopped)
            {
                return;
            }
            if (sequencer.Playing)
            {
                sequencer.KeyShift = clip.Key + keyShift;
                sequencer.VolumeScale = clip.Volume * volume;
                sequencer.TempoScale = clip.TempoScale * tempoScale;
            }
            if (state == PlayState.Playing)
            {
                if (!sequencer.Playing)
                {
                    Stop();
                }
                return;
            }
            if (state != PlayState.Prepareing)
            {
                return;
            }
            if (sequencer.Playing)
            {
                return;
            }
            if (syntheStation == null)
            {
                return;
            }
            if (clip.Dirty)
            {
                syntheStation.PrepareClip(clip);
            }
            if (!clip.Prepared)
            {
                return;
            }
            if (!clip.Ready)
            {
                Stop();
                return;
            }
            for (int i = 0; i < clip.Ports.Length; i++)
            {
                var s = (int)clip.Ports[i];
                if ((s >= 0) && (s <= syntheStation.Synthesizers.Count))
                {
                    sequencer.SetSynthesizer(i, syntheStation.Synthesizers[s], clip.VoiceMask);
                }
            }
            sequencer.KeyShift = clip.Key + keyShift;
            sequencer.VolumeScale = clip.Volume * volume;
            sequencer.TempoScale = clip.TempoScale * tempoScale;
            sequencer.Play(clip.Unit.Sequence, clip.Unit.ToneMap, fadeInTime, clip.Loop);
            state = PlayState.Playing;
        }
    }
}
