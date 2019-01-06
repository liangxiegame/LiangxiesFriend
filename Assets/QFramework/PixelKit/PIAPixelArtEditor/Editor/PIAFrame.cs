using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PIAFrame
{
    #region Fields
    //every layer is rappresented through sub-textures in each frame
    [SerializeField]
    private List<PIATexture> textures;

    // this is what you see in the left side preview
    private Texture2D frameTexture;

    // this is what you see when you paint on the grid
    private Texture2D frameTextureWithFilters;

    #endregion

    #region Properties

    public List<PIATexture> Textures { get { return textures; } }

    #endregion

    #region Methods

    public void Init(PIAImageData _imageData)
    {

        textures = new List<PIATexture>();
        frameTexture = PIATexture.CreateBlank(_imageData.Width, _imageData.Height);
        frameTextureWithFilters = PIATexture.CreateBlank(_imageData.Width, _imageData.Height);

        // we make sure every time a new frame is created it contains the same amount of sub-textures 
        // as the layers in the image
        for (int i = 0; i < _imageData.Layers.Count; i++)
        {
            AddTexture(_imageData, i);
        }

    }

    public PIATexture AddTexture(PIAImageData imageData, int layerIndex)
    {
        PIATexture texture = new PIATexture();
        texture.Init(imageData.Width, imageData.Height, layerIndex);
        textures.Add(texture);
        return texture;
    }
    public PIATexture AddTexture()
    {
        PIAImageData imageData = PIASession.Instance.ImageData;
        return AddTexture(imageData, imageData.CurrentLayer);
    }
    public void CopyFrom(PIAFrame source)
    {
        textures.Clear();

        foreach (var item in source.textures)
        {
            byte[] itemData = item.Texture.EncodeToPNG();
            PIATexture piaTexture = AddTexture();
            piaTexture.LayerIndex = item.LayerIndex;
            Texture2D texture = piaTexture.Texture;
            texture.LoadImage(itemData);
            texture.Apply();
        }
    }
    public void RemoveTexture(int index)
    {
        if (textures.Contains(textures[index]))
            textures.Remove(textures[index]);
    }

    public PIATexture GetCurrentImage()
    {
        return textures[PIASession.Instance.ImageData.CurrentLayer];
    }
    public Texture2D GetFrameTextureWithLayerFilters()
    {
        // in case we load from a saved asset
        if (frameTextureWithFilters == null)
            frameTextureWithFilters = PIATexture.CreateBlank(PIASession.Instance.ImageData.Width, PIASession.Instance.ImageData.Height);

        // clean up
        PIATexture.Wipe(frameTextureWithFilters);
        PIAImageData imageData = PIASession.Instance.ImageData;
        frameTextureWithFilters.filterMode = FilterMode.Point;

        // caching hidden layers indexes
        List<int> hiddenLayersIndex = new List<int>();
        foreach (var item in imageData.Layers)
        {
            if (item.Hidden == true)
                hiddenLayersIndex.Add(item.Index);
        }

        // ordering up textures so the last layer is always drawn on top of the first layer
        textures = textures.OrderBy(x => x.LayerIndex).ToList();

        int currentLayerTextureIndex = 0;

        // building the filtered texture from each sub-texture (layer) in the frame
        for (int i = 0; i < textures.Count; i++)
        {
            var item = textures[i];
            // skipping hidden layers
            if (hiddenLayersIndex.Contains(item.LayerIndex))
            {
                continue;
            }

            // if the sub-texture index is NOT the current selected index it gets painted transparent (alpha/2)
            if (imageData.CurrentLayer != item.LayerIndex)
            {
                for (int x = 0; x < PIASession.Instance.ImageData.Width; x++)
                    for (int y = 0; y < PIASession.Instance.ImageData.Height; y++)
                    {
                        Color pixelColor = item.Texture.GetPixel(x, y);
                        pixelColor.a = pixelColor.a / 2;
                        if (pixelColor.a > 0)
                        {
                            frameTextureWithFilters.SetPixel(x, y, pixelColor);
                        }
                    }
            }
            else
                currentLayerTextureIndex = i;

        }

        // drawing current layer texture
        for (int x = 0; x < PIASession.Instance.ImageData.Width; x++)
            for (int y = 0; y < PIASession.Instance.ImageData.Height; y++)
            {
                Color pixelColor = textures[currentLayerTextureIndex].Texture.GetPixel(x, y);
                if (pixelColor.a > 0)
                {
                    frameTextureWithFilters.SetPixel(x, y, pixelColor);
                }
            }

        frameTextureWithFilters.Apply();

        

        return frameTextureWithFilters;
    }
    public Texture2D GetFrameTexture()
    {
        if (frameTexture == null)
            frameTexture = PIATexture.CreateBlank(PIASession.Instance.ImageData.Width, PIASession.Instance.ImageData.Height);
        PIATexture.Wipe(frameTexture);
        frameTexture.filterMode = FilterMode.Point;

        // ordering up textures so the last layer is always drawn on top of the first layer
        textures = textures.OrderBy(x => x.LayerIndex).ToList();

        //building up the texture without any filters
        foreach (var item in textures)
        {
            for (int x = 0; x < PIASession.Instance.ImageData.Width; x++)
            {
                for (int y = 0; y < PIASession.Instance.ImageData.Height; y++)
                {
                    Color pixelColor = item.Texture.GetPixel(x, y);
                    if (pixelColor.a > 0)
                    {
                        frameTexture.SetPixel(x, y, pixelColor);
                    }
                }
            }

        }
        frameTexture.Apply();

        return frameTexture;

    }

    #endregion




}