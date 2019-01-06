using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Qframework
{
	public class PixelKit : EditorWindow 
	{
		static string appName = "PixelKit";

		// there shouldnt be need to modify these values in code
		int imageSize = 32; // default image size
		Texture2D canvas;
		Texture2D canvasBackground; // canvas background
		Texture2D canvasMouseCursor;
		Texture2D blackPixel;
		Texture2D whitePixel;
		Texture2D gridCellOutLine; // current grid cell outline box
		int topOffsetY = 32; // canvas top margin
		int leftOffsetX = 16; // canvas left margin
		int pixelSizeAdjust = 16;
		GUIStyle customGUIStyle;
		int mouseX,mouseY;
		Color colorUnderCursor;

		// outline canvas
		Texture2D canvasOutlineBG;
		bool automaticOutline = false;
		bool automaticOutlineForBlack = false;
		Color outlineColor = Color.black;

		// mirror drawing mode
		bool mirrorX=false;
		bool mirrorY=false;
		bool mirrorXFix=true;
		int mirrorXOffset = 1; // 0 = have middle position for mirror

		// loading
		Texture2D selectedTexture;

		// palette
		Texture2D paletteTexture;


		// mouse cursor preview pixel
		bool mouseCursorPixel = true;
		int mouseCursorPixelPosX;
		int mouseCursorPixelPosY;
		Color oldpaintColor1 = Color.clear;

		bool isInitialized = false;

		// color stuff
		Color paintColor1 = Color.black; // main color 1
		Color paintColor2 = Color.white; // secondary color 2
		Color clearColor = Color.clear; // clear color

		// quick color slots
		Color quickColor1 = Color.red;
		Color quickColor2 = Color.green;
		Color quickColor3 = Color.blue;

		float mouseWheelSpeed = 0.01f; // slow down mouse wheel

		// grid	
		Color gridColorDark = new Color(0.25f,0.25f,0.25f,0.5f);
		Color gridColorBright = new Color(0.4f,0.4f,0.4f,0.5f);

		// toolbar
		//										 0        1        2       3       4        5        6
		string[] toolbarStrings = new string[] {"新建.."," 笔刷", "填充", "保存", "另存为", "清空", "设置"};
		int defaultToolBarMode = 1;
		int toolbarMode = 1;

		// saving
		string textureSaveName = "NewPixelImage"; // default filename
		int nameCounter = 1;
		int fakeTimeCounter = 0;
		int fakeNextTimeCounter = 250;
		bool savedAsCopy=false; 
		bool wasModified=false;

		// helpers
		bool waitingFile = false;
		bool needsUpdate = false;

		// window
		[SerializeField] static PixelKit window;

		[MenuItem ("QFramework/Pixel Kit/Simple Version")]
		static void Init () {
			window = (PixelKit)EditorWindow.GetWindow (typeof (PixelKit));
			window.titleContent= new GUIContent(appName);
			window.minSize = new Vector2(680,710); // default size, currently locked
			window.maxSize = new Vector2(681,711);
		}


		// Mainloop
		void OnGUI () 
		{

			this.wantsMouseMove = true;

			if (waitingFile) return; // we are waiting for save
			if (needsUpdate) {Repaint(); needsUpdate=false;} // we need to update
			if (!isInitialized) InitializeCanvas();



			// ** GUI **
			GUIToolbar();

			if (toolbarMode==6)
			{
				GUISettingsTab();
				return;
			}

			GUILayout.Space(19);
			GUICurrentImageInfo();


			// background canvas
			GUI.DrawTextureWithTexCoords(new Rect(leftOffsetX,topOffsetY,imageSize*pixelSizeAdjust,imageSize*pixelSizeAdjust), canvasBackground, new Rect(0f,0f,1f,1f),true);

			// drawing canvas
			GUI.DrawTextureWithTexCoords(new Rect(leftOffsetX,topOffsetY,imageSize*pixelSizeAdjust,imageSize*pixelSizeAdjust), canvas, new Rect(0f,0f,1f,1f),true);

			// outline canvas
			if (automaticOutline)
			{
				GUI.DrawTextureWithTexCoords(new Rect(leftOffsetX,topOffsetY,imageSize*pixelSizeAdjust,imageSize*pixelSizeAdjust), canvasOutlineBG, new Rect(0f,0f,1f,1f),true);
			}
	
			// parts of the GUI
			GUIMouseInfo();
			GUIDrawPreviews();
			GUICurrentColors();

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			//GUIUndoRedo(); // not working properly yet
			//GUILayout.Space(8);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(GUILayout.Width(leftOffsetX-4));
			GUILayout.Space(0); // dummy
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			GUILoadBuffer();
			GUILayout.EndVertical();
			GUIPalette();
			GUILayout.EndHorizontal();

			GUILayout.BeginArea(new Rect(527,585,182,73));
			GUIAutomaticOutlines();
			GUIMirrorToggles();
			GUIMovePanTools();
			GUILayout.EndArea();

			GUIInfoAndStatus();

			GUIEvents();

		} // OnGUI



		void GUIEvents()
		{
			Event current = Event.current;

			bool altDown=false;
			//			bool ctrlDown=false;
			//			bool shiftDown=false;

			if (Event.current.alt)	altDown = true;
			//if (Event.current.shift)	shiftDown = true;
			//if (Event.current.control)	ctrlDown = true;


			if (current.type == EventType.ScrollWheel)
			{

				if (altDown) // adjust alpha only
				{
					paintColor1.a -= Mathf.Sign(current.delta.y)*mouseWheelSpeed;
					paintColor1.a = Mathf.Clamp01(paintColor1.a);
				}else{ // darken/lighten color
					paintColor1.r -= Mathf.Sign(current.delta.y)*mouseWheelSpeed;
					paintColor1.g -= Mathf.Sign(current.delta.y)*mouseWheelSpeed;
					paintColor1.b -= Mathf.Sign(current.delta.y)*mouseWheelSpeed;

					paintColor1.r = Mathf.Clamp01(paintColor1.r);
					paintColor1.g = Mathf.Clamp01(paintColor1.g);
					paintColor1.b = Mathf.Clamp01(paintColor1.b);

					CurrentCellOutlineRebuild();
				}
				HandleUtility.Repaint();
			}

			if (current.type == EventType.KeyDown)
			{

				switch(current.keyCode)
				{
					case KeyCode.X:  // x key, swap colors
						Color tempColor = paintColor1;
						paintColor1 = paintColor2;
						paintColor2 = tempColor;
						current.Use();
						CurrentCellOutlineRebuild();
						break;

					case KeyCode.D:  // d key, restore default colors
						paintColor1 = Color.black;
						paintColor2 = Color.white;
						current.Use();
						CurrentCellOutlineRebuild();
						break;

					case KeyCode.I:  // i key, invert color1
						paintColor1.r = 1-paintColor1.r;
						paintColor1.g = 1-paintColor1.g;
						paintColor1.b = 1-paintColor1.b;
						current.Use();
						CurrentCellOutlineRebuild();
						break;

					case KeyCode.B:  // b key, brush
						toolbarMode=defaultToolBarMode;
						current.Use();
						break;

					case KeyCode.F:  // f key, fill
						toolbarMode=2;
						current.Use();
						break;

					case KeyCode.Alpha1:  // 1 = quick color slot 1
						paintColor1 = quickColor1;
						current.Use();
						CurrentCellOutlineRebuild();
						break;

					case KeyCode.Alpha2:  // 2 = quick color slot 2
						paintColor1 = quickColor2;
						current.Use();
						CurrentCellOutlineRebuild();
						break;

					case KeyCode.Alpha3:  // 3 = quick color slot 3
						paintColor1 = quickColor3;
						current.Use();
						CurrentCellOutlineRebuild();
						break;

				}
			} // keypress



			// repaint after undo
			if (Event.current.type == EventType.ValidateCommand)
			{
				switch (Event.current.commandName)
				{
					case "UndoRedoPerformed":
						HandleUtility.Repaint();
						break;
				}
			}			

			current = Event.current;
			// mouse buttons
			if (current.type == EventType.MouseDrag || current.type == EventType.MouseDown)
			{
				int x = (int) (current.mousePosition.x-leftOffsetX);
				int y = (int) (current.mousePosition.y-topOffsetY);
				int px = (int) (x/pixelSizeAdjust);
				int py = (int) ((imageSize*pixelSizeAdjust-y)/pixelSizeAdjust);


				// check canvas bounds
				if (x>=0 &&  x < imageSize*pixelSizeAdjust && y>=0 && y < imageSize*pixelSizeAdjust)
				{

					if (toolbarMode==1) // Brush
					{
						switch (current.button)
						{
							case 0: // left mouse button (paint)
								Undo.RecordObject(canvas, "Paint " + px +","+py);
								canvas.SetPixel(px,py,paintColor1);

								if (mirrorX)
								{
									canvas.SetPixel(imageSize-mirrorXOffset-px,py,paintColor1);
								}

								if (mirrorY)
								{
									canvas.SetPixel(px,imageSize-py,paintColor1);
								}

								if (mirrorX && mirrorY)
								{
									canvas.SetPixel(imageSize-mirrorXOffset-px,imageSize-1-py,paintColor1);
								}


								break;

							case 2: // middle mouse button (color pick)
								paintColor1 = canvas.GetPixel(px,py);

								break;

							case 1: // right mouse button (clear)
								Undo.RecordObject(canvas, "Erase " + px +","+py);

								if (altDown) // smart erase
								{
									if (Compare4Neighbours(px,py))
									{
										Color32 smartEraseColor = canvas.GetPixel(px,py+1);
										canvas.SetPixel(px,py,smartEraseColor);
										if (mirrorX)
										{
											canvas.SetPixel(imageSize-mirrorXOffset-px,py,smartEraseColor);
										}

										if (mirrorY)
										{
											canvas.SetPixel(px,imageSize-mirrorXOffset-py,smartEraseColor);
										}

										if (mirrorX && mirrorY)
										{
											canvas.SetPixel(imageSize-mirrorXOffset-px,imageSize-1-py,smartEraseColor);
										}

									}else{ // use average instead

										Color32 smartEraseColor = GetAverageNeighbourColor4(px,py);
										canvas.SetPixel(px,py,smartEraseColor);
										if (mirrorX)
										{
											canvas.SetPixel(imageSize-mirrorXOffset-px,py,smartEraseColor);
										}

										if (mirrorY)
										{
											canvas.SetPixel(px,imageSize-mirrorXOffset-py,smartEraseColor);
										}

										if (mirrorX && mirrorY)
										{
											canvas.SetPixel(imageSize-mirrorXOffset-px,imageSize-1-py,smartEraseColor);
										}
									}
								}else{
									canvas.SetPixel(px,py,clearColor);

									if (mirrorX)
									{
										canvas.SetPixel(imageSize-mirrorXOffset-px,py,clearColor);
									}

									if (mirrorY)
									{
										canvas.SetPixel(px,imageSize-1-py,clearColor);
									}

									if (mirrorX && mirrorY)
									{
										canvas.SetPixel(imageSize-mirrorXOffset-px,imageSize-1-py,clearColor);
									}

								}

								break;
						}
					}



					if (toolbarMode==2) // floodfill
					{
						switch (current.button)
						{
							case 0: // left mouse button
								Undo.RecordObject(canvas, "Floodfill " + px +","+py);
								floodFill(px,py,canvas.GetPixel(px,py),paintColor1);
								break;

							case 2: // middle mouse button
								paintColor1 = canvas.GetPixel(px,py);
								break;

							case 1: // right mouse button erase
								Undo.RecordObject(canvas, "Erase " + px +","+py);
								canvas.SetPixel(px,py,paintColor2);
								break;
						}
					}

					canvas.Apply(false);
					if (!wasModified) {}
					{
						if (!window) window = (PixelKit)EditorWindow.GetWindow (typeof (PixelKit));
						window.titleContent = new GUIContent(appName+"*");

						wasModified = true;
					}

					if (automaticOutline) DrawOutline();

					HandleUtility.Repaint();
				}
			}

			// show mouse cursor pixel preview
			if (mouseCursorPixel)
			{
				int x = (int)current.mousePosition.x-leftOffsetX;
				int y = (int)current.mousePosition.y-topOffsetY;
				int y2 = (int)current.mousePosition.y;

				if (x>=0 &&  x < imageSize*pixelSizeAdjust && y>=0 && y < imageSize*pixelSizeAdjust)
				{
					// get pixel coords
					mouseX = x/pixelSizeAdjust;
					mouseY = (imageSize*pixelSizeAdjust-y)/pixelSizeAdjust;

					if (x!=mouseCursorPixelPosX || y!=mouseCursorPixelPosY || paintColor1.ToString()!=oldpaintColor1.ToString())
					{
						HandleUtility.Repaint();
					}

					mouseCursorPixelPosX = x;
					mouseCursorPixelPosY = y;

					oldpaintColor1 = paintColor1;

					int gridX = (x)/pixelSizeAdjust*pixelSizeAdjust+leftOffsetX;
					int gridY = (y2/pixelSizeAdjust)*pixelSizeAdjust;
					GUI.DrawTextureWithTexCoords(new Rect(gridX-1,gridY-1,pixelSizeAdjust+2,pixelSizeAdjust+2), gridCellOutLine, new Rect(0f,0f,1f,1f),true);

				}else{
					// TODO: repaint only once..?
					HandleUtility.Repaint();
				}
			}

		} // GUIEvents()

		bool Compare4Neighbours(int sx,int sy)
		{
			// TODO: skip if outside border?
			if (canvas.GetPixel(sx,sy+1)==canvas.GetPixel(sx,sy-1))
			{
				if (canvas.GetPixel(sx+1,sy)==canvas.GetPixel(sx-1,sy))
				{
					if (canvas.GetPixel(sx+1,sy)==canvas.GetPixel(sx,sy-1))
					{
						return true;
					}
				}
			}

			// TODO: return best color match

			return false;
		}

		Color GetAverageNeighbourColor4(int sx,int sy)
		{
			Color c1 = canvas.GetPixel(sx+1,sy)*0.25f;
			Color c2 = canvas.GetPixel(sx-1,sy)*0.25f;
			Color c3 = canvas.GetPixel(sx,sy+1)*0.25f;
			Color c4 = canvas.GetPixel(sx,sy-1)*0.25f;
			Color averageColor = c1+c2+c3+c4;
			return averageColor;
		}

		void GUIMouseInfo()
		{
			Rect coordsPos = new Rect(imageSize*pixelSizeAdjust+leftOffsetX+12,topOffsetY,64,20);
			// TODO: use texture instead
			EditorGUI.ColorField(coordsPos,canvas.GetPixel(mouseX,mouseY));
			coordsPos.y += 24;
			GUI.Label(coordsPos,""+(mouseX<10?"  ":"")+mouseX+","+(mouseY<10?"  ":"")+mouseY, EditorStyles.boldLabel);
		}

		void GUIUndoRedo()
		{
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(GUILayout.Width(leftOffsetX-4));
			GUILayout.Space(0); // dummy
			GUILayout.EndVertical();

			if (GUILayout.Button(new GUIContent("<U", "Undo"),GUILayout.Width(32)))
			{
				//Undo.PerformUndo(); // TODO: fixme some error due to this?
			}

			if (GUILayout.Button(new GUIContent("R>", "Redo"),GUILayout.Width(32)))
			{
				//Undo.PerformRedo();
			}
			GUILayout.EndHorizontal();
		}

		void GUIInfoAndStatus()
		{
			int paletteButtonXpos = 110+10;
			int paletteButtonYpos = 665;
			Rect tempPos = new Rect(paletteButtonXpos,paletteButtonYpos,512,16);
			GUI.Label(tempPos,"B=Brush | F=Fill | D=Default color | X=Swap color | I=Invert color | 1,2,3=Quick colors", EditorStyles.miniLabel);
			tempPos.y+=15;
			GUI.Label(tempPos,"Ctrl+Z=Undo | Ctrl+Y=Redo | RightButton=Erase | MouseScroll=Darken/Lighten color", EditorStyles.miniLabel);
			tempPos.y+=15;
			GUI.Label(tempPos,"Alt+RightButton=Smart Erase | Alt+MouseWheel=Current color opacity", EditorStyles.miniLabel);
		}

		void GUICurrentColors()
		{
			GUILayout.Space(topOffsetY+imageSize*pixelSizeAdjust+14);
			GUILayout.BeginHorizontal();
			paintColor1 = EditorGUI.ColorField(new Rect(400,topOffsetY+imageSize*pixelSizeAdjust+10,60,24),paintColor1);
			paintColor2 = EditorGUI.ColorField(new Rect(406+62,topOffsetY+imageSize*pixelSizeAdjust+10,60,24),paintColor2);
			GUILayout.EndHorizontal();
		}

		void GUIMirrorToggles()
		{
			GUILayout.BeginHorizontal();
			mirrorX = GUILayout.Toggle(mirrorX, "MirrorX");
			mirrorXFix = GUILayout.Toggle(mirrorXFix, "FixX");
			mirrorXOffset = mirrorXFix?0:1;
			GUILayout.EndHorizontal();

			mirrorY = GUILayout.Toggle(mirrorY, "MirrorY");
		}

		void GUIAutomaticOutlines()
		{
			automaticOutline = GUILayout.Toggle(automaticOutline, "AutoBorder");
			//				automaticOutline = EditorGUILayout.BeginToggleGroup("AutoBorder",automaticOutline);
			//					automaticOutlineForBlack = EditorGUILayout.Toggle("AutoBorderForBlack",automaticOutlineForBlack);
			//					pos[1] = EditorGUILayout.Toggle("y", pos[1]);
			//					pos[2] = EditorGUILayout.Toggle("z", pos[2]);
			//				EditorGUILayout.EndToggleGroup();			
			///		automaticOutlineForBlack = GUILayout.Toggle(automaticOutlineForBlack, "AutoBorderForBlack");


		}

		void GUICurrentImageInfo()
		{
			if (selectedTexture)
			{
				EditorGUILayout.LabelField(selectedTexture.name+" : "+imageSize+"x"+imageSize, EditorStyles.miniLabel);
			}
		}

		void GUIPalette()
		{
			// TODO: fix this nest of magic numbers
			int paletteButtonWidth = 256+16;
			int paletteButtonHeight = 64+16;
			int paletteButtonXpos = 110+10;
			int paletteButtonYpos = 595-32-8;
			if (GUI.RepeatButton(new Rect(paletteButtonXpos,paletteButtonYpos,paletteButtonWidth,paletteButtonHeight),paletteTexture))
			{
				if (!paletteTexture) return;

				Vector2 pickpos = Event.current.mousePosition;

				int pickX = (int)pickpos.x - paletteButtonXpos-(paletteButtonWidth/2-paletteTexture.width/2);
				int pickY = (int)paletteTexture.height-((int)pickpos.y - paletteButtonYpos-(paletteButtonHeight/2-paletteTexture.height/2));
				if (pickX>=0 && pickX<=paletteTexture.width && pickY>=0 && pickY<=paletteTexture.height)
				{
					paintColor1 = paletteTexture.GetPixel(pickX,pickY);
				}
			}		
			// TODO: show selected position in palette, TODO: later new palette system
			paletteTexture = EditorGUI.ObjectField(new Rect(paletteButtonXpos+paletteButtonWidth+8,595-6,46,46),paletteTexture, typeof(Texture2D),false) as Texture2D;
		}

		void GUIGridSetup()
		{
			/*
			int paletteButtonXpos = 110+10;
			int paletteButtonYpos = 680-38;
			Rect tempPos = new Rect(paletteButtonXpos,paletteButtonYpos,48,16);

			gridColorBright = EditorGUI.ColorField(tempPos,gridColorBright);
			tempPos.x+=58;
			gridColorDark = EditorGUI.ColorField(tempPos,gridColorDark);
			tempPos.width = 32;
			tempPos.x+=58;
			if (GUI.Button(tempPos,"Set")) InitGridTexture(reInitTexture:false);
			tempPos.x+=38;
			if (GUI.Button(tempPos,"R")) 
			{
				gridColorDark = new Color(0.25f,0.25f,0.25f,0.5f);
				gridColorBright = new Color(0.4f,0.4f,0.4f,0.5f);
				InitGridTexture(reInitTexture:false);
			}*/

			EditorGUILayout.BeginVertical();
			GUILayout.Space(32);
			EditorGUILayout.LabelField("Grid colors");
			// TODO: preview?
			EditorGUILayout.BeginHorizontal(GUILayout.Width(32));
			gridColorBright = EditorGUILayout.ColorField(gridColorBright);
			gridColorDark = EditorGUILayout.ColorField(gridColorDark);
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Set",GUILayout.Width(88))) InitGridTexture(reInitTexture:false);
			if (GUILayout.Button("Reset", GUILayout.Width(88))) 
			{
				gridColorDark = new Color(0.25f,0.25f,0.25f,0.5f);
				gridColorBright = new Color(0.4f,0.4f,0.4f,0.5f);
				InitGridTexture(reInitTexture:false);
			}
			EditorGUILayout.EndVertical();
		}

		void GUIOutlineSetup()
		{
			GUILayout.Space(32);

			EditorGUILayout.BeginVertical();
			GUILayout.Space(32);
			EditorGUILayout.LabelField("Automatic Outline");
			outlineColor = EditorGUILayout.ColorField(outlineColor,GUILayout.Width(64));
			// TODO: other options for outline
			EditorGUILayout.EndVertical();

		}




		void GUILoadBuffer()
		{
			selectedTexture = EditorGUILayout.ObjectField(selectedTexture, typeof(Texture2D),false, GUILayout.Width(64), GUILayout.Height(64)) as Texture2D;
			if (GUILayout.Button(new GUIContent("Load", ""),GUILayout.Width(64))) LoadSelectedImage();
			if (GUILayout.Button(new GUIContent("SaveTo", ""),GUILayout.Width(64))) SaveToSelectedImage();
			if (selectedTexture) EditorGUILayout.LabelField(selectedTexture.width+"x"+selectedTexture.height, EditorStyles.miniLabel);
		}

		void GUIMovePanTools()
		{
			// TODO: move/pan/rot/flip
			GUILayout.BeginHorizontal();
			/*

			GUILayout.BeginVertical(GUILayout.Width(leftOffsetX-4));
			GUILayout.Space(0); // dummy
			GUILayout.EndVertical();*/

			if (GUILayout.Button(new GUIContent("<", ""),GUILayout.ExpandWidth(false)))	panImage(-1,0);
			if (GUILayout.Button(new GUIContent(">", ""),GUILayout.ExpandWidth(false)))	panImage(1,0);
			if (GUILayout.Button(new GUIContent("/\\", ""),GUILayout.ExpandWidth(false))) panImage(0,1);
			if (GUILayout.Button(new GUIContent("\\/", ""),GUILayout.ExpandWidth(false))) panImage(0,-1);


			//GUILayout.Space(8);
			GUILayout.EndHorizontal();
		}

		void GUIToolbar()
		{
			customGUIStyle = GUI.skin.GetStyle("minibutton");
			customGUIStyle.padding = new RectOffset(1,1,0,0);
			customGUIStyle.overflow = new RectOffset(0,0,2,4);
			customGUIStyle.fixedHeight = 18f;
			customGUIStyle.imagePosition = ImagePosition.ImageAbove;				
			toolbarMode = GUI.Toolbar(new Rect(0, 0, 640-2, 24), toolbarMode, toolbarStrings, customGUIStyle);

			switch (toolbarMode)
			{
				case 0: // new image
					CreateNewImage();
					toolbarMode = defaultToolBarMode;
					break;
				case 1: // brush
					toolbarMode = defaultToolBarMode;
					break;
				case 2: // fill mode
					break;
				case 3:	// save
					SavePNG();
					toolbarMode = defaultToolBarMode;
					break;
				case 4:	// save+ copy
					savedAsCopy=true;
					SavePNG();
					toolbarMode = defaultToolBarMode;
					break;
				case 5: // clear
					clearImage();
					toolbarMode = defaultToolBarMode;
					break;
				default:
					break;
			}

		}


		void GUIDrawPreviews()
		{
			// preview 1
			GUI.DrawTextureWithTexCoords(new Rect(imageSize*pixelSizeAdjust+24+leftOffsetX,topOffsetY+64,imageSize,imageSize), canvas, new Rect(0f,0f,1f,1f),true);

			// preview 2 (black bg)
			GUI.DrawTextureWithTexCoords(new Rect(imageSize*pixelSizeAdjust+24+leftOffsetX,topOffsetY+64+48,imageSize,imageSize), blackPixel, new Rect(0f,0f,1f,1f),true);
			GUI.DrawTextureWithTexCoords(new Rect(imageSize*pixelSizeAdjust+24+leftOffsetX,topOffsetY+64+48,imageSize,imageSize), canvas, new Rect(0f,0f,1f,1f),true);

			// preview 3 (white bg)
			GUI.DrawTextureWithTexCoords(new Rect(imageSize*pixelSizeAdjust+24+leftOffsetX,topOffsetY+64+48+48,imageSize,imageSize), whitePixel, new Rect(0f,0f,1f,1f),true);
			GUI.DrawTextureWithTexCoords(new Rect(imageSize*pixelSizeAdjust+24+leftOffsetX,topOffsetY+64+48+48,imageSize,imageSize), canvas, new Rect(0f,0f,1f,1f),true);

			// TODO: preview 4 (main camera bg color), take main cam bg color, create temp texture at init, adjust color if it changes..show here as bg
			//GUI.DrawTextureWithTexCoords(new Rect(size*zoom+24+leftOffsetX,topOffsetY + (size+16)*4,size,size), whitePixel, new Rect(0f,0f,1f,1f),true);
			//GUI.DrawTextureWithTexCoords(new Rect(size*zoom+24+leftOffsetX,topOffsetY + (size+16)*4,size,size), canvas, new Rect(0f,0f,1f,1f),true);
		}

		void CreateNewImage()
		{
			switch (EditorUtility.DisplayDialogComplex("UniPix : New Image","Select image size in pixels.\nWarning: image is cleared!","16x16","32x32","Cancel"))
			{
				case 0: // 16x16
					imageSize = 16;
					pixelSizeAdjust = 32;
					isInitialized = false;
					break;

				case 1: // 32x32
					imageSize = 32;
					pixelSizeAdjust = 16;
					isInitialized = false;
					break;

				case 2: // Cancel
					break;
				default:
					Debug.LogError(appName+"> Unrecognized option");
					break;
			}				
		}

		void GUISettingsTab()
		{
			GUIGridSetup();
			GUIOutlineSetup();
		}

		void InitializeCanvas()
		{
			// build preview backgrounds
			blackPixel = new Texture2D(1,1, TextureFormat.ARGB32, false);
			whitePixel = new Texture2D(1,1, TextureFormat.ARGB32, false);
			blackPixel.hideFlags = HideFlags.HideAndDontSave;
			whitePixel.hideFlags = HideFlags.HideAndDontSave;

			// mousecursor cell
			gridCellOutLine = new Texture2D(pixelSizeAdjust+2,pixelSizeAdjust+2, TextureFormat.ARGB32, false);
			gridCellOutLine.hideFlags = HideFlags.HideAndDontSave;
			gridCellOutLine.wrapMode = TextureWrapMode.Clamp;
			gridCellOutLine.filterMode = FilterMode.Point;

			CurrentCellOutlineRebuild();

			blackPixel.SetPixel(0,0,Color.black);
			whitePixel.SetPixel(0,0,Color.white);

			whitePixel.Apply(false);
			blackPixel.Apply(false);

			// build canvas texture
			canvas = new Texture2D(imageSize,imageSize, TextureFormat.ARGB32, false);
			canvas.wrapMode = TextureWrapMode.Clamp;
			canvas.filterMode = FilterMode.Point;
			canvas.hideFlags = HideFlags.HideAndDontSave;


			canvasOutlineBG = new Texture2D(imageSize,imageSize, TextureFormat.ARGB32, false);
			canvasOutlineBG.wrapMode = TextureWrapMode.Clamp;
			canvasOutlineBG.filterMode = FilterMode.Point;
			canvasOutlineBG.hideFlags = HideFlags.HideAndDontSave;


			canvasMouseCursor = new Texture2D(imageSize,imageSize, TextureFormat.ARGB32, false);
			canvasMouseCursor.wrapMode = TextureWrapMode.Clamp;
			canvasMouseCursor.filterMode = FilterMode.Point;
			canvasMouseCursor.hideFlags = HideFlags.HideAndDontSave;

			// draw canvas
			for(int x = 0; x < canvas.width; x++)
			{
				for(int y = 0; y < canvas.height; y++)
				{
					canvas.SetPixel(x,y,Color.clear);
					canvasOutlineBG.SetPixel(x,y,Color.clear);
					canvasMouseCursor.SetPixel(x,y,Color.clear);
				}
			}

			canvas.Apply(false);
			canvasOutlineBG.Apply(false);
			canvasMouseCursor.Apply(false);

			InitGridTexture(reInitTexture:true);

			HandleUtility.Repaint();
			isInitialized = true;   
		}

		void InitGridTexture(bool reInitTexture)
		{
			if (reInitTexture)
			{
				canvasBackground = new Texture2D(imageSize,imageSize, TextureFormat.ARGB32, false);
				canvasBackground.wrapMode = TextureWrapMode.Clamp;
				canvasBackground.filterMode = FilterMode.Point;
				canvasBackground.hideFlags = HideFlags.HideAndDontSave;
			}

			// draw grid
			for(int x = 0; x < canvas.width; x++)
			{
				for(int y = 0; y < canvas.height; y++)
				{
					canvasBackground.SetPixel(x,y,((x%2)==(y%2))?gridColorBright:gridColorDark);
				}
			}
			canvasBackground.Apply(false);

		}


		// window closed
		void OnDestroy()
		{
			// clean up time!
			DestroyImmediate(canvas);
			DestroyImmediate(canvasBackground);
			DestroyImmediate(canvasOutlineBG);
			DestroyImmediate(blackPixel);
			DestroyImmediate(whitePixel);
			selectedTexture = null;
		}

		// Temporary fix for waiting to save the file & then updating importer flags
		void Update()
		{
			if (!waitingFile)	return;

			fakeTimeCounter++;
			if (fakeTimeCounter > fakeNextTimeCounter) 
			{
				fakeTimeCounter=0;
				UpdateImporterFlags();
			}
		}


		void CurrentCellOutlineRebuild()
		{

			// rebuild outline image
			for(int x = 0; x < gridCellOutLine.width; x++)
			{
				for(int y = 0; y < gridCellOutLine.height; y++)
				{
					if (x==0 || y==0 || x==gridCellOutLine.width-1 || y==gridCellOutLine.height-1)
					{
						gridCellOutLine.SetPixel(x,y,paintColor1);
					}else{
						gridCellOutLine.SetPixel(x,y,Color.clear);
					}
				}
			}
			gridCellOutLine.Apply(false);
		}


		void DrawOutline()
		{
			// find canvas pixel image edges
			for(int x = 0; x < canvas.width; x++)
			{
				for(int y = 0; y < canvas.height; y++)
				{
					int centerPix = canvas.GetPixel(x,y).a>0?1:0;


					int upPix = canvas.GetPixel(x,y+1).a>0?1:0;
					int rightPix = canvas.GetPixel(x+1,y).a>0?1:0;
					int downPix = canvas.GetPixel(x,y-1).a>0?1:0;
					int leftPix = canvas.GetPixel(x-1,y).a>0?1:0;


					// decrease count if black color founded
					if (!automaticOutlineForBlack)
					{
						if (upPix>0) upPix -= canvas.GetPixel(x,y+1).grayscale==0?1:0;
						if (rightPix>0) rightPix -= canvas.GetPixel(x+1,y).grayscale==0?1:0;
						if (downPix>0) downPix -= canvas.GetPixel(x,y-1).grayscale==0?1:0;
						if (leftPix>0) leftPix -= canvas.GetPixel(x-1,y).grayscale==0?1:0;
					}

					int neighbourAlphas = upPix+rightPix+downPix+leftPix;
					if (neighbourAlphas>0)
					{
						if (centerPix==0)
						{
							canvasOutlineBG.SetPixel(x,y,outlineColor);
						}else{
							canvasOutlineBG.SetPixel(x,y,Color.clear);
						}
					}else{
						canvasOutlineBG.SetPixel(x,y,Color.clear);
					}
				}
			}
			canvasOutlineBG.Apply(false);
		}


		// scroll image canvas
		void panImage(int moveX,int moveY)
		{
			Texture2D tempCanvas = new Texture2D(imageSize,imageSize, TextureFormat.ARGB32, false);
			tempCanvas.hideFlags = HideFlags.HideAndDontSave;
			tempCanvas.SetPixels(canvas.GetPixels());
			tempCanvas.Apply(false);

			for(int x = 0; x < canvas.width; x++)
			{
				for(int y = 0; y < canvas.height; y++)
				{
					canvas.SetPixel((int)Mathf.Repeat(x+moveX,imageSize),(int)Mathf.Repeat(y+moveY,imageSize),tempCanvas.GetPixel(x,y));
				}
			}
			canvas.Apply(false);

			//if (automaticOutline) autoBorder();
			DestroyImmediate(tempCanvas);
		}

		// floodfill 4-dir, TODO: diagonal version also
		void floodFill(int x, int y, Color hitColor, Color fillColor)
		{
			// early exit if colors are the same)

			//Debug.Log(hitColor.ToString()+":"+fillColor.ToString());
			//Debug.Log(hitColor.ToString() == fillColor.ToString());
			// TODO: fix comparison?

			if (hitColor.ToString() == fillColor.ToString()) return;

			canvas.SetPixel(x,y,fillColor);

			List<int> ptsx = new List<int>();
			ptsx.Add(x);
			List<int> ptsy = new List<int>();
			ptsy.Add(y);

			int maxLoop=imageSize*imageSize+imageSize;
			while (ptsx.Count > 0 && maxLoop>0)
			{
				maxLoop--;

				if (ptsx[0]-1>=0)
				{
					if (canvas.GetPixel(ptsx[0]-1,ptsy[0]).ToString()==hitColor.ToString())
					{
						ptsx.Add(ptsx[0]-1); ptsy.Add(ptsy[0]);
						canvas.SetPixel(ptsx[0]-1,ptsy[0],fillColor);
					}
				}

				if (ptsy[0]-1>=0)
				{
					if (canvas.GetPixel(ptsx[0],ptsy[0]-1).ToString()==hitColor.ToString())
					{
						ptsx.Add(ptsx[0]);ptsy.Add(ptsy[0]-1);
						canvas.SetPixel(ptsx[0],ptsy[0]-1,fillColor);
					}
				}

				if (ptsx[0]+1<=imageSize)
				{
					if (canvas.GetPixel(ptsx[0]+1,ptsy[0]).ToString()==hitColor.ToString())
					{
						ptsx.Add(ptsx[0]+1); ptsy.Add(ptsy[0]);
						canvas.SetPixel(ptsx[0]+1,ptsy[0],fillColor);
					}
				}

				if (ptsy[0]+1<=imageSize)
				{
					if (canvas.GetPixel(ptsx[0],ptsy[0]+1).ToString()==hitColor.ToString())
					{
						ptsx.Add(ptsx[0]); ptsy.Add(ptsy[0]+1);
						canvas.SetPixel(ptsx[0],ptsy[0]+1,fillColor);
					}
				}
				ptsx.RemoveAt(0);
				ptsy.RemoveAt(0);
			}

			if (maxLoop<1) Debug.LogError(appName+" : floodFill overflow..");
		}


		// clear image	
		void clearImage()
		{
			Undo.RecordObject(canvas, "Clear image");
			for(int x = 0; x < canvas.width; x++)
			{
				for(int y = 0; y < canvas.height; y++)
				{
					canvas.SetPixel(x,y,Color.clear);
					canvasOutlineBG.SetPixel(x,y,Color.clear);
				}
			}
			canvas.Apply(false);
			canvasOutlineBG.Apply(false);
			//if (automaticOutline) autoBorder();			
		}


		// load selected texture as main image
		void LoadSelectedImage()
		{
			if (selectedTexture==null) return;
			Undo.RecordObject(canvas, "Load image");

			// check if its readable, if not set it temporarily readable
			bool setReadable = false;
			string path = AssetDatabase.GetAssetPath(selectedTexture);
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			if (textureImporter.isReadable == false)
			{
				textureImporter.isReadable = true;
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				setReadable = true;
			}

			// if same size, just load in
			if (selectedTexture.width == imageSize || selectedTexture.height == imageSize)
			{
				canvas.SetPixels(selectedTexture.GetPixels());

			}else{ // wrong size, TODO: ask to resize canvas OR image

				Debug.LogWarning(appName+": Scaling will happen. Because canvas size="+imageSize+"x"+imageSize+", Loading texture="+selectedTexture.width+"x"+selectedTexture.height);

				float xs=selectedTexture.width/(float)canvas.width;
				float ys=selectedTexture.height/(float)canvas.height;
				float sx=0;
				float sy=0;

				for(int x = 0; x < canvas.width; x++)
				{
					sy=0;
					for(int y = 0; y < canvas.height; y++)
					{
						canvas.SetPixel(x,y,selectedTexture.GetPixel((int)sx,(int)sy));
						sy+=ys;
					}
					sx+=xs;
				}
			}

			canvas.Apply(false);

			// restore isReadable setting, if we had changed it
			if (setReadable)
			{
				textureImporter.isReadable = false;
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				setReadable = false;
			}

			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			needsUpdate=true;

			if (automaticOutline) DrawOutline();
		}

		void SaveToSelectedImage()
		{
			if (selectedTexture==null) return;

			string path = AssetDatabase.GetAssetPath(selectedTexture);
			var bytes = canvas.EncodeToPNG();

			//Debug.Log(path);

			File.WriteAllBytes(path, bytes);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			wasModified = false;
			if (!window) window = (PixelKit)EditorWindow.GetWindow (typeof (PixelKit));
			window.titleContent=new GUIContent(appName);


		}


		// save png out
		void SavePNG()
		{
			if (textureSaveName.Length<1) textureSaveName = "NewPixelTexture";

			// merge borders to image
			if (automaticOutline)
			{
				for(int x = 0; x < canvas.width; x++)
				{
					for(int y = 0; y < canvas.height; y++)
					{
						if (canvas.GetPixel(x,y).a>0)
						{
							canvasOutlineBG.SetPixel(x,y,canvas.GetPixel(x,y));
						}
					}
				}

			}else{
				canvasOutlineBG.SetPixels(canvas.GetPixels());				
			}

			canvasOutlineBG.Apply(false);

			var bytes = canvasOutlineBG.EncodeToPNG();
			bool cancelled=false;

			if (savedAsCopy)
			{
				File.WriteAllBytes(Application.dataPath +"/" + (textureSaveName+nameCounter) +".png", bytes);
				Debug.Log(appName+"> image saved : "+(textureSaveName+nameCounter)+".png");
			}else{
				string saveTempStr = Application.dataPath +"/" + (textureSaveName) +".png";

				if (File.Exists(saveTempStr))
				{
					switch (EditorUtility.DisplayDialogComplex("UniPix","File already exists, what to do?\n ("+textureSaveName+".png)","&Overwrite","Auto&Rename","&Cancel"))
					{
						case 0: // overwrite
							break;

						case 1: // rename this
							saveTempStr = Application.dataPath +"/" + (textureSaveName+(System.DateTime.Now.ToString("MMddyyyyHHMMss"))) +".png";
							break;

						case 2: // Cancel
							cancelled = true;
							break;
						default:
							break;
					}				
				}


				if (!cancelled)
				{
					File.WriteAllBytes(saveTempStr, bytes);
					Debug.Log(appName+"> image saved : "+(textureSaveName)+".png");
				}
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			// TODO: if file already exists, then no need to adjust importer flags!
			waitingFile = true;
			wasModified = false;
			window.titleContent=new GUIContent(appName);
		}


		// update file importer flags
		void UpdateImporterFlags()
		{
			try
			{
				string texturePathName;
				if (savedAsCopy)
				{
					texturePathName = "Assets/"+(textureSaveName+nameCounter)+".png";
				}else{
					texturePathName = "Assets/"+(textureSaveName)+".png";
				}

				// modify the importer settings
				TextureImporter textureImporter = AssetImporter.GetAtPath(texturePathName) as TextureImporter;
				textureImporter.wrapMode = TextureWrapMode.Clamp;
				textureImporter.filterMode = FilterMode.Point;
				textureImporter.maxTextureSize = (int)Mathf.Clamp(imageSize,32,4096);
				textureImporter.mipmapEnabled = false; // FIXME: doesnt work?
				textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				AssetDatabase.ImportAsset(texturePathName, ImportAssetOptions.ForceUpdate );

				waitingFile=false;
				needsUpdate=true;

				// saved file as copy, increase counter
				if (savedAsCopy)
				{
					savedAsCopy=false;
					nameCounter++;
					// TODO: check if exists, then increase counter?
				}

			}catch{

			}
		}
	} // class
} // namespace
