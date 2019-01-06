using UnityEditor;
using UnityEngine;
using Piacenti.EditorTools;
using System.Collections.Generic;
using System;

public class PIAEditorWindow : EditorWindow {

    #region Static

    public static PIAEditorWindow window;
    private static PIAEditorWindow _instance;
    public static PIAEditorWindow Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion

    #region Const

    // grid rect size multiplier
    const float INIT_SCALE_MULTIPLIER = 0.95f;

    #endregion
  

    #region Fields

    WindowSection header;
    WindowSection leftSide;
    WindowSection body;
    WindowSection rightSide;

    // only used to draw sections background on loop
    List<WindowSection> sections;

    PIAInputArea globalInputArea;
    PIAInputArea bodyInputArea;

    PIADrawer drawer;

    GUISkin skin;
    PIAGrid grid;

    float scaleMultiplier = INIT_SCALE_MULTIPLIER;
    float imageOffsetX = 0;
    float imageOffsetY = 0;
    
    Vector2 mouseCellCoordinate;
    Color blackBarBGColor = new Color(0.1294f, 0.1294f, 0.1294f, 1.0000f);

    // invisible sliders that appear adding too many layers or frames
    Vector2 framesSlider;
    Vector2 layersSlider;

    // little green rect on the selected tool in toolbar
    Rect selectedToolRect = Rect.zero;

    Texture2D pen;
    Texture2D eraser;
    Texture2D squareTool;
    Texture2D filledSquareTool;
    Texture2D selectionBox;
    Texture2D ditheringTool;
    Texture2D selectedToolBG;
    Texture2D blackBarBG;

    #endregion

    #region Properties

    public Rect BodyRect { get { return body.GetRect(); } }

    // this is the texture on top of the frame texture to preview where the drawing tool is going to paint (maybe shit name?)
    public PIATexture SelectionTexture { get; set; }

    #endregion


    #region Methods
    [MenuItem("QFramework/Pixel Kit/Open Panel")]
    public static void ShowWindow()
    {
        // INIT WINDOW
        window = GetWindow<PIAEditorWindow>();
        Vector2 windowSize = new Vector2(995, 800);
        window.position = new Rect(Screen.width / 2 - windowSize.x / 2, 100, windowSize.x, windowSize.y);
        window.minSize = new Vector2(600, 200);
        Texture2D windowIcon = Application.HasProLicense()? PIATextureDatabase.Instance.GetTexture("brush") : PIATextureDatabase.Instance.GetTexture("brushblack");
        window.titleContent = new GUIContent("Pixel Editor",windowIcon);
        window.autoRepaintOnSceneChange = false;
        window.Show();
    }

    private void OnEnable()
    {
        if(_instance==null)
            _instance = this;

        //DRAWER
        drawer = new PIADrawer();
        grid = new PIAGrid();

        //INIT INPUT AREAS 
        globalInputArea = new PIAInputArea();
        globalInputArea.OnGUIUpdate += ChangeSelectedTool;
        bodyInputArea = new PIAInputArea();
        bodyInputArea.OnGUIUpdate += ChangeImageScaleMultiplier;
        bodyInputArea.OnGUIUpdate += (e) => drawer.OnGUIExecute(e,mouseCellCoordinate);
        bodyInputArea.OnGUIUpdate += ChangeImageOffset;
        SelectionTexture = new PIATexture();
        SelectionTexture.Init(PIASession.Instance.ImageData.Width, PIASession.Instance.ImageData.Height, 0);

        //INIT WINDOW SECTIONS
        InitializeSections();

        //INIT GRAPHICS
        skin = Resources.Load<GUISkin>("Skins/PIAPixelArtEditorSkin");
        pen = PIATextureDatabase.Instance.GetTexture("pen");
        eraser = PIATextureDatabase.Instance.GetTexture("eraser");
        squareTool = PIATextureDatabase.Instance.GetTexture("squaretool");
        filledSquareTool = PIATextureDatabase.Instance.GetTexture("filledsquaretool");
        selectionBox = PIATextureDatabase.Instance.GetTexture("selectionbox");
        ditheringTool = PIATextureDatabase.Instance.GetTexture("ditheringtool");
        selectedToolBG = PIATextureDatabase.Instance.GetTexture("sideslight");
        blackBarBG= new Texture2D(1, 1);
        blackBarBG.SetPixel(0, 0, blackBarBGColor);
        blackBarBG.Apply();
    }
    private void OnDisable()
    {
        // closing opened windows
        PIAExportSettingsWindow.CloseWindow();
        PIAExtendedPreviewWindow.CloseWindow();

        if (PIASession.Instance.IsDirty) {
            if (EditorUtility.DisplayDialog("Project Has Been Modified", "Do you want to save any changes made?", "Yes", "No"))
                PIASession.Instance.SaveAsset();
            else
                PIASession.Instance.LoadNewAsset(16, 16);
        }
    }
    private void Update()
    {
        PIAAnimator.Instance.Update();

        // this is painful performance 
        //window.Repaint();

    }
    private void OnGUI()
    {

        DrawLayouts();
        bodyInputArea.GUIUpdate(body.GetRect());
        globalInputArea.GUIUpdate(new Rect(0,0,position.width,position.height));

        DrawHeader();
        DrawLeftSection();
        DrawRightSection();
        DrawBody();

        // this must be placed in OnGUI in order to get global scope position
        PIATooltipUtility.DrawTooltips(skin.GetStyle("tooltip"));

        mouseCellCoordinate = grid.WorldToCellPosition(PIAInputArea.MousePosition);
        if (mouseCellCoordinate.x < 0 || mouseCellCoordinate.y < 0 || mouseCellCoordinate.x >= PIASession.Instance.ImageData.Width || mouseCellCoordinate.y >= PIASession.Instance.ImageData.Height)
            mouseCellCoordinate = new Vector2(-1, -1);
        window.Repaint();
    }

    private void InitializeSections()
    {
        Texture2D sides = PIATextureDatabase.Instance.GetTexture("sides");
        
        sections = new List<WindowSection>();
        header = new WindowSection(new Rect(0, 0, position.width, 40), blackBarBGColor);
        leftSide = new WindowSection(new Rect(0, header.GetRect().height, 220, position.height - header.GetRect().height), sides);
        rightSide = new WindowSection(new Rect(position.width - 220, header.GetRect().height, 220, position.height - header.GetRect().height), sides);
        body = new WindowSection(new Rect(leftSide.GetRect().width, header.GetRect().height, position.width - leftSide.GetRect().width - rightSide.GetRect().width,
            position.height - header.GetRect().height ), new Color(0.6275f, 0.6275f, 0.6275f, 1.0000f));

        sections.Add(header);
        sections.Add(leftSide);
        sections.Add(rightSide);
        sections.Add(body);
    }
    private void DrawLayouts()
    {
        // layouts need to get refreshed in order to maintain aspect ratio
        header.SetRect(0, 0, position.width, 45);
        leftSide.SetRect(0, header.GetRect().height, 220, position.height - header.GetRect().height);
        rightSide.SetRect(position.width - 220, header.GetRect().height, 220, position.height - header.GetRect().height);
        body.SetRect(leftSide.GetRect().width, header.GetRect().y+header.GetRect().height, position.width - leftSide.GetRect().width - rightSide.GetRect().width,
            position.height - header.GetRect().height);

        foreach (var item in sections)
        {
            GUI.DrawTexture(item.GetRect(), item.GetTexture());
        }
    }
    
    private void DrawHeader()
    {
        GUILayout.BeginArea(header.GetRect());
        {
            DrawToolbar();
        }
        GUILayout.EndArea();
    }
    private void DrawLeftSection() {
        Rect leftRect = leftSide.GetRect();

        GUILayout.BeginArea(leftRect);
        {

            Rect preview  = DrawPreview(leftRect);
            DrawLayers(preview);
            DrawProjectName(leftRect);

        }
        GUILayout.EndArea();

    }
    private void DrawRightSection() {
        Rect rightRect = rightSide.GetRect();

        GUILayout.BeginArea(rightRect);
        {
            DrawFrames(rightRect);
            DrawSessionBar(rightRect);
            DrawGridInfo(rightRect);

        }
        GUILayout.EndArea();

    }
    private void DrawBody()
    {
        float scale = body.GetRect().width * scaleMultiplier;
        grid.Grid= new Rect((body.GetRect().width / 2 - scale / 2) + imageOffsetX, (BodyRect.center.y - scale / 2) - header.GetRect().height+ imageOffsetY, scale, scale);

        // here we draw the grid and the current layer filtered texture of the frame
        GUILayout.BeginArea(body.GetRect());
        {
            EditorGUI.DrawTextureTransparent(grid.Grid, PIASession.Instance.ImageData.CurrentFrame.GetFrameTextureWithLayerFilters());
            GUI.DrawTexture(grid.Grid, SelectionTexture.Texture);
            DrawGrid(grid.Grid);
            SelectionTexture.ClearTexture();
        }
        GUILayout.EndArea();


    }

    private void DrawToolbar()
    {
        float iconWidth = 36;
        int spaceBetweenIcons = 8;

        Rect firstColorRect;
        Rect secondColorRect;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        {
            // SELECTED TOOL BOX 
            switch (drawer.ToolType) {
                case PIAToolType.Paint:
                    selectedToolRect = new Rect(15, 4, iconWidth, iconWidth);
                    break;
                case PIAToolType.Erase:
                    selectedToolRect = new Rect(15 + iconWidth + spaceBetweenIcons, 4, iconWidth, iconWidth);

                    break;
                case PIAToolType.Selection:
                    selectedToolRect = new Rect(15 + iconWidth * 2 + spaceBetweenIcons * 2, 4, iconWidth, iconWidth);
                    break;
                case PIAToolType.Rectangle:
                    selectedToolRect = new Rect(15 + iconWidth * 3 + spaceBetweenIcons * 3 , 4, iconWidth, iconWidth);
                    break;
                case PIAToolType.RectangleFilled:
                    selectedToolRect = new Rect(15 + iconWidth * 4 + spaceBetweenIcons * 4, 4, iconWidth, iconWidth);
                    break;
                
                case PIAToolType.Dithering:
                    selectedToolRect = new Rect(15 + iconWidth * 5 + spaceBetweenIcons * 5, 4, iconWidth, iconWidth);
                    break;

            }

            GUI.DrawTexture(selectedToolRect, selectedToolBG);

            // PAINT TOOL
            GUILayout.Space(15);
            if (GUILayout.Button(pen, skin.GetStyle("toolbarbutton"), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(iconWidth)))
            {
                drawer.ToolType = PIAToolType.Paint;
            }

            Rect penGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect());
            Rect penTooltipRect = new Rect(0, 0, 75, 22.5f);
            PIATooltip penTooltip = new PIATooltip(penTooltipRect, "Paint (Q)");
            PIATooltip.SetPositionPreset(ref penTooltip, penGlobalRect, PIATooltip.PIATooltipPreset.Down);
            PIATooltipUtility.AddTooltip(penGlobalRect, penTooltip);

            GUILayout.Space(spaceBetweenIcons);

            // ERASER TOOL
            if (GUILayout.Button(eraser, skin.GetStyle("toolbarbutton"), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(iconWidth)))
            {
                drawer.ToolType = PIAToolType.Erase;
            }

            Rect eraseGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect());
            Rect eraseTooltipRect = new Rect(0, 0, 75, 22.5f);
            PIATooltip eraseTooltip = new PIATooltip(eraseTooltipRect, "Erase (E)");
            PIATooltip.SetPositionPreset(ref eraseTooltip, eraseGlobalRect, PIATooltip.PIATooltipPreset.Down);
            PIATooltipUtility.AddTooltip(eraseGlobalRect, eraseTooltip);

            GUILayout.Space(spaceBetweenIcons);

            // SELECTION TOOL
            if (GUILayout.Button(selectionBox, skin.GetStyle("toolbarbutton"), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(iconWidth)))
            {
                drawer.ToolType = PIAToolType.Selection;
            }
            Rect selectionBoxGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect());
            Rect selectionBoxTooltipRect = new Rect(0, 0, 150, 22.5f);
            PIATooltip selectionBoxTooltip = new PIATooltip(selectionBoxTooltipRect, "Filled Erase (Shift+E)");
            PIATooltip.SetPositionPreset(ref selectionBoxTooltip, selectionBoxGlobalRect, PIATooltip.PIATooltipPreset.Down);
            PIATooltipUtility.AddTooltip(selectionBoxGlobalRect, selectionBoxTooltip);

            GUILayout.Space(spaceBetweenIcons);

            // RECTANGLE TOOL
            if (GUILayout.Button(squareTool, skin.GetStyle("toolbarbutton"), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(iconWidth)))
            {
                drawer.ToolType = PIAToolType.Rectangle;
            }

            Rect squareGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect());
            Rect squareTooltipRect = new Rect(0, 0, 105, 22.5f);
            PIATooltip squareTooltip = new PIATooltip(squareTooltipRect, "Rectangle (R)");
            PIATooltip.SetPositionPreset(ref squareTooltip, squareGlobalRect, PIATooltip.PIATooltipPreset.Down);
            PIATooltipUtility.AddTooltip(squareGlobalRect, squareTooltip);

            GUILayout.Space(spaceBetweenIcons);

            // FILLED RECTANGLE TOOL
            if (GUILayout.Button(filledSquareTool, skin.GetStyle("toolbarbutton"), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(iconWidth)))
            {
                drawer.ToolType = PIAToolType.RectangleFilled;
            }

            Rect filledSquareGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect());
            Rect filledSquareTooltipRect = new Rect(0, 0, 175, 22.5f);
            PIATooltip filledSquareTooltip = new PIATooltip(filledSquareTooltipRect, "Filled Rectangle (Shift+R)");
            PIATooltip.SetPositionPreset(ref filledSquareTooltip, filledSquareGlobalRect, PIATooltip.PIATooltipPreset.Down);
            PIATooltipUtility.AddTooltip(filledSquareGlobalRect, filledSquareTooltip);

            GUILayout.Space(spaceBetweenIcons);

            // DITHERING TOOL
            if (GUILayout.Button(ditheringTool, skin.GetStyle("toolbarbutton"), GUILayout.MaxWidth(iconWidth), GUILayout.MaxHeight(iconWidth)))
            {
                drawer.ToolType = PIAToolType.Dithering;
            }
            Rect ditheringToolGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect());
            Rect ditheringToolTooltipRect = new Rect(0, 0, 120, 22.5f);
            PIATooltip ditheringToolTooltip = new PIATooltip(ditheringToolTooltipRect, "Dithering tool (T)");
            PIATooltip.SetPositionPreset(ref ditheringToolTooltip, ditheringToolGlobalRect, PIATooltip.PIATooltipPreset.Down);
            PIATooltipUtility.AddTooltip(ditheringToolGlobalRect, ditheringToolTooltip);

            // COLORS FIELDS RECT
            firstColorRect = new Rect(GUILayoutUtility.GetLastRect().x + 40 + spaceBetweenIcons, 8, 60, 20);
            secondColorRect = new Rect(firstColorRect.x + 35, 18, 60, 20);

        }
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();


        

        // DRAWING COLORS FIELDS
        drawer.FirstColor = EditorGUI.ColorField(firstColorRect, new GUIContent(""), drawer.FirstColor, false, true, false, null);
        drawer.SecondColor = EditorGUI.ColorField(secondColorRect, new GUIContent(""), drawer.SecondColor, false, true, false, null);

    }
    private void DrawProjectName(Rect parent)
    {
        float projectNameRectHeight = 40;
        Rect projectNameRect = new Rect(0, parent.height - projectNameRectHeight, parent.width, projectNameRectHeight);

        // DRAWING BACKGROUND
        GUI.DrawTexture(projectNameRect, blackBarBG);

        // DRAWING PROJECT NAME LABEL
        GUILayout.BeginArea(projectNameRect);
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(PIASession.Instance.ProjectName, skin.GetStyle("projectname"));
                GUILayout.FlexibleSpace();


            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

        }
        GUILayout.EndArea();
        

    }
    private void DrawGridInfo(Rect parent)
    {
        PIAImageData imageData = PIASession.Instance.ImageData;

        //GRID INFO RECT
        float gridInfoRectHeight = 40;
        Rect gridInfoRect = new Rect(0, parent.height - gridInfoRectHeight, parent.width, gridInfoRectHeight);

        //DRAWING STUFF
      
        GUI.DrawTexture(gridInfoRect, blackBarBG);

        GUILayout.BeginArea(gridInfoRect);
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("[" + imageData.Width + "x" + imageData.Height + "]", skin.GetStyle("editorbutton2"));
                    GUILayout.Label(mouseCellCoordinate.ToString(), skin.GetStyle("editorbutton2"));
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();


            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

        }
        GUILayout.EndArea();


    }
    private void DrawGrid(Rect rect)
    {

        if (grid.CellWidth <= 10 || grid.CellHeight <= 10)
            return;

        // weird bug with this but definitely looking better
        // -------------------------------------------------------------------------------------
        //for (float offsetX = 0; offsetX <= rect.width + 1; offsetX += grid.CellWidth)
        //{   
        //    EditorGUI.DrawRect(new Rect(new Vector2(rect.x + offsetX, rect.y), new Vector2(1, rect.height)), Color.black);
        //}
        //for (float offsetY = 0; offsetY <= rect.height + 1; offsetY += grid.CellHeight)
        //{
        //    EditorGUI.DrawRect(new Rect(new Vector2(rect.x, rect.y + offsetY), new Vector2(rect.width, 1)), Color.black);
        //}
        // -------------------------------------------------------------------------------------

        Handles.BeginGUI();
        {
            Handles.color = new Color(Color.black.r, Color.black.g, Color.black.b, 1);

            for (float offsetX = 0; offsetX <= rect.width; offsetX += grid.CellWidth)
            {
                Handles.DrawLine(new Vector2(rect.x + offsetX, rect.y), new Vector2(rect.x + offsetX, rect.y + rect.height));
            }
            for (float offsetY = 0; offsetY <= rect.height; offsetY += grid.CellHeight)
            {
                Handles.DrawLine(new Vector2(rect.x, rect.y + offsetY), new Vector2(rect.x + rect.width, rect.y + +offsetY));

            }
            Handles.color = Color.white;
        }
        Handles.EndGUI();
    }

    private Rect DrawPreview(Rect parent) {
        Vector2 offset = new Vector2(20, 20);

        // PREVIEW RECT
        Vector2 previewLocalPosition = new Vector2(offset.x, offset.y);
        Vector2 previewDimension = new Vector2(parent.width - offset.x * 2, parent.width - offset.x * 2);
        Rect previewRect = new Rect(previewLocalPosition, previewDimension);

        // FPS COUNTER RECT
        Vector2 fpsLocalPosition = new Vector2(previewRect.x, previewRect.yMax);
        Vector2 fpsDimension = new Vector2(40, 20);
        Rect fpsRect = new Rect(fpsLocalPosition, fpsDimension);

        // SLIDER RECT
        Vector2 speedSliderLocalPosition = new Vector2(fpsRect.xMax, previewRect.yMax);
        Vector2 speedSliderDimension = new Vector2(previewRect.width - fpsRect.width, 20);
        Rect speedSliderRect = new Rect(speedSliderLocalPosition, speedSliderDimension);

        // BG RECT
       
        Rect previewBGRect = new Rect(previewRect.x - offset.x / 2, previewRect.y - offset.y / 2, previewRect.width + offset.x,
            previewRect.height + speedSliderRect.height + offset.y / 2);

        // EXTEND WINDOW RECT
        Vector2 extendWindowRectOffset = new Vector2(5, 5);
        float extendWindowRectWidth = 24;
        float extendWindowRectHeight = 24;
        Rect extendWindowRect = new Rect(previewRect.xMax - extendWindowRectWidth - extendWindowRectOffset.x, 
            previewRect.yMax - extendWindowRectHeight - extendWindowRectOffset.y, extendWindowRectWidth, extendWindowRectHeight);

        // DRAWING STUFF
        GUI.DrawTexture(previewBGRect, blackBarBG);
        EditorGUI.DrawTextureTransparent(previewRect,PIAAnimator.Instance.GetFrameOrFirst().GetFrameTexture());
        GUI.Label(fpsRect, PIAAnimator.Instance.Speed + " FPS", skin.GetStyle("fpscounter"));
        PIAAnimator.Instance.Speed = (int)GUI.HorizontalSlider(speedSliderRect, PIAAnimator.Instance.Speed, 0, 24);
        if (PIAInputArea.IsMouseInsideRect(previewBGRect))
        {
            if (GUI.Button(extendWindowRect, GUIContent.none,skin.GetStyle("extendpreview")))
                PIAExtendedPreviewWindow.ShowWindow();
            Rect extendWindowGlobalRect = PIATooltipUtility.ChildToGlobalRect(extendWindowRect, parent);
            Rect extendWindowTooltipRect = new Rect(0, 0, 105, 22.5f);
            PIATooltip extendWindowTooltip = new PIATooltip(extendWindowTooltipRect, "Extend preview");
            PIATooltip.SetPositionPreset(ref extendWindowTooltip, extendWindowGlobalRect, PIATooltip.PIATooltipPreset.Down);
            PIATooltipUtility.AddTooltip(extendWindowGlobalRect, extendWindowTooltip);
        }
        


        return previewBGRect;
    }
    private void DrawLayers(Rect verticalParent) {

        Vector2 offset = new Vector2(10, 20);

        // INIT LAYER RECT
        float layerRectPositionX = verticalParent.x + offset.x;
        float layerRectPositionY = verticalParent.yMax + offset.y;
        float layerRectWidth = verticalParent.width-offset.x*2;
        float layerRectHeight = 40;
        float spaceBetweenLayers = 10;

        // SCROLL VIEW STUFF
        Rect viewRect = new Rect(layerRectPositionX, layerRectPositionY, layerRectWidth+ offset.x, (layerRectHeight + spaceBetweenLayers) * (PIASession.Instance.ImageData.Layers.Count + 1) + offset.y*2);
        Rect sliderRect = new Rect(layerRectPositionX, layerRectPositionY, layerRectWidth+offset.x, leftSide.GetRect().height - verticalParent.height - offset.y);

        // caching and changing default gui skins for scroll view 
        GUIStyle nativeVerticalScrollbarThumb = GUI.skin.verticalScrollbarThumb;
        GUI.skin.verticalScrollbarThumb.normal.background = PIATextureDatabase.Instance.GetTexture("empty");
        GUIStyle nativeVerticalScrollbarDownButton = GUI.skin.verticalScrollbarDownButton;
        GUI.skin.verticalScrollbarDownButton.normal.background = PIATextureDatabase.Instance.GetTexture("empty");
        GUIStyle nativeVerticalScrollbarUpButton = GUI.skin.verticalScrollbarUpButton;
        GUI.skin.verticalScrollbarUpButton.normal.background = PIATextureDatabase.Instance.GetTexture("empty");

        // DRAWING LAYERS 
        layersSlider = GUI.BeginScrollView(sliderRect, layersSlider, viewRect, false, false, skin.GetStyle("horizontalscrollbar"), skin.GetStyle("verticalscrollbar"));
        {
            for (int i = 0; i < PIASession.Instance.ImageData.Layers.Count; i++)
            {
                var item = PIASession.Instance.ImageData.Layers[i];
                Rect layerRect = new Rect(layerRectPositionX, layerRectPositionY, layerRectWidth, layerRectHeight);

                GUI.DrawTexture(layerRect, blackBarBG);
                GUILayout.BeginArea(layerRect);
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginHorizontal();
                    {
                        // LABEL
                        GUILayout.BeginVertical();
                        {
                            GUILayout.FlexibleSpace();

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(15);
                                GUILayout.Label(item.Name, skin.GetStyle("editorbutton2"));
                                GUILayout.FlexibleSpace();

                            }
                            GUILayout.EndHorizontal();
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndVertical();

                        // EYE & DELETE & TOOLTIP
                        GUILayout.BeginVertical();
                        {
                            GUILayout.FlexibleSpace();

                            GUILayout.BeginHorizontal();
                            {
                                item.Hidden = GUILayout.Toggle(item.Hidden, GUIContent.none, skin.GetStyle("layereye"), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30));
                                Rect layerEyeGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect(), layerRect, new Rect(-5, 40, 1, 1));
                                Rect layerEyeTooltipRect = new Rect(0, 0, 150, 22.5f);
                                PIATooltip layerEyeTooltip = new PIATooltip(layerEyeTooltipRect, "Show layer On / Off");
                                PIATooltip.SetPositionPreset(ref layerEyeTooltip, layerEyeGlobalRect, PIATooltip.PIATooltipPreset.Down);
                                PIATooltipUtility.AddTooltip(layerEyeGlobalRect, layerEyeTooltip);
                                GUILayout.Space(5);
                                if (i != 0)
                                {
                                    if (GUILayout.Button(GUIContent.none, skin.GetStyle("deletelayer"), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30)))
                                    {
                                        PIASession.Instance.ImageData.RemoveLayer(i);
                                        return;
                                    }
                                    Rect deleteLayerGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect(), layerRect, new Rect(-5, 40, 1, 1));
                                    Rect deleteLayerTooltipRect = new Rect(0, 0, 50, 22.5f);
                                    PIATooltip deleteLayerTooltip = new PIATooltip(deleteLayerTooltipRect, "Delete");
                                    PIATooltip.SetPositionPreset(ref deleteLayerTooltip, deleteLayerGlobalRect, PIATooltip.PIATooltipPreset.Down);
                                    PIATooltipUtility.AddTooltip(deleteLayerGlobalRect, deleteLayerTooltip);


                                }
                                GUILayout.Space(5);
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.FlexibleSpace();


                        }
                        GUILayout.EndVertical();

                    }
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();


                }
                GUILayout.EndArea();

                // LAYER SELECTION BUTTON
                if (GUI.Button(layerRect, GUIContent.none, skin.GetStyle("bglayerbutton")))
                {
                    PIASession.Instance.ImageData.CurrentLayer = i;
                }
                layerRectPositionY += layerRectHeight + spaceBetweenLayers;

                // SELECTED LAYER OVERLAY
                if (i == PIASession.Instance.ImageData.CurrentLayer)
                {
                    GUI.Label(layerRect, GUIContent.none, skin.GetStyle("selectedlayeroverlay"));
                }

                PIASession.Instance.ImageData.Layers[i] = item;
            }

            // ADD LAYER 
            Rect addLayerRect = new Rect(layerRectPositionX, layerRectPositionY, layerRectWidth, layerRectHeight);
            GUI.DrawTexture(addLayerRect, blackBarBG);
            GUILayout.BeginArea(addLayerRect);
            {
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                {
                    // ICON
                    GUILayout.BeginVertical();
                    {
                        GUILayout.FlexibleSpace();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(15);
                            GUILayout.Label("", skin.GetStyle("addlayerlabelicon"), GUILayout.MaxWidth(30), GUILayout.MaxHeight(30));
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();


                    }
                    GUILayout.EndVertical();

                    // LABEL
                    GUILayout.BeginVertical();
                    {
                        GUILayout.FlexibleSpace();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(15);
                            GUILayout.Label("Add layer", skin.GetStyle("editorbutton2"));
                            GUILayout.FlexibleSpace();

                        }
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();

                    }
                    GUILayout.EndVertical();

                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

            }
            GUILayout.EndArea();

            // ADD LAYER BUTTON
            if (GUI.Button(addLayerRect, "", skin.GetStyle("bglayerbutton")))
            {
                PIASession.Instance.ImageData.AddLayer();
            }
        }
        GUI.EndScrollView();

        // resetting scroll view gui skin
        GUI.skin.verticalScrollbarThumb = nativeVerticalScrollbarThumb;
        GUI.skin.verticalScrollbarDownButton = nativeVerticalScrollbarDownButton;
        GUI.skin.verticalScrollbarUpButton = nativeVerticalScrollbarUpButton;
    }
    private void DrawFrames(Rect parent)
    {
        PIAImageData imageData = PIASession.Instance.ImageData;
        Vector2 offset = new Vector2(20, 20);
        Vector2 bgSize = new Vector2(15, 15);

        // FRAME RECT
        float frameRectPositionX = offset.x;
        float frameRectPositionY = offset.y;
        float frameRectWidth = 100;
        float frameRectHeight = frameRectWidth;
        float spaceBetweenFrames = 25;

        // FRAME INDEX RECT
        float frameNumberRectWidth = 22;
        float frameNumberRectHeight = 22;

        // DELETE RECT
        float deleteFrameRectWidth = 32;
        float deleteFrameRectHeight = 32;
        Vector2 deleteFrameRectOffset = new Vector2(2, 2);

        // DUPLICATE RECT
        float duplicateFrameRectWidth = 32;
        float duplicateFrameRectHeight = 32;
        Vector2 duplicateFrameRectOffset = new Vector2(2, 2);

        // MOVE UP RECT
        float moveFrameUpRectWidth = 32;
        float moveFrameUpRectHeight = 32;
        Vector2 moveFrameUpRectOffset = new Vector2(2, 2);
       
        // MOVE DOWN RECT
        float moveFrameDownRectWidth = 32;
        float moveFrameDownRectHeight = 32;
        Vector2 moveFrameDownRectOffset = new Vector2(2, 2);

        // ADD FRAME RECT
        float addFrameIconRectWidth = 40;
        float addFrameIconRectHeight = 40;

        // SCROLL VIEW RECT
        Rect viewRect = new Rect(0, 0, parent.width, (frameRectHeight+spaceBetweenFrames) * (imageData.Frames.Count+1)+offset.y);
        Rect sliderRect = new Rect(0, 0, parent.width, parent.height-offset.y);

        // caching and changing default gui skins for scroll view 
        GUIStyle nativeVerticalScrollbarThumb = GUI.skin.verticalScrollbarThumb;
        GUI.skin.verticalScrollbarThumb.normal.background = PIATextureDatabase.Instance.GetTexture("empty");
        GUIStyle nativeVerticalScrollbarDownButton = GUI.skin.verticalScrollbarDownButton;
        GUI.skin.verticalScrollbarDownButton.normal.background = PIATextureDatabase.Instance.GetTexture("empty");
        GUIStyle nativeVerticalScrollbarUpButton = GUI.skin.verticalScrollbarUpButton;
        GUI.skin.verticalScrollbarUpButton.normal.background = PIATextureDatabase.Instance.GetTexture("empty");
        
        // DRAWING FRAMES
        framesSlider = GUI.BeginScrollView(sliderRect, framesSlider,viewRect,false,false,skin.GetStyle("horizontalscrollbar"),skin.GetStyle("verticalscrollbar"));
        {
            for (int i = 0; i < imageData.Frames.Count; i++)
            {
                var item = imageData.Frames[i];

                // refreshing rects
                Rect frameRect = new Rect(frameRectPositionX, frameRectPositionY, frameRectWidth, frameRectHeight);
                Rect frameBGRect = new Rect(frameRect.x - bgSize.x / 2, frameRect.y - bgSize.y / 2, frameRect.width + bgSize.x,
                    frameRect.height + bgSize.y);
                Rect frameNumberBGRect = new Rect(frameBGRect.xMax, frameBGRect.center.y - frameNumberRectHeight / 2, frameNumberRectWidth, frameNumberRectHeight);
                Rect deleteFrameRect = new Rect(frameRect.xMax - deleteFrameRectWidth - deleteFrameRectOffset.x,
                    frameRect.y + deleteFrameRectOffset.y, deleteFrameRectWidth, deleteFrameRectHeight);
                Rect duplicateFrameRect = new Rect(frameRect.xMax - duplicateFrameRectWidth - duplicateFrameRectOffset.x,
                    frameRect.yMax - duplicateFrameRectOffset.y - duplicateFrameRectHeight, duplicateFrameRectWidth, duplicateFrameRectHeight);
                Rect moveFrameUpFrameRect = new Rect(frameRect.x + moveFrameUpRectOffset.x,
                   frameRect.y + moveFrameUpRectOffset.y , moveFrameUpRectWidth, moveFrameUpRectHeight);
                Rect moveFrameDownFrameRect = new Rect(frameRect.x  + moveFrameDownRectOffset.x,
                                   frameRect.yMax - moveFrameDownRectOffset.y - moveFrameDownRectHeight, moveFrameDownRectWidth, moveFrameDownRectHeight);
               
                // INDEX NUMBER
                GUI.DrawTexture(frameNumberBGRect, blackBarBG);
                GUI.Label(frameNumberBGRect, i.ToString(), skin.GetStyle("editorbutton2"));

                // BG
                GUI.DrawTexture(frameBGRect, blackBarBG);
               
                // FRAME CONTENT
                EditorGUI.DrawTextureTransparent(frameRect, imageData.Frames[i].GetFrameTexture());

                if (PIAInputArea.IsMouseInsideRect(frameBGRect))
                {
                    if (imageData.Frames.Count > 1)
                    {
                        // DELETE
                        if (GUI.Button(deleteFrameRect, GUIContent.none, skin.GetStyle("deleteframe")))
                        {
                            PIASession.Instance.ImageData.RemoveFrame(i);
                        }
                        Rect deleteFrameGlobalRect = PIATooltipUtility.ChildToGlobalRect(deleteFrameRect,parent);
                        Rect deleteFrameTooltipRect = new Rect(0, 0, 50, 22.5f);
                        PIATooltip deleteFrameTooltip = new PIATooltip(deleteFrameTooltipRect, "Delete");
                        PIATooltip.SetPositionPreset(ref deleteFrameTooltip, deleteFrameGlobalRect, PIATooltip.PIATooltipPreset.Right);
                        PIATooltipUtility.AddTooltip(deleteFrameGlobalRect, deleteFrameTooltip);

                    }

                    // DUPLICATE
                    if (GUI.Button(duplicateFrameRect, GUIContent.none, skin.GetStyle("copyframe")))
                    {
                        PIAFrame newFrame = PIASession.Instance.ImageData.AddFrame();
                        newFrame.CopyFrom(item);
                        
                    }
                    Rect duplicateFrameGlobalRect = PIATooltipUtility.ChildToGlobalRect(duplicateFrameRect, parent);
                    Rect duplicateFrameTooltipRect = new Rect(0, 0, 75, 22.5f);
                    PIATooltip duplicateFrameTooltip = new PIATooltip(duplicateFrameTooltipRect, "Duplicate");
                    PIATooltip.SetPositionPreset(ref duplicateFrameTooltip, duplicateFrameGlobalRect, PIATooltip.PIATooltipPreset.Up);
                    PIATooltipUtility.AddTooltip(duplicateFrameGlobalRect, duplicateFrameTooltip);

                    // MOVE UP
                    if (i > 0) {
                        if (GUI.Button(moveFrameUpFrameRect, GUIContent.none, skin.GetStyle("moveframup")))
                        {
                            imageData.MoveFrameUp(i);
                        }
                        Rect moveFrameUpGlobalRect = PIATooltipUtility.ChildToGlobalRect(moveFrameUpFrameRect, parent);
                        Rect moveFrameUpTooltipRect = new Rect(0, 0, 75, 22.5f);
                        PIATooltip moveFrameUpTooltip = new PIATooltip(moveFrameUpTooltipRect, "Move up");
                        PIATooltip.SetPositionPreset(ref moveFrameUpTooltip, moveFrameUpGlobalRect, PIATooltip.PIATooltipPreset.Left);
                        PIATooltipUtility.AddTooltip(moveFrameUpGlobalRect, moveFrameUpTooltip);

                    }

                    // MOVE DOWN
                    if (i < imageData.Frames.Count - 1) {
                        if (GUI.Button(moveFrameDownFrameRect, GUIContent.none, skin.GetStyle("moveframedown")))
                        {
                            imageData.MoveFrameDown(i);

                        }
                        Rect moveFrameDownGlobalRect = PIATooltipUtility.ChildToGlobalRect(moveFrameDownFrameRect,parent);
                        Rect moveFrameDownTooltipRect = new Rect(0, 0, 90, 22.5f);
                        PIATooltip moveFrameDownTooltip = new PIATooltip(moveFrameDownTooltipRect, "Move down");
                        PIATooltip.SetPositionPreset(ref moveFrameDownTooltip, moveFrameDownGlobalRect, PIATooltip.PIATooltipPreset.Left);
                        PIATooltipUtility.AddTooltip(moveFrameDownGlobalRect, moveFrameDownTooltip);

                    }
                }

                // FRAME SELECTION BG
                if (GUI.Button(frameBGRect, GUIContent.none, skin.GetStyle("bglayerbutton")))
                {
                    PIASession.Instance.ImageData.CurrentFrameIndex = i;
                }

                frameRectPositionY += frameRectHeight + spaceBetweenFrames;

                // FRAME SELECTION OVERLAY
                if (i == PIASession.Instance.ImageData.CurrentFrameIndex)
                {
                    GUI.Label(frameBGRect, GUIContent.none, skin.GetStyle("selectedframeoverlay"));
                }
            }

            // ADD NEW FRAME
            Rect addFrameRect = new Rect(frameRectPositionX, frameRectPositionY, frameRectWidth, frameRectHeight);
            Rect addFrameBGRect = new Rect(addFrameRect.x - bgSize.x / 2, addFrameRect.y - bgSize.y / 2, addFrameRect.width + bgSize.x,
                    addFrameRect.height + bgSize.y);
            Rect addFrameBGLabelIcon = new Rect(addFrameRect.center.x - addFrameIconRectWidth / 2, addFrameRect.center.y - addFrameIconRectHeight / 2, addFrameIconRectWidth, addFrameIconRectHeight);
            GUI.DrawTexture(addFrameBGRect, blackBarBG);
            GUI.Label(addFrameBGLabelIcon, GUIContent.none, skin.GetStyle("addframe"));

            if (GUI.Button(addFrameRect, GUIContent.none, skin.GetStyle("bglayerbutton")))
            {
                PIASession.Instance.ImageData.AddFrame();
            }
            frameRectPositionY += frameRectHeight + spaceBetweenFrames;
        }
        GUI.EndScrollView();

        // resetting scroll view gui skin
        GUI.skin.verticalScrollbarThumb = nativeVerticalScrollbarThumb;
        GUI.skin.verticalScrollbarDownButton = nativeVerticalScrollbarDownButton;
        GUI.skin.verticalScrollbarUpButton = nativeVerticalScrollbarUpButton;


    }
    private void DrawSessionBar(Rect parent) {
        Vector2 offset = new Vector2(5, 180);
        Vector2 bgOffset = new Vector2(8, 8);

        // INIT BUTTONS RECT
        float buttonWidth = 46;
        float buttonHeight = 46;
        float spaceBetweenRects = 20;
        Rect firstRect = new Rect(parent.width - buttonWidth - offset.x, offset.y, buttonWidth, buttonHeight * 3);
        Rect firstRectBG = new Rect(firstRect.x - bgOffset.x / 2, firstRect.y - bgOffset.y / 2, firstRect.width + bgOffset.x, firstRect.height + bgOffset.y);
        
        // BG
        GUI.DrawTexture(firstRectBG, blackBarBG);
        
        GUILayout.BeginArea(firstRect);
        {
            GUILayout.BeginVertical();
            {
                // NEW SESSION
                if (GUILayout.Button(GUIContent.none, skin.GetStyle("newsession"), GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight))) {
                    PIANewImageWindow.ShowWindow();
                }
                Rect newSessionGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect(), firstRect, parent);
                Rect newSessionTooltipRect = new Rect(0, 0, 105, 45);
                PIATooltip newSessionTooltip = new PIATooltip(newSessionTooltipRect, "Create a new document");
                PIATooltip.SetPositionPreset(ref newSessionTooltip, newSessionGlobalRect, PIATooltip.PIATooltipPreset.Left);
                PIATooltipUtility.AddTooltip(newSessionGlobalRect, newSessionTooltip);

                // OPEN ASSET
                if (GUILayout.Button(GUIContent.none, skin.GetStyle("openasset"), GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight))) {
                    PIASession.Instance.LoadAsset();
                }
                Rect openAssetGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect(), firstRect, parent);
                Rect openAssetTooltipRect = new Rect(0, 0, 120, 45);
                PIATooltip openAssetTooltip = new PIATooltip(openAssetTooltipRect, "Open an existing document");
                PIATooltip.SetPositionPreset(ref openAssetTooltip, openAssetGlobalRect, PIATooltip.PIATooltipPreset.Left);
                PIATooltipUtility.AddTooltip(openAssetGlobalRect, openAssetTooltip);

                // SAVE CURRENT SESSION
                if (GUILayout.Button(GUIContent.none, skin.GetStyle("savesession"), GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight))) {
                    PIASession.Instance.SaveAsset();
                }

                Rect saveSessionGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect(), firstRect, parent);
                Rect saveSessionTooltipRect = new Rect(0, 0, 120, 45);
                PIATooltip saveSessionTooltip = new PIATooltip(saveSessionTooltipRect, "Save the current document");
                PIATooltip.SetPositionPreset(ref saveSessionTooltip, saveSessionGlobalRect, PIATooltip.PIATooltipPreset.Left);
                PIATooltipUtility.AddTooltip(saveSessionGlobalRect, saveSessionTooltip);

            }
            GUILayout.EndVertical();

        }
        GUILayout.EndArea();

        
        Rect secondRect = new Rect(firstRect.x,firstRect.yMax+ spaceBetweenRects,firstRect.width, buttonHeight * 2);
        Rect secondRectBG = new Rect(secondRect.x - bgOffset.x / 2, secondRect.y - bgOffset.y / 2, secondRect.width + bgOffset.x, secondRect.height + bgOffset.y);

        GUI.DrawTexture(secondRectBG, blackBarBG);
        GUILayout.BeginArea(secondRect);
        {
            GUILayout.BeginVertical();
            {
                
                // IMPORT IMAGE
                if (GUILayout.Button(GUIContent.none, skin.GetStyle("importtexture"), GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight))) {
                    PIASession.Instance.LoadImageFromFile();
                }
                Rect importGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect(), secondRect, parent);
                Rect importTooltipRect = new Rect(0, 0, 105, 45);
                PIATooltip importTooltip = new PIATooltip(importTooltipRect, "Import a new image");
                PIATooltip.SetPositionPreset(ref importTooltip, importGlobalRect, PIATooltip.PIATooltipPreset.Left);
                PIATooltipUtility.AddTooltip(importGlobalRect, importTooltip);

                // EXPORT PROJECT
                if (GUILayout.Button(GUIContent.none, skin.GetStyle("exporttexture"), GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight))) {
                    PIAExportSettingsWindow.ShowWindow();
                }
                Rect exportGlobalRect = PIATooltipUtility.ChildToGlobalRect(GUILayoutUtility.GetLastRect(), secondRect, parent);
                Rect exportTooltipRect = new Rect(0, 0, 105, 45);
                PIATooltip exportTooltip = new PIATooltip(exportTooltipRect, "Export the document");
                PIATooltip.SetPositionPreset(ref exportTooltip, exportGlobalRect, PIATooltip.PIATooltipPreset.Left);
                PIATooltipUtility.AddTooltip(exportGlobalRect, exportTooltip);
            }
            GUILayout.EndVertical();

        }
        GUILayout.EndArea();

    }


    public void ChangeImageScaleMultiplier(Event e)
    {
        if (e.type == EventType.ScrollWheel)
        {
            float deltaY = e.delta.y * (-1) * Time.fixedDeltaTime;
            scaleMultiplier += 0.7f * deltaY;
            scaleMultiplier = Mathf.Max(0.2f, scaleMultiplier);
        }
    }
    public void ChangeImageOffset(Event e) {
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.W:
                    imageOffsetY -= 580f * Time.fixedDeltaTime;
                    break;

                case KeyCode.S:
                    imageOffsetY += 580f * Time.fixedDeltaTime;

                    break;
                case KeyCode.A:
                    imageOffsetX -= 580f * Time.fixedDeltaTime;

                    break;

                case KeyCode.D:
                    imageOffsetX += 580f * Time.fixedDeltaTime;

                    break;
            }
        }
    }
    public void ChangeSelectedTool(Event e) {
        switch (e.keyCode) {
            case KeyCode.Q:
                drawer.ToolType = PIAToolType.Paint;
                break;
            case KeyCode.E:
                drawer.ToolType = e.shift ? PIAToolType.Selection : PIAToolType.Erase;
                break;
            case KeyCode.R:
                drawer.ToolType = e.shift ? PIAToolType.RectangleFilled : PIAToolType.Rectangle;
                break;
            case KeyCode.T:
                drawer.ToolType = PIAToolType.Dithering;
                break;
        }
    }
    #endregion


   
}

// this could have been a class too
public struct PIATooltip {
    public Rect rect;
    public string content;

    public PIATooltip(Rect _rect, string _content) {
        rect = _rect;
        content = _content;
    }
    public static void SetPositionPreset(ref PIATooltip tooltip,Rect globalRect, PIATooltipPreset preset) {
        switch (preset) {
            case PIATooltipPreset.Left:
                tooltip.rect = new Rect(globalRect.x - tooltip.rect.width-10, globalRect.y, tooltip.rect.width, tooltip.rect.height);
                break;
            case PIATooltipPreset.Right:
                tooltip.rect = new Rect(globalRect.xMax+ 10, globalRect.y, tooltip.rect.width, tooltip.rect.height);
                break;
            case PIATooltipPreset.Up:
                tooltip.rect = new Rect(globalRect.x, globalRect.y - tooltip.rect.height - 5, tooltip.rect.width, tooltip.rect.height);
                break;
            case PIATooltipPreset.Down:
                tooltip.rect = new Rect(globalRect.x, globalRect.yMax+5, tooltip.rect.width, tooltip.rect.height);
                break;
        }
    }
    public enum PIATooltipPreset {
        Left,
        Right,
        Up,
        Down
    }
}
public static class PIATooltipUtility {
    private static Dictionary<Rect,PIATooltip> tooltips = new Dictionary<Rect, PIATooltip>();

    public static Rect ChildToGlobalRect(Rect child, params Rect[] parents) {
        Rect rect = child;
        foreach (var item in parents)
        {
            rect.x += item.x;
            rect.y += item.y;
        }
        return rect;
    }
    public static void AddTooltip(Rect rect,PIATooltip tooltip)
    {
        if(!tooltips.ContainsKey(rect))
            tooltips.Add(rect,tooltip);
    }
    public static void DrawTooltips(GUIStyle style)
    {
        foreach (KeyValuePair<Rect,PIATooltip> item in tooltips)
        {
            if(item.Key.Contains(PIAInputArea.MousePosition))
                GUI.Label(item.Value.rect, item.Value.content, style);
        }
        tooltips.Clear();
    }
}
