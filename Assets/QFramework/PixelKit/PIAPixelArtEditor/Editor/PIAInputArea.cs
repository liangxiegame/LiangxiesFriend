using UnityEngine;
using System;
public class PIAInputArea {
    // this is useful if we want to do input stuff in a specific area (rect)

    #region Static

    public static Vector2 MousePosition { get { return e.mousePosition; } }
    public static bool IsMouseInsideRect(Rect rect) {
        return rect.Contains(MousePosition);
    }

    #endregion

    #region Fields

    private static Event e;
    public event Action<Event> OnGUIUpdate = delegate { };
    public event Action<Event> OnUpdate = delegate { };


    #endregion

    #region Properties



    #endregion

    #region Methods

    // this needs to get called in OnGUI
    public void GUIUpdate(Rect area)
    {

        e = Event.current;

        if (!area.Contains(e.mousePosition))
            return;

        OnGUIUpdate(e);

    }

    // this needs to get called in Update
    public void Update(Rect area)
    {
        if (!area.Contains(e.mousePosition))
            return;
        OnUpdate(e);
    }

    #endregion





}
