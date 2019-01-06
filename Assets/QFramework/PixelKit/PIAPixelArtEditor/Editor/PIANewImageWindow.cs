using UnityEditor;
using UnityEngine;

public class PIANewImageWindow : EditorWindow
{

    static PIANewImageWindow window;
    int width = 16;
    int height = 16;

    public static void ShowWindow()
    {
        // INIT WINDOW
        window = GetWindow<PIANewImageWindow>();
        window.maxSize = new Vector2(200, 200);
        window.Show();

    }

    private void OnGUI()
    {
        // we use always the same aspect ratio
        width = EditorGUILayout.IntField("Width: ", Mathf.Max(1,width));
        height = EditorGUILayout.IntField("Height: ", Mathf.Max(1, width));

        if (GUILayout.Button("Create"))
        {
            LoadNewAsset();
        }
    }
    private void LoadNewAsset()
    {
        PIASession.Instance.LoadNewAsset(width, height);
        
        // when loading a new asset we want to reset the selection texture
        PIAEditorWindow.Instance.SelectionTexture = new PIATexture();
        PIAEditorWindow.Instance.SelectionTexture.Init(PIASession.Instance.ImageData.Width, PIASession.Instance.ImageData.Height, 0);
        PIAEditorWindow.window.Repaint();
        window.Close();
    }
}

