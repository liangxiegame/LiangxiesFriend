using UnityEngine;
public enum PIAToolType {
    Paint,
    Erase,
    Rectangle,
    RectangleFilled,
    Selection,
    Dithering
}

public class PIADrawer{

    #region Static

    public static Color ClearColor { get { return new Color(255, 255, 255, 0); } }

    private static void TransformToLeftTop(ref Vector2 point, int height) {
        point = new Vector2(point.x, height - point.y-1);
    }
    private static void SwapX(ref Vector2 point0, ref Vector2 point1)
    {
        Vector2 tmp = point0;
        point0 = new Vector2(point1.x, point0.y);
        point1 = new Vector2(tmp.x, point1.y);
    }
    private static void SwapY(ref Vector2 point0, ref Vector2 point1)
    {
        Vector2 tmp = point0;
        point0 = new Vector2(point0.x, point1.y);
        point1 = new Vector2(point1.x, tmp.y);
    }
    public static Rect DrawFilledRectangle(PIATexture tex, Vector2 startingPoint, Vector2 finalPoint, Color color) {
        Rect rectangle = DrawRectangle(tex, startingPoint, finalPoint, color);
        for (int x = (int)rectangle.x+1; x < rectangle.xMax; x++)
        {
            for (int y = (int)rectangle.y; y > rectangle.y - rectangle.height - 1; y--)
            {
                tex.Paint(x,y,color);
            }
        }
        tex.Texture.Apply();

        return rectangle;
    }
    public static Rect DrawRectangle(PIATexture tex, Vector2 startingPoint, Vector2 finalPoint,Color color) {
        // 0,0 on upper left corner
        Rect rectangle;

        TransformToLeftTop(ref startingPoint, tex.Texture.height);
        TransformToLeftTop(ref finalPoint, tex.Texture.height);

        if (startingPoint.x > finalPoint.x)
            SwapX(ref startingPoint, ref finalPoint);
        if (startingPoint.y < finalPoint.y)
            SwapY(ref startingPoint, ref finalPoint);

        rectangle = new Rect(startingPoint.x, startingPoint.y, Mathf.Abs(finalPoint.x - startingPoint.x), Mathf.Abs(finalPoint.y - startingPoint.y));

        // Upper segment
        
        for (int x = (int)startingPoint.x; x <= finalPoint.x; x++)
        {
            tex.Paint(x, (int)startingPoint.y, color,true,false);
        }
        // Downer segment

        for (int x = (int)startingPoint.x; x <= finalPoint.x; x++)
        {
            tex.Paint(x, (int)finalPoint.y, color,true,false);
        }

        // Left segment   

        for (int y = (int)startingPoint.y; y >= finalPoint.y; y--)
        {
            tex.Paint((int)startingPoint.x, y, color,true,false);
        }

        // Right segment

        for (int y = (int)startingPoint.y; y >= finalPoint.y; y--)
        {
            tex.Paint((int)finalPoint.x, y, color,true,false);

        }

        tex.Texture.Apply();

        return rectangle;
    }
    public static void ClearRect(PIATexture tex, Rect rectangle) {
        for (int x = (int)rectangle.x; x <= rectangle.xMax; x++)
        {
            for (int y = (int)rectangle.y; y >= rectangle.y - rectangle.height; y--)
            {
                tex.Paint(x, y, ClearColor);
            }
        }
        tex.Texture.Apply();
        
    }
    #endregion

    #region Fields
    
    private Vector2 downPoint;
    private Vector2 upPoint;
    private Rect selectedRect;

    #endregion

    #region Properties

    public PIAToolType ToolType { get; set; }
    public Color FirstColor { get; set; }
    public Color SecondColor { get; set; }
    #endregion

    #region Methods

    public PIADrawer()
    {
        // INIT DRAWER
        ToolType = PIAToolType.Paint;
        FirstColor = Color.black;
        SecondColor = ClearColor;
    }

    public void OnGUIExecute(Event e, Vector2 pixelCoordinate)
    {
        // mouse is outside the grid
        if (pixelCoordinate.x < 0 || pixelCoordinate.y < 0)
            return;

        // this is used to preview where we are going to draw
        PIATexture helper = PIAEditorWindow.Instance.SelectionTexture;

        PIAFrame frame = PIASession.Instance.ImageData.CurrentFrame;
        int width = PIASession.Instance.ImageData.Width;
        int height = PIASession.Instance.ImageData.Height;

        // this could have been a much better class based state machine but I'm fucking lazy (maybe in PRO version?)
        switch (ToolType)
        {
            case PIAToolType.Paint:

                if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                {
                    if (e.button == 0)
                    {
                        frame.GetCurrentImage().Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, FirstColor);
                        Debug.Log(frame.GetCurrentImage().Texture.width + "," + frame.GetCurrentImage().Texture.height);
                    }
                    if (e.button == 1)
                    {
                        frame.GetCurrentImage().Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, SecondColor);
                    }
                }
                else {
                    helper.Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, new Color(Color.black.r, Color.black.g, Color.black.b, 0.2f),false,true, false);

                }

                break;
            case PIAToolType.Erase:
                if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                {
                    frame.GetCurrentImage().Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, ClearColor);
                }
                else {
                    helper.Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f), false,true,false);
                }
                break;
            case PIAToolType.Rectangle:

                if (e.type == EventType.MouseDown)
                {

                    downPoint = new Vector2((int)pixelCoordinate.x, (int)pixelCoordinate.y);
                    if (e.button == 0)
                    {
                        DrawRectangle(helper, downPoint, pixelCoordinate, new Color(FirstColor.r, FirstColor.g, FirstColor.b, 0.5f));
                    }
                    if (e.button == 1)
                        DrawRectangle(helper, downPoint, pixelCoordinate, new Color(SecondColor.r, SecondColor.g, SecondColor.b, 0.5f));

                }
                if (e.type == EventType.MouseDrag)
                {
                    if (e.button == 0)
                    {
                        DrawRectangle(helper, downPoint, pixelCoordinate, new Color(FirstColor.r, FirstColor.g, FirstColor.b, 0.5f));
                    }
                    if (e.button == 1)
                        DrawRectangle(helper, downPoint, pixelCoordinate, new Color(SecondColor.r, SecondColor.g, SecondColor.b, 0.5f));

                }
                if (e.type == EventType.MouseUp)
                {
                    upPoint = new Vector2((int)pixelCoordinate.x, (int)pixelCoordinate.y);
                    if (e.button == 0)
                    {
                        DrawRectangle(frame.GetCurrentImage(), downPoint, upPoint, FirstColor);
                    }
                    if (e.button == 1)
                        DrawRectangle(frame.GetCurrentImage(), downPoint, upPoint, SecondColor);

                    helper.ClearTexture(true);
                }
                break;
            case PIAToolType.RectangleFilled:

                if (e.type == EventType.MouseDown)
                {

                    downPoint = new Vector2((int)pixelCoordinate.x, (int)pixelCoordinate.y);
                    if (e.button == 0)
                    {
                        DrawFilledRectangle(helper, downPoint, pixelCoordinate, new Color(FirstColor.r, FirstColor.g, FirstColor.b, 0.5f));
                    }
                    if (e.button == 1)
                        DrawFilledRectangle(helper, downPoint, pixelCoordinate, new Color(SecondColor.r, SecondColor.g, SecondColor.b, 0.5f));

                }
                if (e.type == EventType.MouseDrag)
                {
                    if (e.button == 0)
                    {
                        DrawFilledRectangle(helper, downPoint, pixelCoordinate, new Color(FirstColor.r, FirstColor.g, FirstColor.b, 0.5f));
                    }
                    if (e.button == 1)
                        DrawFilledRectangle(helper, downPoint, pixelCoordinate, new Color(SecondColor.r, SecondColor.g, SecondColor.b, 0.5f));

                }
                if (e.type == EventType.MouseUp)
                {
                    upPoint = new Vector2((int)pixelCoordinate.x, (int)pixelCoordinate.y);
                    if (e.button == 0)
                    {
                        DrawFilledRectangle(frame.GetCurrentImage(), downPoint, upPoint, FirstColor);
                    }
                    if (e.button == 1)
                        DrawFilledRectangle(frame.GetCurrentImage(), downPoint, upPoint, SecondColor);

                    helper.ClearTexture(true);
                }
                break;
            case PIAToolType.Selection:
                if (e.type == EventType.MouseDown)
                {

                    downPoint = new Vector2((int)pixelCoordinate.x, (int)pixelCoordinate.y);
                    DrawFilledRectangle(helper, downPoint, pixelCoordinate, new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f));

                }
                if (e.type == EventType.MouseDrag)
                {
                    selectedRect = DrawFilledRectangle(helper, downPoint, pixelCoordinate, new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f));
                }
                if (e.keyCode == KeyCode.Delete) {
                    ClearRect(frame.GetCurrentImage(), selectedRect);
                    helper.ClearTexture(true);
                }

                break;
            case PIAToolType.Dithering:
                if ((pixelCoordinate.x + pixelCoordinate.y) % 2 == 1)
                {
                    helper.Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, new Color(Color.red.r, Color.red.g, Color.red.b, 0.2f), false, true, false);
                    return;
                }

                if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                {
                    if (e.button == 0)
                    {
                        frame.GetCurrentImage().Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, FirstColor);
                    }
                    if (e.button == 1)
                    {
                        frame.GetCurrentImage().Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, SecondColor);
                    }
                }
                else
                {
                    helper.Paint((int)pixelCoordinate.x, height - (int)pixelCoordinate.y - 1, new Color(Color.black.r, Color.black.g, Color.black.b, 0.2f), false, true, false);

                }

                break;
        }


    }
   

    #endregion
    

}

