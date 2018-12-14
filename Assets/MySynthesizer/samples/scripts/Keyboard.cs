
using UnityEngine;
using UnityEngine.UI;
using MySpace;

namespace MySpace.Sample
{
    public class Keyboard : MonoBehaviour
    {
        private MySyntheStation syntheStation = null;

        [SerializeField]
        private KeyProperty blackKey = null;
        [SerializeField]
        private KeyProperty whiteKey = null;
        [SerializeField]
        private int baseNote = 60;

        public int BaseNote
        {
            get
            {
                return baseNote;
            }
            set
            {
                baseNote = value;
            }
        }
        public int PortNo
        {
            get;
            set;
        }
        public int ChNo
        {
            get;
            set;
        }

        private int numKeys;
        private int vel = 100;
        private int vol = 100;
        private Text position;
        private int[] keyState;
        private void OnKeyDown(int index)
        {
            //UnityEngine.Debug.Log("key dw:" + (baseNote + index));
            var nn = BaseNote + index;
            keyState[index] = nn;
            syntheStation.Synthesizers[PortNo].Channel[ChNo].NoteOn((byte)nn, (byte)vel);
        }
        private void OnKeyUp(int index)
        {
            //UnityEngine.Debug.Log("key up:" + (baseNote + index));
            var nn = keyState[index];
            if (nn < 0)
            {
                return;
            }
            syntheStation.Synthesizers[PortNo].Channel[ChNo].NoteOff((byte)nn);
        }
        private void LShift()
        {
            if (BaseNote - 12 >= 0)
            {
                BaseNote -= 12;
                position.text = "^C" + (BaseNote / 12 - 2 + 1);
            }
        }
        private void RShift()
        {
            if (BaseNote + numKeys + 12 <= 128)
            {
                BaseNote += 12;
                position.text = "^C" + (BaseNote / 12 - 2 + 1);
            }
        }
        private void Awake()
        {
            syntheStation = GameObject.FindObjectOfType<MySyntheStation>();

            var p = GetComponent<RectTransform>();
            var r = whiteKey.GetComponent<RectTransform>();
            var width = r.rect.width;
            var basePos = whiteKey.transform.localPosition;
            int[] ofs = { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12 };
            numKeys = (int)((p.rect.width - (p.rect.width / 2 + (basePos.x - width / 2)) * 2) / width) * 12 / 7;
            keyState = new int[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                keyState[i] = -1;
                int o = ofs[i % 12];
                if ((o & 1) != 0)
                {
                    continue;
                }
                int index = i;
                var key = Instantiate(whiteKey);
                var pos = basePos;
                var scl = whiteKey.transform.localScale;
                var rot = whiteKey.transform.rotation;
                pos.x += ((i / 12) * 14 + o) * (width / 2);
                key.transform.SetParent(transform);
                key.transform.localPosition = pos;
                key.transform.localScale = scl;
                key.transform.rotation = rot;
                key.OnKeyDownEvent.AddListener(() => OnKeyDown(index));
                key.OnKeyUpEvent.AddListener(() => OnKeyUp(index));
            }
            for (int i = 0; i < numKeys; i++)
            {
                int o = ofs[i % 12];
                if ((o & 1) == 0)
                {
                    continue;
                }
                int index = i;
                var key = Instantiate(blackKey);
                var pos = basePos;
                var scl = blackKey.transform.localScale;
                var rot = blackKey.transform.rotation;
                pos.x += ((i / 12) * 14 + o) * (width / 2);
                key.transform.SetParent(transform);
                key.transform.localPosition = pos;
                key.transform.localScale = scl;
                key.transform.rotation = rot;
                key.OnKeyDownEvent.AddListener(() => OnKeyDown(index));
                key.OnKeyUpEvent.AddListener(() => OnKeyUp(index));
            }
            whiteKey.gameObject.SetActive(false);
            blackKey.gameObject.SetActive(false);

            position = transform.Find("Position").GetComponent<Text>();
            BaseNote -= BaseNote % 12;
            position.text = "^C" + (BaseNote / 12 - 2 + 1);
            var lshift = transform.Find("LShift").GetComponent<Button>();
            lshift.onClick.AddListener(LShift);
            var rshift = transform.Find("RShift").GetComponent<Button>();
            rshift.onClick.AddListener(RShift);
            var velocity = transform.Find("Velocity").GetComponent<Slider>();
            velocity.onValueChanged.AddListener((value) =>
            {
                vel = (int)value;
                velocity.gameObject.transform.Find("Value").GetComponent<Text>().text = "" + vel;
            });

            var volume = transform.Find("Volume").GetComponent<Slider>();
            volume.onValueChanged.AddListener((value) =>
            {
                vol = (int)value;
                syntheStation.Synthesizers[PortNo].MasterVolume((byte)value);
                volume.gameObject.transform.Find("Value").GetComponent<Text>().text = "" + (byte)value;
            });

            var hold = transform.Find("Hold").GetComponent<KeyProperty>();
            hold.OnKeyDownEvent.AddListener(() => syntheStation.Synthesizers[PortNo].Channel[ChNo].Damper(+127));
            hold.OnKeyUpEvent.AddListener(() => syntheStation.Synthesizers[PortNo].Channel[ChNo].Damper(0));

            var damp = transform.Find("Damp").GetComponent<KeyProperty>();
            damp.OnKeyDownEvent.AddListener(() => syntheStation.Synthesizers[PortNo].Channel[ChNo].Damper(-127 + 256));
            damp.OnKeyUpEvent.AddListener(() => syntheStation.Synthesizers[PortNo].Channel[ChNo].Damper(0));
        }
        private void Start()
        {
            var velocity = transform.Find("Velocity").GetComponent<Slider>();
            velocity.value = vel;
            var volume = transform.Find("Volume").GetComponent<Slider>();
            volume.value = vol;
        }
        private readonly KeyCode[] keys = new KeyCode[] { KeyCode.Z, KeyCode.S, KeyCode.X, KeyCode.D, KeyCode.C, KeyCode.V, KeyCode.G, KeyCode.B, KeyCode.H, KeyCode.N, KeyCode.J, KeyCode.M };
        private void Update()
        {
            if (!GetComponentInParent<CanvasGroup>().interactable)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                syntheStation.Synthesizers[PortNo].Channel[ChNo].AllSoundOff();
            }
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                LShift();
            }
            if (Input.GetKeyDown(KeyCode.Period))
            {
                RShift();
            }
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (Input.GetKeyDown(key))
                {
                    OnKeyDown(i + 12);
                }
                if (Input.GetKeyUp(key))
                {
                    OnKeyUp(i + 12);
                }
            }
        }
    }
}
