using UnityEngine;

public class PIAGrid
{

    #region Properties

    public float CellWidth { get { return Grid.width / PIASession.Instance.ImageData.Width; } }
    public float CellHeight { get { return Grid.height / PIASession.Instance.ImageData.Height; } }
    public Rect Grid { get; set; }

    #endregion

    #region Methods

    public Vector2 WorldToCellPosition(Vector2 worldPosition)
    {
        // world position is transformed to body rect position so we can offset the body rect
        Vector2 localPosition = ParentToLocalPosition(worldPosition, PIAEditorWindow.Instance.BodyRect.position);

        float relX = (localPosition.x - Grid.x) / CellWidth;
        float relY = (localPosition.y - Grid.y) / CellHeight;

        // world position is out of body rect
        if (relX < 0)
            relX = -1;
        if (relY < 0)
            relY = -1;

        Vector2 cellPosition = new Vector2((int)relX, (int)relY);
        return cellPosition;
    }
    public Vector2 CellToWorldPosition(Vector2 gridPixel)
    {
        float relX = (gridPixel.x * CellWidth) + Grid.x;
        float relY = (gridPixel.y * CellWidth) + Grid.y;
        Vector2 worldPosition = new Vector2((int)relX, (int)relY);
        Vector2 parentPosition = LocalToParentPosition(worldPosition, PIAEditorWindow.Instance.BodyRect.position);

        return new Vector2((int)parentPosition.x, (int)parentPosition.y);
    }

    #endregion

    #region Static Methods

    public static Vector2 ParentToLocalPosition(Vector2 parentPosition, Vector2 childPosition)
    {
        return parentPosition - childPosition;
    }
    public static Vector2 LocalToParentPosition(Vector2 parentPosition, Vector2 childPosition)
    {
        return parentPosition + childPosition;
    }

    #endregion
}