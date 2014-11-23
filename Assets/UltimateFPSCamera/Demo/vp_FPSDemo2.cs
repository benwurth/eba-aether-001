/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemo2.cs
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


public class vp_FPSDemo2 : MonoBehaviour
{

	private vp_FPSDemoManager m_Demo = null;

	public Texture ImageLeftArrow = null;
	public Texture ImageRightArrow = null;
	public Texture ImageCheckmark = null;
	public Texture ImagePresetDialogs = null;
	public Texture ImageShooter = null;
	public Texture ImageAllParams = null;

	public GameObject PlayerGameObject = null;
	private vp_FPSPlayer m_Player = null;

	private Vector3 m_StartPos = new Vector3(103, 105, -119);
	private Vector2 m_StartAngle = new Vector2(39, 0);

	private Vector3 m_OverviewPos = new Vector3(114, 107, -86);
	private Vector2 m_OverviewAngle = new Vector2(153.5f, 10);


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Start()
	{

		m_Player = (vp_FPSPlayer)PlayerGameObject.GetComponent(typeof(vp_FPSPlayer));
		m_Demo = new vp_FPSDemoManager(m_Player);
		m_Player.LockCursor = false;

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Update()
	{

		m_Demo.Update();

		// input special cases for the 'STATES DEMO' screen
		if (m_Demo.CurrentScreen == 2)
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
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
		    if (m_Player.Camera.CurrentWeaponID == 1)
		        DoExamplePistolReloadSequence();
		}

		// if user presses 'ENTER' toggle the mouse pointer
		if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
			m_Player.LockCursor = !m_Player.LockCursor;

	}


	///////////////////////////////////////////////////////////
	// demo screen to explain preset loading and saving features
	///////////////////////////////////////////////////////////
	private void DemoPreset()
	{


		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle, true);
			m_Player.Camera.CancelWeaponSwitch();
			m_Player.SetWeapon(0);
		}

		m_Demo.DrawBoxes("presets & states", "Perhaps our most powerful feature is the COMPONENT PRESET SYSTEM, which allows you to save component snapshots in text files. The new advanced STATE MANAGER then blends between your presets at runtime. This allows for very complex preset combinations with smooth, natural transitions (and little or no need for scripting).", null, ImageRightArrow);
		m_Demo.DrawImage(ImagePresetDialogs);

		m_Demo.ForceCameraShake();
		
	}


	///////////////////////////////////////////////////////////
	// demo screen to introduce the state system
	///////////////////////////////////////////////////////////
	private void DemoStates()
	{

		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = true;
			m_Demo.UnFreezePlayer();
			m_Demo.Teleport(m_StartPos, m_StartAngle);
			m_Player.LockCursor = true;
			m_Player.SetWeapon(1);
		}
		
		m_Demo.DrawBoxes("states demo", "This player has been set up with some standard First Person Shooter states.\n• Press SHIFT to RUN, C to CROUCH, and the RIGHT or MIDDLE mouse button to ZOOM.\n• To SWITCH WEAPONS, press Q, E or 1-3.\n• Press R to RELOAD", ImageLeftArrow, ImageRightArrow);

		// draw menu re-enable text
		if (m_Demo.ShowGUI && m_Player.LockCursor)
		{
			GUI.color = new Color(1, 1, 1, 1);
			GUI.Label(new Rect((Screen.width / 2) - 200, 140, 400, 20), "(Press ENTER to reenable menu)", m_Demo.CenterStyle);
			GUI.color = new Color(1, 1, 1, 1 * m_Demo.GlobalAlpha);
		}

	}
	

	///////////////////////////////////////////////////////////
	// demo screen to introduce the shooter component & fx features
	///////////////////////////////////////////////////////////
	private void DemoShooting()
	{

		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle, true);
			m_Player.SetWeapon(0);
		}

		m_Demo.DrawBoxes("examples & FX", "Ultimate FPS Camera has many example scripts for rapid prototyping, including raycast bullets, advanced recoil, realistic shell case physics, bullet holes and muzzleflashes.\nNEW in v1.3: Explosions with area damage, knockback and particle effects. And a damage handler allowing your gameobjects to take damage, die and respawn.", ImageLeftArrow, ImageRightArrow);
		m_Demo.DrawImage(ImageShooter);

		m_Demo.ForceCameraShake();

	}


	///////////////////////////////////////////////////////////
	// demo screen to show a final summary message and editor screenshot
	///////////////////////////////////////////////////////////
	private void DemoOutro()
	{

		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle, true);
			m_Player.SetWeapon(0);
		}

		m_Demo.DrawBoxes("putting it all together", "Included in the package is full, well commented C# source code, a very detailed 63 page manual in PDF format, a game-ready FPS PLAYER prefab along with all the scripts and content used in this demo. A fantastic starting point - or upgrade - for any FPS project.\nGet it from the Unity Asset Store today!", ImageLeftArrow, ImageCheckmark, delegate() { Application.LoadLevel(0); });
		m_Demo.DrawImage(ImageAllParams);

	}

			
	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void OnGUI()
	{

		m_Demo.OnGUI();
				
		// perform drawing method specific to the current demo screen
		switch (m_Demo.CurrentScreen)
		{
			case 1:
				DemoPreset();
				break;
			case 2:
				DemoStates();
				break;
			case 3:
				DemoShooting();
				break;
			case 4:
				DemoOutro();
				break;
		}

	}


	///////////////////////////////////////////////////////////
	// this is just an example of how you can play around with
	// states and timers to create animations.
	// NOTE: this is provided just as an example. it is not the
	// recommended way of doing things: any complex animation such
	// as a pistol reload sequence should be done using a regular
	// animation on the pistol mesh. if you want this method in
	// your actual game project (not really recommended), copy it
	// into your player script along with its reference in 'Update'
	///////////////////////////////////////////////////////////
	vp_Timer m_PistolReloadTimer = null;
	private void DoExamplePistolReloadSequence()
	{

		// NOTE: (the 'Reload' state is activated in the 'Reload'
		// method of vp_FPSPlayer so the pistol is already tilted
		// to the side)

		// always disable the timer if it's running, to avoid hickups
		// caused by button spamming
		if (m_PistolReloadTimer != null)
			m_PistolReloadTimer.Cancel();

		// after 0.4 seconds, simulate replacing the clip
		m_PistolReloadTimer = vp_Timer.In(0.4f, delegate()
		{
			// but first make sure we're still reloading since the player
			// may have switched weapons
			if (!m_Player.Camera.CurrentWeapon.StateEnabled("Reload"))
				return;

			// apply a force as if hitting the gun from below
			m_Player.Camera.CurrentWeapon.AddForce2(new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));

			// 0.15 seconds later, twist the gun backwards
			if (m_PistolReloadTimer != null)
				m_PistolReloadTimer.Cancel();
			m_PistolReloadTimer = vp_Timer.In(0.15f, delegate()
			{
				if (!m_Player.Camera.CurrentWeapon.StateEnabled("Reload"))
					return;
				// to do this, switch from the pistol 'Reload' state to
				// its 'Reload2' state
				m_Player.Camera.SetState("Reload", false);
				m_Player.Camera.CurrentWeapon.SetState("Reload", false);
				m_Player.Camera.CurrentWeapon.SetState("Reload2", true);
				m_Player.Camera.CurrentWeapon.RotationOffset.z = 0;
				m_Player.Camera.CurrentWeapon.Refresh();

				// after 0.35 seconds, pull the slide
				if (m_PistolReloadTimer != null)
					m_PistolReloadTimer.Cancel();
				m_PistolReloadTimer = vp_Timer.In(0.35f, delegate()
				{
					if (!m_Player.Camera.CurrentWeapon.StateEnabled("Reload2"))
						return;

					// apply a force pulling the whole gun backwards briefly
					m_Player.Camera.CurrentWeapon.AddForce2(new Vector3(0, 0, -0.05f), new Vector3(5, 0, 0));

					// 0.1 seconds later, disable the reload state to point
					// the gun forward again
					if (m_PistolReloadTimer != null)
						m_PistolReloadTimer.Cancel();
					m_PistolReloadTimer = vp_Timer.In(0.1f, delegate()
					{
						m_Player.SetState("Reload2", false);
						// if the player is crouching, restore the zoom state on the pistol
						if (m_Player.Controller.StateEnabled("Zoom"))
							m_Player.Camera.CurrentWeapon.SetState("Zoom", true);
						m_Player.CurrentShooter.NextAllowedFireTime = Time.time + 0.5f;
						m_Player.CurrentShooter.NextAllowedReloadTime = Time.time + 0.5f;
					});
				});
			});

		});

	}

}

