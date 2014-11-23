/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemo.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	PLEASE NOTE: this class is very specialized	for the demo
//					walkthrough and is not meant to be used as the starting
//					point for a game, nor an example of best workflow practices.
//					for an example of how to use the Ultimate FPS Camera system
//					in a game development context, please see 'vp_FPSPlayer.cs'.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public class vp_FPSDemo : MonoBehaviour
{

	vp_FPSDemoManager Demo = null;

	// demo states
	private int m_ExamplesCurrentSel = 0;			// selection for the 'EXAMPLES' screen
	private TextAsset m_LastLoadedCamera = null;	// used for some demo weapon transition special cases

	// player
	public GameObject PlayerGameObject = null;
	private vp_FPSPlayer m_Player = null;

	// timers
	private vp_Timer m_ChrashingAirplaneRestoreTimer = null;

	// GUI
	private bool m_WeaponLayerToggle = false;		// state of the weapon layer toggle button
		
	// positions
	private Vector3 m_MouseLookPos = new Vector3(-8.093015f, 20.08f, 3.416737f);
	private Vector3 m_OverviewPos = new Vector3(1.246535f, 32.08f, 21.43753f);
	private Vector3 m_StartPos = new Vector3(-18.14881f, 20.08f, -24.16859f);
	private Vector3 m_WeaponLayerPos = new Vector3(-19.43989f, 16.08f, 2.10474f);
	private Vector3 m_ForcesPos = new Vector3(-8.093015f, 20.08f, 3.416737f);
	private Vector3 m_MechPos = new Vector3(0.02941191f, 1.08f, -93.50691f);
	private Vector3 m_DrunkPos = new Vector3(18.48685f, 21.08f, 24.05441f);
	private Vector3 m_SniperPos = new Vector3(0.8841875f, 33.08f, 21.3446f);
	private Vector3 m_OldSchoolPos = new Vector3(25.88745f, 0.08f, 23.08822f);
	private Vector3 m_AstronautPos = new Vector3(20.0f, 20.0f, 16.0f);
	protected Vector3 m_UnFreezePosition = Vector3.zero;

	// angles
	private Vector2 m_MouseLookAngle = new Vector2(33.10683f, 0);
	private Vector2 m_OverviewAngle = new Vector2(224, 28.89369f);
	private Vector2 m_PerspectiveAngle = new Vector2(223, 27);
	private Vector2 m_StartAngle = new Vector2(0, 0);
	private Vector2 m_WeaponLayerAngle = new Vector2(-90, 0);
	private Vector2 m_ForcesAngle = new Vector2(33.10683f, -167);
	private Vector2 m_MechAngle = new Vector3(180, 0);
	private Vector2 m_DrunkAngle = new Vector3(-90, 0);
	private Vector2 m_SniperAngle = new Vector2(180, 20);
	private Vector2 m_OldSchoolAngle = new Vector2(180, 0);
	private Vector2 m_AstronautAngle = new Vector2(269.5f, 0);
	
	// textures
	public Texture m_ImageEditorPreview = null;
	public Texture m_ImageEditorPreviewShow = null;
	public Texture m_ImageCameraMouse = null;
	public Texture m_ImageWeaponPosition = null;
	public Texture m_ImageWeaponPerspective = null;
	public Texture m_ImageWeaponPivot = null;
	public Texture m_ImageEditorScreenshot = null;
	public Texture m_ImageLeftArrow = null;
	public Texture m_ImageRightArrow = null;
	public Texture m_ImageCheckmark = null;
	public Texture m_ImageLeftPointer = null;
	public Texture m_ImageRightPointer = null;
	public Texture m_ImageUpPointer = null;
	public Texture m_ImageCrosshair = null;
	public Texture m_ImageFullScreen = null;

	// sounds
	AudioSource m_AudioSource = null;
	public AudioClip m_StompSound = null;
	public AudioClip m_EarthquakeSound = null;
	public AudioClip m_ExplosionSound = null;

	// presets
	public TextAsset ArtilleryCamera = null;
	public TextAsset ArtilleryController = null;
	public TextAsset AstronautCamera = null;
	public TextAsset AstronautController = null;
	public TextAsset CowboyCamera = null;
	public TextAsset CowboyController = null;
	public TextAsset CowboyWeapon = null;
	public TextAsset CowboyShooter = null;
	public TextAsset CrouchController = null;
	public TextAsset DefaultCamera = null;
	public TextAsset DefaultController = null;
	public TextAsset DefaultWeapon = null;
	public TextAsset DrunkCamera = null;
	public TextAsset DrunkController = null;
	public TextAsset ImmobileCamera = null;
	public TextAsset ImmobileController = null;
	public TextAsset MaceCamera = null;
	public TextAsset MaceWeapon = null;
	public TextAsset MafiaCamera = null;
	public TextAsset MafiaWeapon = null;
	public TextAsset MafiaShooter = null;
	public TextAsset MechCamera = null;
	public TextAsset MechController = null;
	public TextAsset MechWeapon = null;
	public TextAsset MechShooter = null;
	public TextAsset ModernCamera = null;
	public TextAsset ModernController = null;
	public TextAsset ModernWeapon = null;
	public TextAsset ModernShooter = null;
	public TextAsset MouseLowSensCamera = null;
	public TextAsset MouseRawUnityCamera = null;
	public TextAsset MouseSmoothingCamera = null;
	public TextAsset OldSchoolCamera = null;
	public TextAsset OldSchoolController = null;
	public TextAsset OldSchoolWeapon = null;
	public TextAsset OldSchoolShooter = null;
	public TextAsset Persp1999Camera = null;
	public TextAsset Persp1999Weapon = null;
	public TextAsset PerspModernCamera = null;
	public TextAsset PerspModernWeapon = null;
	public TextAsset PerspOldCamera = null;
	public TextAsset PerspOldWeapon = null;
	public TextAsset PivotChestWeapon = null;
	public TextAsset PivotElbowWeapon = null;
	public TextAsset PivotMuzzleWeapon = null;
	public TextAsset PivotWristWeapon = null;
	public TextAsset SmackController = null;
	public TextAsset SniperCamera = null;
	public TextAsset SniperWeapon = null;
	public TextAsset SniperShooter = null;
	public TextAsset StompingCamera = null;
	public TextAsset SystemOFFCamera = null;
	public TextAsset SystemOFFController = null;
	public TextAsset SystemOFFShooter = null;
	public TextAsset SystemOFFWeapon = null;
	public TextAsset SystemOFFWeaponGlideIn = null;
	public TextAsset TurretCamera = null;
	public TextAsset TurretWeapon = null;
	public TextAsset TurretShooter = null;
	public TextAsset WallFacingCamera = null;
	public TextAsset WallFacingWeapon = null;


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Start()
	{

		m_Player = (vp_FPSPlayer)PlayerGameObject.GetComponent(typeof(vp_FPSPlayer));
		Demo = new vp_FPSDemoManager(m_Player);
		m_Player.LockCursor = false;

		m_Player.Camera.RenderingFieldOfView = 20;
		m_Player.Camera.SnapZoom();
		m_Player.Camera.PositionOffset = new Vector3(0, 1.75f, 0.1f);

		// add an audio source to the camera, for playing various demo sounds
		m_AudioSource = (AudioSource)m_Player.Camera.gameObject.AddComponent("AudioSource");

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Update()
	{

		Demo.Update();

		// special case to make sure the weapon doesn't flicker into
		// view briefly in the first frame
		if (Demo.CurrentScreen == 1 && m_Player.Camera.CurrentWeaponID != 0)
			m_Player.Camera.SetWeapon(0);

		// input special cases for the 'EXAMPLES' screen
		if (Demo.CurrentScreen == 2)
		{
			if ((Input.mousePosition.x < 150) ||
				((Input.mousePosition.y > (Screen.height - 130))) &&
				(Input.mousePosition.x < ((Screen.width * 0.5f) - 290) ||
				Input.mousePosition.x > ((Screen.width * 0.5f) + 290)
				))
			{
				m_Player.LockCursor = false;
			}
			else
			{
				if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
				{
					m_Player.LockCursor = true;
				}
			}

			// switch weapon examples using the 1-0 number keys
			if (Input.GetKeyDown(KeyCode.Backspace))
				Demo.ButtonSelection = 0;
			if (Input.GetKeyDown(KeyCode.Alpha1))
				Demo.ButtonSelection = 1;
			if (Input.GetKeyDown(KeyCode.Alpha2))
				Demo.ButtonSelection = 2;
			if (Input.GetKeyDown(KeyCode.Alpha3))
				Demo.ButtonSelection = 3;
			if (Input.GetKeyDown(KeyCode.Alpha4))
				Demo.ButtonSelection = 4;
			if (Input.GetKeyDown(KeyCode.Alpha5))
				Demo.ButtonSelection = 5;
			if (Input.GetKeyDown(KeyCode.Alpha6))
				Demo.ButtonSelection = 6;
			if (Input.GetKeyDown(KeyCode.Alpha7))
				Demo.ButtonSelection = 7;
			if (Input.GetKeyDown(KeyCode.Alpha8))
				Demo.ButtonSelection = 8;
			if (Input.GetKeyDown(KeyCode.Alpha9))
				Demo.ButtonSelection = 9;
			if (Input.GetKeyDown(KeyCode.Alpha0))
				Demo.ButtonSelection = 10;

			// cycle to previous example
			if (Input.GetKeyDown(KeyCode.Q))
			{
				Demo.ButtonSelection--;
				if (Demo.ButtonSelection < 1)
					Demo.ButtonSelection = 10;
			}

			// cycle to next example
			if (Input.GetKeyDown(KeyCode.E))
			{
				Demo.ButtonSelection++;
				if (Demo.ButtonSelection > 10)
					Demo.ButtonSelection = 1;
			}

			// if user presses 'ENTER' toggle the mouse pointer
			if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
				m_Player.LockCursor = !m_Player.LockCursor;

		}

		// special case to cancel the crashing airplane example zoom reset
		// if user navigates away from the 'EXTERNAL FORCES' screen
		if (Demo.CurrentScreen != 3 && m_ChrashingAirplaneRestoreTimer != null)
			m_ChrashingAirplaneRestoreTimer.Cancel();

	}


	///////////////////////////////////////////////////////////
	// demo screen to show a welcoming message
	///////////////////////////////////////////////////////////
	private void DemoIntro()
	{

		// draw the three big boxes at the top of the screen;
		// the next & prev arrows, and the main text box
		Demo.DrawBoxes("welcome", "Ultimate FPS Camera is a next-gen first person camera system with ultra smooth PROCEDURAL ANIMATION of player movements. Camera and weapons are manipulated using over 100 parameters, allowing for a vast range of super-lifelike behaviors.", null, m_ImageRightArrow);

		// this bracket is only run the first frame of a new demo screen
		// being rendered. it is used to make some initial settings
		// specific to the current demo screen.
		if (Demo.FirstFrame)
		{
			Demo.DrawCrosshair = false;
			Demo.FirstFrame = false;
			m_Player.Camera.RenderingFieldOfView = 20;
			m_Player.Camera.SnapZoom();
			m_Player.Camera.SetWeapon(0);
			Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle, true);			// prevent player from moving
			Demo.LastInputTime -= 20;								// makes the big arrow start fading 20 seconds earlier on this screen
			Demo.RefreshDefaultState();
		}

		Demo.ForceCameraShake();

	}


	///////////////////////////////////////////////////////////
	// demo screen to show some example presets that are possible
	// with Ultimate FPS Camera
	///////////////////////////////////////////////////////////
	private void DemoExamples()
	{

		Demo.DrawBoxes("examples", "Try MOVING, JUMPING and STRAFING with the demo presets on the left.\nNote that NO ANIMATIONS are used in this demo. Instead, the camera and weapons are manipulated using realtime SPRING PHYSICS, SINUS BOB and NOISE SHAKING.\nCombining this with traditional animations (e.g. reload) can be very powerful!", m_ImageLeftArrow, m_ImageRightArrow);
		if (Demo.FirstFrame)
		{
			Demo.DrawCrosshair = true;
			m_Player.Camera.SetWeapon(0);
			LoadCamera(SystemOFFCamera);
			m_Player.Controller.Load(SystemOFFController);
			Demo.Teleport(m_StartPos, m_StartAngle);						// place the player at start point
			Demo.FirstFrame = false;
			m_UnFreezePosition = m_Player.Controller.transform.position;	// makes player return to start pos when unfreezed (from sniper, turret presets etc.)
			Demo.ButtonSelection = 0;
			m_Player.SetWeaponAvailable(1, false);
			m_Player.SetWeaponAvailable(2, false);
			m_Player.SetWeaponAvailable(3, false);
			m_Player.SetWeaponAvailable(4, false);
			m_Player.SetWeaponAvailable(5, false);
			SwitchWeapon(3, SystemOFFWeaponGlideIn, SystemOFFShooter, 0, false, false);
			m_Player.Camera.SnapSprings();
			Demo.RefreshDefaultState();
		}

		// if selected button in toggle column has changed, change
		// the current preset
		if (Demo.ButtonSelection != m_ExamplesCurrentSel)
		{

			m_Player.ResetState();

			m_Player.LockCursor = true;

			m_Player.Camera.SnapSprings();
			m_Player.Camera.StopEarthQuake();
			if (m_Player.Camera.CurrentWeapon != null)
				m_Player.Camera.SnapSprings();

			m_Player.Camera.BobStepCallback = null;

			switch (Demo.ButtonSelection)
			{
				case 0:	// --- System OFF ---
					Demo.DrawCrosshair = true;
					m_Player.Controller.Stop();
					m_Player.Controller.Load(SystemOFFController);
					if (m_Player.Camera.CurrentWeaponID == 5)	// mech cockpit is not allowed in 'system off' mode
					{
						LoadCamera(SystemOFFCamera);	// so replace it by pistol
						SwitchWeapon(1, SystemOFFWeapon, SystemOFFShooter, 0, false, false);
					}
					else
					{
						LoadCamera(SystemOFFCamera);
						Demo.SetWeaponPreset(SystemOFFWeapon);
						m_Player.AllowState("Zoom", false);
						m_Player.AllowState("Crouch", false);
					}
					Demo.RefreshDefaultState();
					break;
				case 1:	// --- Mafia Boss ---
					Demo.DrawCrosshair = true;
					if (m_LastLoadedCamera == SystemOFFCamera)
					{
						LoadCamera(MafiaCamera);
						m_Player.Camera.SetWeapon(3);
						m_Player.AllowState("Zoom", true);
						m_Player.AllowState("Crouch", true);
						Demo.SetWeaponPreset(MafiaWeapon, MafiaShooter, true);
					}
					else
					{
						LoadCamera(MafiaCamera);
						SwitchWeapon(3, MafiaWeapon, MafiaShooter, -1, true, true);
					}
					m_Player.Controller.Load(ModernController);
					Demo.RefreshDefaultState();
					break;
				case 2:	// --- Modern Shooter ---
					Demo.DrawCrosshair = true;
					if (m_LastLoadedCamera == SystemOFFCamera)
					{
						LoadCamera(ModernCamera);
						m_Player.Camera.SetWeapon(1);
						m_Player.AllowState("Zoom", true);
						m_Player.AllowState("Crouch", true);
						Demo.SetWeaponPreset(ModernWeapon, ModernShooter, true);
					}
					else
					{
						LoadCamera(ModernCamera);
						SwitchWeapon(1, ModernWeapon, ModernShooter, -1, true, true);
					}
					m_Player.Controller.Load(ModernController);
					Demo.RefreshDefaultState();
					break;
				case 3:	// --- Barbarian ---
					Demo.DrawCrosshair = false;
					if (m_LastLoadedCamera == SystemOFFCamera)
					{
						LoadCamera(MaceCamera);
						m_Player.Camera.SetWeapon(4);
						m_Player.AllowState("Zoom", false);
						m_Player.AllowState("Crouch", true);
						Demo.SetWeaponPreset(MaceWeapon, null, true);
					}
					else
					{
						LoadCamera(MaceCamera);
						SwitchWeapon(4, MaceWeapon, null, -1, false, true);
					}
					m_Player.Controller.Load(ModernController);
					Demo.RefreshDefaultState();
					break;
				case 4:	// --- Sniper Breath ---
					Demo.DrawCrosshair = true;
					LoadCamera(SniperCamera);
					m_Player.Camera.SetWeapon(2);
					m_Player.AllowState("Zoom", false);
					m_Player.AllowState("Crouch", false);
					Demo.SetWeaponPreset(SniperWeapon, SniperShooter, true);
					m_Player.Controller.Stop();
					m_Player.Controller.Load(CrouchController);
					Demo.Teleport(m_SniperPos, m_SniperAngle);
					Demo.RefreshDefaultState();
					break;
				case 5:	// --- Astronaut ---
					Demo.DrawCrosshair = false;
					LoadCamera(AstronautCamera);
					m_Player.Camera.SetWeapon(0);
					m_Player.AllowState("Zoom", false);
					m_Player.AllowState("Crouch", false);
					m_Player.Controller.Load(AstronautController);
					Demo.Teleport(m_AstronautPos, m_AstronautAngle);
					Demo.RefreshDefaultState();
					break;
				case 6:	// --- Mech... or Dino? ---
					Demo.DrawCrosshair = true;
					m_UnFreezePosition = m_DrunkPos;
					m_Player.Controller.Stop();
					m_Player.Controller.Load(MechController);
					Demo.Teleport(m_MechPos, m_MechAngle);
					LoadCamera(MechCamera);
					m_Player.Camera.SetWeapon(5);
					m_Player.AllowState("Zoom", true);
					m_Player.AllowState("Crouch", false);
					Demo.SetWeaponPreset(MechWeapon, MechShooter, true);
					Demo.RefreshDefaultState();
					m_Player.Camera.BobStepCallback = delegate()
					{
						m_Player.Camera.AddForce2(new Vector3(0.0f, -1.0f, 0.0f));
						m_Player.Camera.CurrentWeapon.AddForce(new Vector3(0, 0, 0), new Vector3(-0.3f, 0, 0));
						m_AudioSource.PlayOneShot(m_StompSound);
					};
					break;
				case 7:	// --- Tank Turret ---
					Demo.DrawCrosshair = true;
					LoadCamera(TurretCamera);
					m_Player.Camera.SetWeapon(3);
					m_Player.AllowState("Zoom", false);
					m_Player.AllowState("Crouch", false);
					Demo.SetWeaponPreset(TurretWeapon, TurretShooter, true);
					m_Player.Camera.CurrentWeapon.SnapSprings();
					m_Player.Controller.Stop();
					m_Player.Controller.Load(DefaultController);
					Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle);
					Demo.RefreshDefaultState();
					break;
				case 8:	// --- Drunk Person ---
					Demo.DrawCrosshair = false;
					LoadCamera(DrunkCamera);
					m_Player.Camera.SetWeapon(0);
					m_Player.AllowState("Zoom", false);
					m_Player.AllowState("Crouch", true);
					m_Player.Controller.Stop();
					m_Player.Controller.Load(DrunkController);
					Demo.Teleport(m_DrunkPos, m_DrunkAngle);
					Demo.RefreshDefaultState();
					break;
				case 9:	// --- Old School ---
					Demo.DrawCrosshair = true;
					LoadCamera(OldSchoolCamera);
					m_Player.Camera.SetWeapon(1);
					m_Player.AllowState("Zoom", false);
					m_Player.AllowState("Crouch", false);
					Demo.SetWeaponPreset(OldSchoolWeapon, OldSchoolShooter);
					m_Player.Controller.Stop();
					m_Player.Controller.Load(OldSchoolController);
					Demo.Teleport(m_OldSchoolPos, m_OldSchoolAngle);
					m_Player.Camera.SnapSprings();
					m_Player.Camera.SnapZoom();
					m_Player.CurrentWeapon.SnapZoom();
					m_Player.CurrentWeapon.SnapSprings();
					m_Player.Camera.CurrentWeapon.SnapPivot();
					Demo.RefreshDefaultState();
					break;
				case 10:// --- Crazy Cowboy ---
					Demo.DrawCrosshair = true;
					if (m_LastLoadedCamera == SystemOFFCamera)
					{
						LoadCamera(CowboyCamera);
						m_Player.Camera.SetWeapon(2);
						m_Player.AllowState("Zoom", true);
						m_Player.AllowState("Crouch", true);
						Demo.SetWeaponPreset(CowboyWeapon, CowboyShooter, true);
					}
					else
					{
						LoadCamera(CowboyCamera);
						SwitchWeapon(2, CowboyWeapon, CowboyShooter, -1, true, true);
					}
					m_Player.Controller.Load(CowboyController);
					Demo.Teleport(m_StartPos, m_StartAngle);
					Demo.RefreshDefaultState();
					break;
			}

			m_Player.SetState("Zoom", false);
			Demo.LastInputTime = Time.time;
			m_ExamplesCurrentSel = Demo.ButtonSelection;

		}

		if (m_Player.Camera.CurrentShooter != null)
		{
			if (Demo.ButtonSelection == 0)
				m_Player.Camera.CurrentShooter.AmmoCount = 0;
			else
				m_Player.Camera.CurrentShooter.AmmoCount = 100000000;
		}

		if (Demo.ShowGUI)
		{

			// show a toggle column, a compound control displaying a
			// column of buttons that can be toggled like radio buttons
			m_ExamplesCurrentSel = Demo.ButtonSelection;
			string[] strings = new string[] { "System OFF", "Mafia Boss", "Modern Shooter", "Barbarian", "Sniper Breath", "Astronaut", "Mech... or Dino?", "Tank Turret", "Drunk Person", "Old School", "Crazy Cowboy" };
			Demo.ButtonSelection = Demo.ToggleColumn(140, 150, Demo.ButtonSelection, strings, false, true, m_ImageRightPointer, m_ImageLeftPointer);

		}

		// draw menu re-enable text
		if (Demo.ShowGUI && Screen.lockCursor)
		{
			GUI.color = new Color(1, 1, 1, 1);
			GUI.Label(new Rect((Screen.width / 2) - 200, 140, 400, 20), "(Press ENTER to reenable menu)", Demo.CenterStyle);
			GUI.color = new Color(1, 1, 1, 1 * Demo.GlobalAlpha);
		}

	}


	///////////////////////////////////////////////////////////
	// demo screen to explain external forces
	///////////////////////////////////////////////////////////
	private void DemoForces()
	{

		Demo.DrawBoxes("external forces", "The camera and weapon are mounted on 8 positional and angular SPRINGS.\nEXTERNAL FORCES can be applied to these in various ways, creating unique movement patterns every time. This is useful for shockwaves, explosion knockback and earthquakes.", m_ImageLeftArrow, m_ImageRightArrow);

		if (Demo.FirstFrame)
		{
			Demo.DrawCrosshair = false;
			LoadCamera(StompingCamera);
			SwitchWeapon(1, ModernWeapon, null, -1, false, false);
			m_Player.Controller.Load(SmackController);
			m_Player.Camera.SnapZoom();
			Demo.FirstFrame = false;
			Demo.Teleport(m_ForcesPos, m_ForcesAngle);
			Demo.ButtonColumnArrowY = -100.0f;
		}

		if (Demo.ShowGUI)
		{
	
			// draw toggle column showing force examples
			Demo.ButtonSelection = -1;
			string[] strings = new string[] { "Earthquake", "Boss Stomp", "Incoming Artillery", "Crashing Airplane"};
			Demo.ButtonSelection = Demo.ButtonColumn(150, Demo.ButtonSelection, strings, m_ImageRightPointer);
			if (Demo.ButtonSelection != -1)
			{
				switch (Demo.ButtonSelection)
				{
					case 0:	// --- Earthquake ---
						LoadCamera(StompingCamera);
						m_Player.Controller.Load(SmackController);
						m_Player.Camera.DoEarthQuake(0.1f, 0.1f, 10.0f);
						Demo.ButtonColumnArrowFadeoutTime = Time.time + 9;
						m_AudioSource.Stop();
						m_AudioSource.PlayOneShot(m_EarthquakeSound);
						break;
					case 1:	// --- Boss Stomp ---
						m_Player.Camera.StopEarthQuake();
						LoadCamera(ArtilleryCamera);
						m_Player.Controller.Load(SmackController);
						m_Player.Camera.DoStomp(1.0f);
						Demo.ButtonColumnArrowFadeoutTime = Time.time;
						m_AudioSource.Stop();
						m_AudioSource.PlayOneShot(m_StompSound);
						break;
					case 2:	// --- Incoming Artillery ---
						m_Player.Camera.StopEarthQuake();
						LoadCamera(ArtilleryCamera);
						m_Player.Controller.Load(ArtilleryController);
						m_Player.Camera.DoBomb(1);
						m_Player.Controller.AddForce(UnityEngine.Random.Range(-1.5f, 1.5f), 0.1f,
																		UnityEngine.Random.Range(-1.5f, 1.5f));
						Demo.ButtonColumnArrowFadeoutTime = Time.time + 1;
						m_AudioSource.Stop();
						m_AudioSource.PlayOneShot(m_ExplosionSound);
						break;
					case 3:	// --- Crashing Airplane ---
						LoadCamera(StompingCamera);
						m_Player.Controller.Load(SmackController);
						m_Player.Camera.DoEarthQuake(0.25f, 0.2f, 10.0f, 1.0f, 6.5f);
						Demo.ButtonColumnArrowFadeoutTime = Time.time + 9;
						m_AudioSource.Stop();
						m_AudioSource.PlayOneShot(m_EarthquakeSound);
						m_Player.Camera.RenderingFieldOfView = 80;
						m_Player.Camera.Zoom();
						if (m_ChrashingAirplaneRestoreTimer != null)
							m_ChrashingAirplaneRestoreTimer.Cancel();
						m_ChrashingAirplaneRestoreTimer = vp_Timer.In(9, delegate() { m_Player.Camera.RenderingFieldOfView = 60; m_Player.Camera.Zoom(); });
						break;
				}
				Demo.LastInputTime = Time.time;
			}


			// show a screenshot preview of the mouse input editor section
			// in the bottom left corner.
			Demo.DrawEditorPreview(m_ImageWeaponPosition, m_ImageEditorPreview, m_ImageEditorScreenshot);
		}

	}

	
	///////////////////////////////////////////////////////////
	// demo screen to explain mouse smoothing and acceleration
	///////////////////////////////////////////////////////////
	private void DemoMouseInput()
	{

		Demo.DrawBoxes("mouse input", "Any good FPS should offer configurable MOUSE SMOOTHING and ACCELERATION.\n• Smoothing interpolates mouse input over several frames to reduce jittering.\n • Acceleration + low mouse sensitivity allows high precision without loss of turn speed.\n• Click the below buttons to compare some example setups.", m_ImageLeftArrow, m_ImageRightArrow);

		if (Demo.FirstFrame)
		{
			Demo.DrawCrosshair = true;
			Demo.FreezePlayer(m_MouseLookPos, m_MouseLookAngle, true);
			m_Player.AllowState("Zoom", false);
			m_Player.AllowState("Crouch", false);
			LoadCamera(MouseRawUnityCamera);
			Demo.FirstFrame = false;
			m_Player.Camera.SetWeapon(0);
			m_Player.AllowState("Zoom", false);
			m_Player.AllowState("Crouch", false);
		}

		if (Demo.ShowGUI)
		{

			// show a toggle column for mouse examples
			int currentSel = Demo.ButtonSelection;
			bool showArrow = (Demo.ButtonSelection == 2) ? false : true;	// small arrow for the 'acceleration' button
			string[] strings = new string[] { "Raw Unity Mouse Input", "Mouse Smoothing", "Low Sens. + Acceleration" };
			Demo.ButtonSelection = Demo.ToggleColumn(200, 150, Demo.ButtonSelection, strings, true, showArrow, m_ImageRightPointer, m_ImageLeftPointer);

			if (Demo.ButtonSelection != currentSel)
			{
				switch (Demo.ButtonSelection)
				{
					case 0:	// --- Raw Unity Mouse Input ---
						LoadCamera(MouseRawUnityCamera);
						break;
					case 1:	// --- Mouse Smoothing ---
						LoadCamera(MouseSmoothingCamera);
						break;
					case 2:	// --- Low Sens. + Acceleration ---
						LoadCamera(MouseLowSensCamera);
						break;
				}
				Demo.LastInputTime = Time.time;
			}

			// separate small arrow for the 'ON / OFF' buttons. this one points
			// upward and is only shown if 'acceleration' is chosen
			showArrow = true;
			if (Demo.ButtonSelection != 2)
			{
				GUI.enabled = false;
				showArrow = false;
			}

			// show a 'button toggle', a compound control for a basic on / off toggle
			m_Player.Camera.MouseAcceleration = Demo.ButtonToggle(new Rect((Screen.width / 2) + 110, 215, 90, 40),
														"Acceleration", m_Player.Camera.MouseAcceleration, showArrow, m_ImageUpPointer);
			GUI.color = new Color(1, 1, 1, 1 * Demo.GlobalAlpha);
			GUI.enabled = true;

			Demo.DrawEditorPreview(m_ImageCameraMouse, m_ImageEditorPreview, m_ImageEditorScreenshot);

		}

	}

	
	///////////////////////////////////////////////////////////
	// demo screen to explain weapon FOV and offset
	///////////////////////////////////////////////////////////
	private void DemoWeaponPerspective()
	{

		Demo.DrawBoxes("weapon perspective", "Proper WEAPON PERSPECTIVE is crucial to the final impression of your game!\nThis weapon has its own separate Field of View for full perspective control,\nalong with weapon position and rotation offset.", m_ImageLeftArrow, m_ImageRightArrow);

		if (Demo.FirstFrame)
		{
			Demo.DrawCrosshair = false;
			LoadCamera(PerspOldCamera);
			SwitchWeapon(3, PerspOldWeapon, null, -1, false, false);
			m_Player.Camera.SnapZoom();	// prevents animated zooming and instead sets the zoom in one frame
			Demo.FirstFrame = false;
			Demo.FreezePlayer(m_OverviewPos, m_PerspectiveAngle, true);
		}

		if (Demo.ShowGUI)
		{

			// show toggle column for the weapon FOV example buttons
			int currentSel = Demo.ButtonSelection;
			string[] strings = new string[] { "Old School", "1999 Internet Café", "Modern Shooter" };
			Demo.ButtonSelection = Demo.ToggleColumn(200, 150, Demo.ButtonSelection, strings, true, true, m_ImageRightPointer, m_ImageLeftPointer);
			if (Demo.ButtonSelection != currentSel)
			{
				switch (Demo.ButtonSelection)
				{
					case 0:	// --- Old School ---
						LoadCamera(PerspOldCamera);
						Demo.SetWeaponPreset(PerspOldWeapon, null, true);
						m_Player.Camera.SnapZoom();
						m_Player.Camera.CurrentWeapon.SnapPivot();	// prevents transitioning the pivot and sets its position in one frame
						Demo.FreezePlayer(m_OverviewPos, m_PerspectiveAngle, true);
						break;
					case 1:	// --- 1999 Internet Café ---
						LoadCamera(Persp1999Camera);
						Demo.SetWeaponPreset(Persp1999Weapon, null, true);
						m_Player.Camera.SnapZoom();
						m_Player.Camera.CurrentWeapon.SnapPivot();
						Demo.FreezePlayer(m_OverviewPos, m_PerspectiveAngle, true);
						break;
					case 2:	// --- Modern Shooter ---
						LoadCamera(PerspModernCamera);
						Demo.SetWeaponPreset(PerspModernWeapon, null, true);
						m_Player.Camera.SnapZoom();
						m_Player.Camera.CurrentWeapon.SnapPivot();
						Demo.FreezePlayer(m_OverviewPos, m_PerspectiveAngle, true);
						break;
				}
				Demo.LastInputTime = Time.time;
			}

			Demo.DrawEditorPreview(m_ImageWeaponPerspective, m_ImageEditorPreview, m_ImageEditorScreenshot);
		}

	}


	///////////////////////////////////////////////////////////
	// demo screen for explaining weapon camera layer
	// NOTE: weapon layer is hardcoded as layer 31. this is
	// set in vp_Layer.cs
	///////////////////////////////////////////////////////////
	private void DemoWeaponLayer()
	{

		Demo.DrawBoxes("weapon layer", "\nThe weapon is rendered by a SEPARATE CAMERA so that it never sticks through walls or other geometry. Try toggling the weapon layer ON and OFF below.", m_ImageLeftArrow, m_ImageRightArrow);

		if (Demo.FirstFrame)
		{
			Demo.DrawCrosshair = true;
			LoadCamera(WallFacingCamera);
			m_Player.Camera.SetWeapon(3);
			m_Player.AllowState("Zoom", false);
			m_Player.AllowState("Crouch", false);
			Demo.SetWeaponPreset(WallFacingWeapon);
			m_Player.Camera.SnapZoom();
			m_WeaponLayerToggle = false;
			Demo.FirstFrame = false;
			Demo.FreezePlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
			int layer = (m_WeaponLayerToggle ? vp_Layer.Weapon : 0);
			m_Player.Camera.SetWeaponLayer(layer);
		}

		if (Demo.ShowGUI)
		{

			// show button toggle for enabling / disabling the weapon layer
			bool currentWeaponLayer = m_WeaponLayerToggle;
			m_WeaponLayerToggle = Demo.ButtonToggle(new Rect((Screen.width / 2) - 45, 180, 90, 40), "Weapon Layer",
													m_WeaponLayerToggle, true, m_ImageUpPointer);
			if (currentWeaponLayer != m_WeaponLayerToggle)
			{
				Demo.FreezePlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
				int layer = (m_WeaponLayerToggle ? vp_Layer.Weapon : 0);
				m_Player.Camera.SetWeaponLayer(layer);
				Demo.LastInputTime = Time.time;
			}

		}

	}


	///////////////////////////////////////////////////////////
	// demo screen to explain pivot manipulation
	///////////////////////////////////////////////////////////
	private void DemoPivot()
	{

		Demo.DrawBoxes("weapon pivot", "The PIVOT POINT of the weapon model greatly affects movement pattern.\nManipulating it at runtime can be quite useful, and easy with Ultimate FPS Camera!\nClick the examples below and move the camera around.", m_ImageLeftArrow, m_ImageRightArrow, delegate() { Application.LoadLevel(1); });

		if (Demo.FirstFrame)
		{
			Demo.DrawCrosshair = false;
			LoadCamera(DefaultCamera);
			m_Player.Controller.Load(DefaultController);
			Demo.FirstFrame = false;
			Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle);
			m_Player.Camera.SetWeapon(1);
			m_Player.AllowState("Zoom", false);
			m_Player.AllowState("Crouch", false);
			Demo.SetWeaponPreset(DefaultWeapon);
			Demo.SetWeaponPreset(PivotMuzzleWeapon, null, true);
			m_Player.Camera.CurrentWeapon.SetPivotVisible(true);

		}

		if (Demo.ShowGUI)
		{

			// show toggle column for the various pivot examples
			int currentSel = Demo.ButtonSelection;
			string[] strings = new string[] { "Muzzle", "Grip", "Chest", "Elbow (Uzi Style)" };
			Demo.ButtonSelection = Demo.ToggleColumn(200, 150, Demo.ButtonSelection, strings, true, true, m_ImageRightPointer, m_ImageLeftPointer);
			if (Demo.ButtonSelection != currentSel)
			{
				switch (Demo.ButtonSelection)
				{
					case 0:	// --- Muzzle ---
						Demo.SetWeaponPreset(PivotMuzzleWeapon, null, true);
						break;
					case 1:	// --- Grip ---
						Demo.SetWeaponPreset(PivotWristWeapon, null, true);
						break;
					case 2:	// --- Chest ---
						Demo.SetWeaponPreset(PivotChestWeapon, null, true);
						break;
					case 3:	// --- Elbow (Uzi Style) ---
						Demo.SetWeaponPreset(PivotElbowWeapon, null, true);
						break;
				}
				Demo.LastInputTime = Time.time;
			}

			Demo.DrawEditorPreview(m_ImageWeaponPivot, m_ImageEditorPreview, m_ImageEditorScreenshot);
		}

	}


	///////////////////////////////////////////////////////////
	// loads a camera preset and remembers it as the last used
	// camera preset (for demo weapon transition special cases)
	///////////////////////////////////////////////////////////
	private void LoadCamera(TextAsset asset)
	{

		m_LastLoadedCamera = asset;
		m_Player.Camera.Load(asset);

	}


	///////////////////////////////////////////////////////////
	// performs SwitchWeapon with a weapon preset. optional
	// parameters are a shooter preset and a fixed ammo count.
	// presets and ammo are set when the new weapon goes active
	///////////////////////////////////////////////////////////
	private void SwitchWeapon(int weapon, TextAsset weaponPreset, TextAsset shooterPreset = null,
							 int ammo = -1, bool allowZoom = true, bool allowCrouch = true)
	{

		if (shooterPreset == null)
		{
			m_Player.Camera.SwitchWeapon(weapon, delegate()
			{
				if (m_Player.Camera.CurrentWeapon != null)
					m_Player.Camera.CurrentWeapon.Load(weaponPreset);
				m_Player.AllowState("Zoom", allowZoom);
				m_Player.AllowState("Crouch", allowCrouch);
				m_Player.Camera.Refresh();
				m_Player.Camera.RefreshDefaultState();
			});
		}
		else
		{
			m_Player.Camera.SwitchWeapon(weapon, delegate()
			{
				if (m_Player.Camera.CurrentWeapon != null)
					m_Player.Camera.CurrentWeapon.Load(weaponPreset);
				if (m_Player.Camera.CurrentShooter != null)
				{
					m_Player.Camera.CurrentShooter.Load(shooterPreset);
					if (ammo > -1)
						m_Player.Camera.CurrentShooter.AmmoCount = ammo;
				}
				m_Player.AllowState("Zoom", allowZoom);
				m_Player.AllowState("Crouch", allowCrouch);
				m_Player.Camera.Refresh();
				m_Player.Camera.RefreshDefaultState();
			});
		}

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void OnGUI()
	{

		Demo.OnGUI();

		// perform drawing method specific to the current demo screen
		switch (Demo.CurrentScreen)
		{
			case 1: DemoIntro(); break;
			case 2: DemoExamples(); break;
			case 3: DemoForces(); break;
			case 4: DemoMouseInput(); break;
			case 5: DemoWeaponPerspective(); break;
			case 6: DemoWeaponLayer(); break;
			case 7: DemoPivot(); break;
		}


	}
	
}

