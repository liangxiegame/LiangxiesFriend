using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// every variable we want to save has to be serialized in the editor
[CreateAssetMenu(fileName ="NewImageData",menuName ="Create ImageData",order =0)]
public class PIAImageData : ScriptableObject {
    #region Fields
  
    [SerializeField]
    private List<PIALayer> layers;
    [SerializeField]
    private List<PIAFrame> frames;

    [SerializeField]
    private int _width;
    [SerializeField]
    private int _height;

    #endregion

    #region Properties
    
    public int CurrentFrameIndex { get; set; }
    public int CurrentLayer { get; set; }
    public PIAFrame CurrentFrame { get { return frames[CurrentFrameIndex]; } }

    public List<PIAFrame> Frames { get { return frames; } set { frames = value; } }
    public List<PIALayer> Layers { get { return layers; } set { layers = value; } }

    public int Width { get { return _width; }  set { _width = value; } }
    public int Height { get { return _height; } set { _height = value; } }

    #endregion

    #region Methods

    public void Init(int width, int height)
    {
        Width = width;
        Height = height;
        frames = new List<PIAFrame>();
        layers = new List<PIALayer>();
        AddLayer();
    }
    public void AddLayer()
    {
        PIALayer layer = new PIALayer();
        layer.Index = layers.Count;
        layer.Name = "Layer" + layer.Index;
        layers.Add(layer);

        CurrentLayer = layers.Count - 1;

        //every time we add a new layer we need to make sure every frame gets a new sub-texture (see PIAFrame class)
        if (frames.Count == 0)
            AddFrame();
        else
            foreach (var item in frames)
            {
                item.AddTexture();
            }
            
    }
    public void RemoveLayer(int index)
    {
        if (layers.Contains(layers[index]))
        {
            layers.Remove(layers[index]);
            CurrentLayer = Mathf.Max(0,index - 1);

            //every time we remove a layer we need to make sure every frame gets its sub-texture deleted (see PIAFrame class)
            foreach (var item in frames)
            {
                item.RemoveTexture(index);
            }
        }
    }
    public PIAFrame AddFrame() {
        PIAFrame frame = new PIAFrame();
        frame.Init(this);
        frames.Add(frame);
        CurrentFrameIndex = frames.Count - 1;
        return frame;
    }
    public void RemoveFrame(int index) {
        if (frames.Contains(frames[index]))
        {

            frames.Remove(frames[index]);
            CurrentFrameIndex = Mathf.Max(0, index-1);
            
        }
    }
    public void MoveFrameUp(int currentIndex) {
        PIAFrame tmp = frames[currentIndex-1];
        frames[currentIndex - 1] = frames[currentIndex];
        frames[currentIndex] = tmp;
    }
    public void MoveFrameDown(int currentIndex) {
        PIAFrame tmp = frames[currentIndex + 1];
        frames[currentIndex + 1] = frames[currentIndex];
        frames[currentIndex] = tmp;
    }

    public void Save() {
        foreach (var frame in frames)
        {
            foreach (var texture in frame.Textures)
            {
                texture.Save();
            }
        }
    }
    #endregion
    
}

// this could have been a class too
[System.Serializable]
public struct PIALayer {
    [SerializeField]
    private int _index;
    [SerializeField]
    private string _name;
    [SerializeField]
    private bool _hidden;

    public bool Hidden { get { return _hidden; } set { _hidden = value; } }
    public int Index { get { return _index; } set { _index = value; } }
    public string Name { get { return _name; } set { _name = value; } }
}







