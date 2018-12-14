using System;
using System.Runtime.InteropServices;

#if UNITY_WEBGL

namespace MySpace
{
    public class MyWebAudioStreamer : IDisposable
    {
        private static float[] work = null;
        private static Action<float[], int> onAudioStreamRead = null;
        private static int bufferLength = 0;
        private static int bufferCount = 0;
        private static int sampleRate = 0;
        private Action<float[], int> action = null;

        public MyWebAudioStreamer(int bufferLength, int bufferCount, int sampleRate, Action<float[], int> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException();
            }
            if (work != null)
            {
                if ((MyWebAudioStreamer.bufferCount != bufferCount) || (MyWebAudioStreamer.bufferLength != bufferLength) || (MyWebAudioStreamer.sampleRate != sampleRate))
                {
                    throw new ArgumentException();
                }
            }
            this.action = action;
            onAudioStreamRead += action;
            if (work == null)
            {
                MyWebAudioStreamer.bufferCount = bufferCount;
                MyWebAudioStreamer.bufferLength = bufferLength;
                MyWebAudioStreamer.sampleRate = sampleRate;
                work = new float[bufferLength * 2];
#if !UNITY_EDITOR
                MyWebAudioStreamerStart(bufferCount, bufferLength, sampleRate);
#endif
            }
        }
        ~MyWebAudioStreamer()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (action != null)
            {
                onAudioStreamRead -= action;
                action = null;
            }
            if (onAudioStreamRead == null)
            {
#if !UNITY_EDITOR
                MyWebAudioStreamerStop();
#endif
                work = null;
            }
        }
        public void Update()
        {
#if !UNITY_EDITOR
            float[] buf = null;
            while (MyWebAudioStreamerUpdate(buf))
            {
                Array.Clear(work, 0, work.Length);
                onAudioStreamRead.Invoke(work, 2);
                buf = work;
            }
#endif
        }

#if !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void MyWebAudioStreamerStart(int bufferCount, int bufferLength, float sampleRate);

        [DllImport("__Internal")]
        private static extern bool MyWebAudioStreamerUpdate(float[] data);

        [DllImport("__Internal")]
        private static extern void MyWebAudioStreamerStop();
#endif
    }
}
#endif

