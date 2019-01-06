using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PIASession {
    #region Static

    private static PIASession _instance;

    public static PIASession Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PIASession();
            }
            return _instance;
        }
    }

    #endregion

    #region Fields

    private PIAImageData _imageData;

    #endregion

    #region Properties

    // ask to save the document on exit
    public bool IsDirty { get; set; }
    public PIAImageData ImageData { get { return _imageData; } set { _imageData = value; } }

    // does it exist as an asset?
    private bool isNew
    {
        get
        {
            string path = AssetDatabase.GetAssetPath(ImageData);
            path = Path.GetFileNameWithoutExtension(path);
            return string.IsNullOrEmpty(path) ? true : false;
        }
    }

    // project name = file name (bottom left label)
    public string ProjectName
    {
        get
        {
            string path = AssetDatabase.GetAssetPath(ImageData);
            path = Path.GetFileNameWithoutExtension(path);
            return string.IsNullOrEmpty(path) ? "NewImage" : path;
        }
    }

    #endregion

    #region Methods

    public void LoadAsset()
    {
        OpenAsset(ref _imageData);
        if (ImageData == null)
            return;
        // it's getting pulled from default execution
        if (ImageData.CurrentFrame == null)
            ImageData.Init(16,16);

        // when loading a new asset we want to reset the selection texture
        PIAEditorWindow.Instance.SelectionTexture = new PIATexture();
        PIAEditorWindow.Instance.SelectionTexture.Init(ImageData.Width, ImageData.Height, 0);

    }
    public PIASession()
    {
        LoadNewAsset(16,16);
    }
    public Texture2D LoadImageFromFile()
    {
        // only PNG and JPG are supported
        string path = EditorUtility.OpenFilePanelWithFilters("Import Image", "", new string[] { "PNG", "png", "JPG", "jpg" });

        if (string.IsNullOrEmpty(path))
            return null;

        // the selected texture from file panel is hard pushed on the current frame texture
        Texture2D texture = new Texture2D(2, 2);
        texture.filterMode = FilterMode.Point;
        byte[] fileData = File.ReadAllBytes(path);
        texture.LoadImage(fileData);
        ImageData.CurrentFrame.GetCurrentImage().Texture = texture;
        return ImageData.CurrentFrame.GetCurrentImage().Texture;
    }

    private void OpenAsset(ref PIAImageData asset)
    {
        string path = EditorUtility.OpenFilePanelWithFilters("Select Asset", "Assets/", new string[] { "ASSET", "asset" });
        if (string.IsNullOrEmpty(path))
            return;

        string internalPath = FileUtil.GetProjectRelativePath(path);
        if (string.IsNullOrEmpty(internalPath)) {
            EditorUtility.DisplayDialog("No Path Found", "Be sure to place your asset inside 'Asset/'.", "Ok");
            return;
        }

        asset = AssetDatabase.LoadAssetAtPath<PIAImageData>(internalPath);

    }

    public void ExportImage(Texture2D tex, string path)
    {
        // --- JPG AND PNG ONLY SUPPORTED ----

        if (string.IsNullOrEmpty(path))
            return;
        byte[] encodedBytes;
        string extension = Path.GetExtension(path);
        switch (extension)
        {
            case ".png":
                encodedBytes = tex.EncodeToPNG();
                break;
            case ".jpg":
                encodedBytes = tex.EncodeToJPG();
                break;
            default:
                encodedBytes = tex.EncodeToPNG();
                break;
        }
        File.WriteAllBytes(path, encodedBytes);
        AssetDatabase.Refresh();

        // MODIFYING TEXTURE IMPORT SETTINGS
        TextureImporter importer = TextureImporter.GetAtPath(FileUtil.GetProjectRelativePath(path)) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.filterMode = FilterMode.Point;
        importer.maxTextureSize = GetTextureImporterMaxSize(tex);
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        AssetDatabase.Refresh();

    }
    public string ExportImage(Texture2D tex)
    {
        string path = EditorUtility.SaveFilePanel("Export Image", "", ProjectName, "png");
        ExportImage(tex, path);
        return path;

    }
    public void ExportAll() {

        string pathParent = EditorUtility.SaveFolderPanel("Choose Folder", FileUtil.GetProjectRelativePath("Assets"), ProjectName);

        // exporting every frame on a selected folder
        for (int i = 0; i < ImageData.Frames.Count; i++)
        {
            string path = pathParent + "/" + ProjectName + i + ".png";
            var item = ImageData.Frames[i];
            ExportImage(item.GetFrameTexture(), path);
        }

    }
    public void ExportSpriteSheet() {

        Texture2D spriteSheet = PIASpriteSheet.GenerateSpriteSheet(ImageData.Frames.Count, ImageData.Width, ImageData.Height, ImageData.Frames.ToArray());
        string path = ExportImage(spriteSheet);
        PIASpriteSheet.Slice(spriteSheet, path, ImageData.Width, ImageData.Height);

        // we need to hard select the asset in order to the built in SpriteEditorWindow to work
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(FileUtil.GetProjectRelativePath(path));
        // little bit of reflection to open built in Sprite Editor Window 
        EditorWindow spriteEditorWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetTypes().Where(x => x.Name == "SpriteEditorWindow").FirstOrDefault());
        
    }
    public int GetTextureImporterMaxSize(Texture2D tex)
    {
        int value = tex.width > tex.height ? tex.width : tex.height;
        int[] values = new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        for (int i = 0; i < values.Length; i++)
        {
            if (value <= values[i])
            {
                value = values[i];
                return value;
            }
        }
        return 2048;
    }
    public void SaveAsset()
    {
        string path;
        if (isNew)
        {
            path = EditorUtility.SaveFilePanel("Save Asset", "Assets/", ProjectName, "asset");
            path = FileUtil.GetProjectRelativePath(path);
            if (string.IsNullOrEmpty(path))
                return;
            AssetDatabase.CreateAsset(ImageData, path);
        }
        ImageData.Save();
        AssetDatabase.Refresh();
        // this ask the user to save project on unity exit
        EditorUtility.SetDirty(_imageData);

    }
    public void LoadNewAsset(int width, int height)
    {
        ImageData = ScriptableObject.CreateInstance<PIAImageData>();
        ImageData.Init(width,height);
    }

    #endregion



    private static class PIASpriteSheet {
        public static Texture2D GenerateSpriteSheet(int framesCount, int frameWidth, int frameHeight, PIAFrame[] frames) {
            Texture2D spriteSheet;
            int rows = (int)Mathf.Sqrt(framesCount);

            int spriteSheetWidth = (framesCount * frameWidth) / rows ;
            spriteSheetWidth += spriteSheetWidth % frameWidth;

            int spriteSheetHeight = frameHeight * rows;
            spriteSheetHeight += spriteSheetHeight % frameHeight;

            spriteSheet = PIATexture.CreateBlank(spriteSheetWidth, spriteSheetHeight);
            int offsetX = 0;
            int offsetY = spriteSheetHeight-frameHeight;


            for (int i = 0; i < framesCount; i++)
            {
                if (i != 0 && (frameWidth * i) % spriteSheetWidth == 0)
                {
                    offsetY -= frameHeight;
                    offsetX = 0;
                }

                for (int x = 0; x < frameWidth; x++)
                {
                    for (int y = 0; y < frameHeight; y++)
                    {
                        Color framePixelColor = frames[i].GetFrameTexture().GetPixel(x, y);
                        spriteSheet.SetPixel(x + offsetX, y + offsetY, framePixelColor);
                        spriteSheet.Apply();
                    }

                }
                offsetX += frameWidth;
                
            }


            return spriteSheet;
        }

        public static void Slice(Texture2D tex,string path, int sliceWidth,int sliceHeight) {
            // "slices" in the built in Sprite Editor Window are just SpriteMetaData s saved on an array
            // we can set this array through TextureImporter.spritesheet

            TextureImporter importer = TextureImporter.GetAtPath(FileUtil.GetProjectRelativePath(path)) as TextureImporter;
            importer.isReadable = true;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            List<SpriteMetaData> spritesheetMetaData = new List<SpriteMetaData>();
            for (int x = 0; x < tex.width; x+=sliceWidth)
            {
                for (int y = tex.height; y >0; y-=sliceHeight)
                {
                    SpriteMetaData data = new SpriteMetaData();
                    data.pivot = new Vector2(0.5f, 0.5f);
                    data.alignment = 9;
                    data.name = tex.name + x + "_" + y;
                    data.rect = new Rect(x, y - sliceHeight, sliceWidth, sliceHeight);
                    spritesheetMetaData.Add(data);

                }
            }

            importer.spritesheet = spritesheetMetaData.ToArray();

          

            AssetDatabase.Refresh();
            importer.SaveAndReimport();
        }
    }


}
