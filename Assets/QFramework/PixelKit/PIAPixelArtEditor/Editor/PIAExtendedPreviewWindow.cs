using UnityEditor;
using Piacenti.EditorTools;
using UnityEngine;

public class PIAExtendedPreviewWindow : EditorWindow
{
    static PIAExtendedPreviewWindow window;

    WindowSection body;
    Vector2 bodyWorldOffset = new Vector2(220, 0);

    public static void ShowWindow()
    {
        // INIT WINDOW 
        window = GetWindow<PIAExtendedPreviewWindow>();
        Vector2 windowSize = new Vector2(995, 800);
        window.position = new Rect(Screen.width / 2 - windowSize.x / 2, 100, windowSize.x, windowSize.y);
        window.Show();

    }
    public static void CloseWindow()
    {
        // just to make sure it was opened
        if(window!=null)
            window.Close();

    }
    private void OnEnable()
    {
        // INIT LAYOUTS
        body = new WindowSection(new Rect(bodyWorldOffset.x, bodyWorldOffset.y, position.width - bodyWorldOffset.x * 2,
            position.height - bodyWorldOffset.y * 2), new Color(0.6275f, 0.6275f, 0.6275f, 1.0000f));
    }
    private void OnGUI()
    {
        bodyWorldOffset = new Vector2(position.width / 6, 0);

        body.SetRect(new Rect(bodyWorldOffset.x, bodyWorldOffset.y, position.width - bodyWorldOffset.x * 2,
            position.height - bodyWorldOffset.y * 2));

        // drawing the background body
        GUI.DrawTexture(body.GetRect(), body.GetTexture());

        DrawBody();
        window.Repaint();
    }

    private void DrawBody()
    {
        float scale = body.GetRect().width;
        Rect grid = new Rect((body.GetRect().width / 2 - scale / 2), (body.GetRect().center.y - scale / 2), scale, scale);

        // drawing the actual frame from animator
        GUILayout.BeginArea(body.GetRect());
        {
            EditorGUI.DrawTextureTransparent(grid, PIAAnimator.Instance.GetFrameOrFirst().GetFrameTexture());

        }
        GUILayout.EndArea();


    }
}