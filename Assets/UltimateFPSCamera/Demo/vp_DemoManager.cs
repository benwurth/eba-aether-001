/////////////////////////////////////////////////////////////////////////////////
//
//	vp_DemoManager.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	system for performing a walkthrough demo with multiple screens,
//					navigation buttons and selection menus
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public class vp_DemoManager
{

	// styles, used mainly for aligning the various texts
	public GUIStyle UpStyle = null;
	public GUIStyle LabelStyle = null;
	public GUIStyle DownStyle = null;
	public GUIStyle CenterStyle = null;

	// GUI & demo states, variables
	public int CurrentScreen = 1;						// current demo screen
	public Resolution DesktopResolution;				// initial desktop screen resolution
	public bool FirstFrame = true;						// whether we're in the first frame of a new demo screen
	public bool EditorPreviewSectionExpanded = true;	// some screens have a bottom left editor preview screenshot section
	public bool ShowGUI = true;							// if false, disables gui rendering
	public float ButtonColumnClickTime = 0.0f;			// time when a button was last pressed in a button column
	public float ButtonColumnArrowY = -100.0f;			// y position of arrow in a button column
	public float ButtonColumnArrowFadeoutTime = 0.0f;	// varying fadeout time of the arrow in a button column
	public int ButtonSelection = 0;						// last pressed button in the various demo screens
	public float LastInputTime = 0.0f;					// used for detecting how long the user has idled
	private float m_FadeSpeed = 0.03f;					// used as speed for all transitions
	private int m_FadeToScreen = 0;						// demo screen that we are currently transitioning to
	private bool m_StylesInitialized = false;			// for styles to only be set up once
	enum FadeState										// screen transition state
	{
		None,
		FadeOut,
		FadeIn
	}
	FadeState m_FadeState = FadeState.None;

	// alpha state
	public float GlobalAlpha = 0.35f;					// override alpha of all gui controls
	private float m_TextAlpha = 1.0f;					// current opacity of the demo text. may be fading out etc.
	private float m_EditorPreviewScreenshotTextAlpha = 0.0f;	// current opacity of the preview section
	private float m_FullScreenTextAlpha = 1.0f;			// current opacity of optional start fullscreen message
	private float m_BigArrowFadeAlpha = 1.0f;			// current opacity of the fading 'next screen' button

	// pointer function for the last demo screen (checkmark button)
	public delegate void EndCallback();

	// this variable can be used to test framerate indepencence for
	// various features. NOTE: will only work in the editor.
	bool m_SimulateLowFPS = false;
	


	///////////////////////////////////////////////////////////
	// constructor
	///////////////////////////////////////////////////////////
	public vp_DemoManager()
	{

		DesktopResolution = Screen.currentResolution;
		
		LastInputTime = Time.time;

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	public virtual void Update()
	{

		// toggle gui on 'G'
		if (Input.GetKeyDown(KeyCode.G))
			ShowGUI = !ShowGUI;

		// toggle fullscreen on 'F'
		if (Input.GetKeyDown(KeyCode.F))
			Screen.fullScreen = !Screen.fullScreen;

		// make fullscreen use desktop resolution
		if (Screen.fullScreen == true && Screen.currentResolution.width != DesktopResolution.width)
			Screen.SetResolution(DesktopResolution.width, DesktopResolution.height, true);

		// toggle low framerate simulation on 'L'
		if (Input.GetKeyDown(KeyCode.L))
			m_SimulateLowFPS = !m_SimulateLowFPS;
		if (m_SimulateLowFPS)
		{
			for (int v = 0; v < 20000000; v++) { }
		}

		// end application on 'ESC' (ignored in editor & web player)
		if (Input.GetKey(KeyCode.Escape)) { Application.Quit(); }

	}


	///////////////////////////////////////////////////////////
	// draws a 'button toggle', a compound control for a basic
	// on / off toggle
	///////////////////////////////////////////////////////////
	public bool ButtonToggle(Rect rect, string label, bool state, bool arrow, Texture imageUpPointer)
	{

		if(!ShowGUI)
			return false;

		GUIStyle onStyle = UpStyle;
		GUIStyle offStyle = DownStyle;

		// if state is true, button is 'ON'
		float arrowOffset = 0.0f;
		if (state)
		{
			onStyle = DownStyle;
			offStyle = UpStyle;
			arrowOffset = (rect.width * 0.5f) + 2;
		}

		// draw toggle label
		GUI.Label(new Rect(rect.x, rect.y - 30, rect.width, rect.height), label, CenterStyle);
		
		// draw buttons
		if (GUI.Button(new Rect(rect.x, rect.y, (rect.width * 0.5f)-2, rect.height), "OFF", offStyle))
			state = false;
		if (GUI.Button(new Rect(rect.x + (rect.width * 0.5f) + 2, rect.y,
											(rect.width * 0.5f), rect.height), "ON", onStyle))
			state = true;

		// if button has an arrow, draw it
		if(arrow)
			GUI.Label(new Rect(rect.x + ((rect.width * 0.5f) * 0.5f) - 14 + arrowOffset,
											rect.y + rect.height, 32, 32), imageUpPointer);
		
		return state;

	}


	///////////////////////////////////////////////////////////
	// this method draws three big boxes at the top of the screen;
	// next & prev arrows, and a main text box
	///////////////////////////////////////////////////////////
	public void DrawBoxes(string caption, string description, Texture imageLeftArrow, Texture imageRightArrow, EndCallback endCallback = null)
	{

		if(!ShowGUI)
			return;

		GUI.color = new Color(1, 1, 1, 1 * GlobalAlpha);
		float screenCenter = Screen.width / 2;

		GUILayout.BeginArea(new Rect(screenCenter - 400, 30, 800, 100));

		// draw box for 'prev' button, if texture is provided
		if (imageLeftArrow != null)
			GUI.Box(new Rect(30, 10, 80, 80), "");

		// draw big text background box
		GUI.Box(new Rect(120, 0, 560, 100), "");

		GUI.color = new Color(1, 1, 1, m_TextAlpha);

		// draw text
		for (int v = 0; v < 3; v++)
		{
			GUILayout.BeginArea(new Rect(130, 10, 540, 80));
			GUILayout.Label("--- " + caption.ToUpper() + " ---" + "\n" + description, LabelStyle);
			GUILayout.EndArea();
		}
		GUI.color = new Color(1, 1, 1, 1 * GlobalAlpha);

		// draw box for 'next' button, if texture is provided
		if (imageRightArrow != null)
			GUI.Box(new Rect(690, 10, 80, 80), "");

		// draw 'prev' arrow button, if texture is provided
		if (imageLeftArrow != null)
		{
			if (GUI.Button(new Rect(35, 15, 80, 80), imageLeftArrow, "Label"))
			{
				m_FadeToScreen = Mathf.Max(CurrentScreen - 1, 1);
				m_FadeState = FadeState.FadeOut;
			}
		}

		// handle arrow fade state
		if (Time.time < LastInputTime + 30)
			m_BigArrowFadeAlpha = 1;
		else
			m_BigArrowFadeAlpha = 0.5f - Mathf.Sin((Time.time - 0.5f) * 6) * 1.0f;
		GUI.color = new Color(1, 1, 1, m_BigArrowFadeAlpha * GlobalAlpha);

		// draw 'next' arrow button, if texture is provided, unless
		// a checkmark texture is provided ...
		if (imageRightArrow != null)
		{
			if (GUI.Button(new Rect(700, 15, 80, 80), imageRightArrow, "Label"))
			{
				if(endCallback == null)
				{
					m_FadeToScreen = CurrentScreen + 1;
					m_FadeState = FadeState.FadeOut;
				}
				else
					endCallback();
			}
		}

		GUI.color = new Color(1, 1, 1, 1 * GlobalAlpha);
		GUILayout.EndArea();
		GUI.color = new Color(1, 1, 1, m_TextAlpha * GlobalAlpha);

	}


	///////////////////////////////////////////////////////////
	// draws a 'toggle column', a compound control displaying a
	// column of buttons that can be toggled like radio buttons
	///////////////////////////////////////////////////////////
	public int ToggleColumn(int width, int y, int sel, string[] strings, bool center, bool arrow, Texture imageRightPointer, Texture imageLeftPointer)
	{

		if(!ShowGUI)
			return 0;

		float height = (strings.Length * 30);

		Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

		// initial alignment
		Rect rect;
		if(center)
			rect = new Rect(screenCenter.x - width, y, width, 30);
		else
			rect = new Rect(Screen.width - width - 10, screenCenter.y - (height / 2), width, 30);

		// draw one button for each string
		int v = 0;
		foreach (string s in strings)
		{

			// individual button alignment
			if (center)
				rect.x = screenCenter.x - (width / 2);
			else
				rect.x = 10;
			rect.width = width;

			// set default style
			GUIStyle style = UpStyle;
			
			// pressed buttons
			if (v == sel)
			{

				Color col = GUI.color;
				GUI.color = new Color(1, 1, 1, 1);

				// set pressed style
				style = DownStyle;

				// make button rect appear pressed
				if (center)
					rect.x = screenCenter.x - (width / 2) + 10;
				else
					rect.x = 20;
				rect.width = width - 20;

				// draw arrow if applicable
				if (arrow)
				{
					if (center)
						GUI.Label(new Rect(rect.x - 27, rect.y, 32, 32), imageRightPointer);
					else
						GUI.Label(new Rect(rect.x + rect.width + 5, rect.y, 32, 32), imageLeftPointer);
				}

				GUI.color = col;

			}
			
			// draw the button and detect selection
			if (GUI.Button(rect, s, style))
				sel = v;
			
			rect.y += 35;
			v++;

		}

		return sel;

	}

	
	///////////////////////////////////////////////////////////
	// draws a 'button column' compound control, with a selection
	// arrow that fades out after a set amount of time.
	// presently used only in the 'EXTERNAL FORCES' demo
	///////////////////////////////////////////////////////////
	public int ButtonColumn(int y, int sel, string[] strings, Texture imagePointer)
	{

		if(!ShowGUI)
			return 0;

		float screenCenter = Screen.width / 2;
		Rect rect = new Rect(screenCenter - 100, y, 200, 30);

		// draw one button for each string
		int v = 0;
		foreach (string s in strings)
		{

			rect.x = screenCenter - 100;
			rect.width = 200;

			// draw button and detect click
			if (GUI.Button(rect, s))
			{
				sel = v;
				ButtonColumnClickTime = Time.time;
				ButtonColumnArrowY = rect.y;
			}
			
			rect.y += 35;
			v++;
		}

		// fade out the arrow after a set amount of time
		if (Time.time < ButtonColumnArrowFadeoutTime)
			ButtonColumnClickTime = Time.time;

		// draw the arrow
		GUI.color = new Color(1, 1, 1, Mathf.Max(0, 1 - (Time.time - ButtonColumnClickTime) * 1.0f * GlobalAlpha));
		GUI.Label(new Rect(rect.x - 27, ButtonColumnArrowY, 32, 32), imagePointer);
		GUI.color = new Color(1, 1, 1, 1 * GlobalAlpha);

		return sel;

	}

	
	///////////////////////////////////////////////////////////
	// resets various settings. used when transitioning between
	// two demo screens
	///////////////////////////////////////////////////////////
	protected virtual void Reset()
	{
		ButtonSelection = 0;
		FirstFrame = true;
		LastInputTime = Time.time;
	}
		

	///////////////////////////////////////////////////////////
	// sets up any special gui styles
	///////////////////////////////////////////////////////////
	private void InitGUIStyles()
	{

		LabelStyle = new GUIStyle("Label");
		LabelStyle.alignment = TextAnchor.LowerCenter;

		UpStyle = new GUIStyle("Button");

		DownStyle = new GUIStyle("Button");
		DownStyle.normal = DownStyle.active;

		CenterStyle = new GUIStyle("Label");
		CenterStyle.alignment = TextAnchor.MiddleCenter;

		m_StylesInitialized = true;

	}


	///////////////////////////////////////////////////////////
	// helper method to draw an image relative to the center
	// of the screen
	///////////////////////////////////////////////////////////
	public void DrawImage(Texture image, float xOffset, float yOffset)
	{

		if(!ShowGUI)
			return;

		if (image == null)
			return;

		float screenCenter = Screen.width / 2;
		float width = Mathf.Min(image.width, Screen.width);
		float aspect = (float)image.height / (float)image.width;
		GUI.DrawTexture(new Rect(screenCenter - (width / 2) + xOffset, 140 + yOffset, width, width * aspect), image);

	}

	public void DrawImage(Texture image)
	{
		DrawImage(image, 0, 0);
	}



	///////////////////////////////////////////////////////////
	// draws a screenshot preview of the corresponding editor section
	// in the bottom left corner. the screenshot can be expanded and
	// collapsed, and fades in a 'screenshot' label if the mouse is
	// held over it.
	///////////////////////////////////////////////////////////
	public void DrawEditorPreview(Texture section, Texture imageEditorPreview, Texture imageEditorScreenshot)
	{

		if(!ShowGUI)
			return;

		Texture caption;
		Color col = GUI.color;
		Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		float xPos = 0;

		// handle expanded preview section
		if (EditorPreviewSectionExpanded)
		{

			// draw image in lower left corner
			caption = imageEditorPreview;
			float captionY = Screen.height - section.height - caption.height;
			float sectionY = Screen.height - section.height;

			GUI.DrawTexture(new Rect(xPos, captionY, caption.width, caption.height), caption);
			GUI.DrawTexture(new Rect(xPos, sectionY, section.width, section.height), section);

			// if mouse is over the preview section, fade in 'screenshot'
			// text and test for click.
			if ((mousePos.x > xPos) && (mousePos.x < xPos + section.width) &&
										(mousePos.y > captionY) &&
										(mousePos.y < Screen.height - caption.height))
			{
				m_EditorPreviewScreenshotTextAlpha = Mathf.Min(1.0f, m_EditorPreviewScreenshotTextAlpha + 0.01f);
				// if user presses left mouse button, collapse the preview section
				if (Input.GetMouseButtonDown(0))
					EditorPreviewSectionExpanded = false;
			}
			else
				m_EditorPreviewScreenshotTextAlpha = Mathf.Max(0.0f, m_EditorPreviewScreenshotTextAlpha - 0.03f);
			GUI.color = new Color(1, 1, 1, (col.a * 0.5f) * m_EditorPreviewScreenshotTextAlpha);
			GUI.DrawTexture(new Rect(xPos + 48, sectionY + (section.height / 2) - (imageEditorScreenshot.height / 2),
							imageEditorScreenshot.width, imageEditorScreenshot.height), imageEditorScreenshot);
		}

		// handle collapsed preview section
		else
		{
			// draw image in lower left corner
			caption = imageEditorPreview;
			float captionY = Screen.height - caption.height;
			GUI.DrawTexture(new Rect(xPos, captionY, caption.width, caption.height), caption);

			// if mouse is over the preview section, test for click
			if ((mousePos.x > xPos) && (mousePos.x < xPos + section.width) && (mousePos.y > captionY))
			{
				// if user presses left mouse button, expand the preview section
				if (Input.GetMouseButtonUp(0))
					EditorPreviewSectionExpanded = true;
			}

		}

		GUI.color = col;

	}


	///////////////////////////////////////////////////////////
	// draws a fullscreen info text for 3 seconds, used right
	// after application start
	///////////////////////////////////////////////////////////
	public void DrawFullScreenText(Texture imageFullScreen)
	{
		
		if(!ShowGUI)
			return;

		if ((Time.realtimeSinceStartup > 5.0f))
			return;

		if (Time.realtimeSinceStartup > 3.0f)
			m_FullScreenTextAlpha -= m_FadeSpeed * Time.deltaTime * 15.0f;

		GUI.color = new Color(1, 1, 1, m_FullScreenTextAlpha * GlobalAlpha);
		GUI.DrawTexture(new Rect((Screen.width / 2) - 120, (Screen.height / 2) - 16, 240, 32), imageFullScreen);
		GUI.color = new Color(1, 1, 1, 1 * GlobalAlpha);

	}


	///////////////////////////////////////////////////////////
	// handles logic to fade between demo screens
	///////////////////////////////////////////////////////////
	public void DoScreenTransition()
	{

		if(!ShowGUI)
			return;

		// fadeout stage
		if (m_FadeState == FadeState.FadeOut)
		{
			// decrement text alpha
			m_TextAlpha -= m_FadeSpeed;
			if (m_TextAlpha <= 0.0f)
			{
				// reached zero alpha, time to switch screens
				m_TextAlpha = 0.0f;					// cap alpha at zero
				Reset();							// reset all standard values
				CurrentScreen = m_FadeToScreen;		// switch screens
				m_FadeState = FadeState.FadeIn;		// move to fadein stage
			}
		}

		// fadein stage
		else if (m_FadeState == FadeState.FadeIn)
		{
			// increment text alpha
			m_TextAlpha += m_FadeSpeed;
			if (m_TextAlpha >= 1.0f)
			{
				// reached full alpha, time to disable transition
				m_TextAlpha = 1.0f;					// cap alpha at 1
				m_FadeState = FadeState.None;		// disable transition
			}
		}

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	public void OnGUI()
	{

		// set up gui styles, but only once
		if (!m_StylesInitialized)
			InitGUIStyles();
		
		// handle fading between demo screens
		DoScreenTransition();

		// if the mouse cursor is locked, partly fade out the gui, otherwise show it fully
		if (Screen.lockCursor)
			GlobalAlpha = 0.35f;
		else
			GlobalAlpha = 1.0f;

	}


}

