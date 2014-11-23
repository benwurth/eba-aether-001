/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSPlayer.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	example class for an FPS player. this script is a regular
//					monobehaviour and can be heavily modified to suit your
//					game's needs.
//					NOTE: the 'Crouch', 'Zoom' and 'Run' states used in this
//					script refer to the state names of the example FPSPlayer
//					prefab. these states are not hard coded by the system. you
//					can rename them or remove them. you can also easily create
//					your own states on the FPS components in the Inspector and
//					bind them to controls via this or any other script.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(vp_FPSController))]

public class vp_FPSPlayer : MonoBehaviour
{

	// components
	[HideInInspector]
	public vp_FPSCamera Camera = null;
	[HideInInspector]
	public vp_FPSController Controller = null;

	protected bool m_LockCursor = true;						// enables / disables the mouse cursor

	// timers
	protected vp_Timer m_ReenableWeaponStatesTimer = null;	// timer to reenable crouch / run states on the weapon after firing et cetera
	protected vp_Timer m_DoneReloadingTimer = null;			// timer for disabling the reload state when done reloading

	// player info
	public float m_Health = 10.0f;							// placeholder health variable, modified by the 'Damage' method
	protected List<GameObject> m_AvailableWeapons = new List<GameObject>();		// placeholder for your game's actual inventory system.


	///////////////////////////////////////////////////////////
	// properties
	///////////////////////////////////////////////////////////
	public bool LockCursor { get { return m_LockCursor; } set { m_LockCursor = value; Screen.lockCursor = value; } }
	public vp_FPSWeapon CurrentWeapon { get { return Camera.CurrentWeapon; } }
	public int CurrentWeaponID { get { return Camera.CurrentWeaponID; } }
	public vp_FPSShooter CurrentShooter { get { return Camera.CurrentShooter; } }
	public int WeaponCount { get { return Camera.Weapons.Count; } }


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Awake()
	{

		// get hold of the vp_FPSCamera and vp_FPSController attached to
		// this game object. NOTE: vp_FPSWeapons and vp_FPSShooters are
		// accessed via 'CurrentWeapon' and 'CurrentShooter'
		Camera = gameObject.GetComponentInChildren<vp_FPSCamera>();
		Controller = gameObject.GetComponent<vp_FPSController>();

	}


	///////////////////////////////////////////////////////////
	// in 'Start' we do things that potentially depend on all
	// other components first having run their 'Awake' calls
	///////////////////////////////////////////////////////////
	void Start()
	{

		// set up weapon availability. if a weapon is not in this
		// list the script won't allow the player to use it. all
		// weapons are available by default.
		// NOTE: this is just a placeholder for your game's actual
		// inventory system (as is ammo availability, which is set
		// on every vp_FPSShooter individually)
		foreach (GameObject weapon in Camera.Weapons)
		{
			m_AvailableWeapons.Add(weapon);
		}

		// use the 'SetWeaponAvailable' method to give or take weapons
		// from the player, and check 'WeaponAvailable' to see if the player
		// has a certain weapon. all weapons can be taken away by doing:
		// 'm_AvailableWeapons.Clear();'

		// ready weapon 1 by default
		if (WeaponCount > 0)
			SetWeapon(1);

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Update()
	{

		// if the 'LockCursor' property is set on the player, hide
		// mouse cursor and center it every frame 
		Screen.lockCursor = LockCursor;

		// TIP: edit the 'Input' methods to customize how your game
		// deals with input

		// handle input for moving
		InputWalk();
		InputRun();
		InputJump();
		InputCrouch();

		// handle input for weapons
		InputFire();
		InputZoom();
		InputReload();
		InputCycleWeapon();
		InputSetWeapon();

		// TIP: uncomment either of these lines to debug print the
		// speed of the character controller
		//Debug.Log(Controller.Velocity.magnitude);		// speed in meters per second
		//Debug.Log(Controller.Velocity.sqrMagnitude);	// speed as used by the camera bob
		
	}


	///////////////////////////////////////////////////////////
	// move the controller forward, back, left and right when
	// the 'WASD' keys are being pressed, respectively
	///////////////////////////////////////////////////////////
	protected void InputWalk()
	{

		// classic 'WASD' first person controls
		if (Input.GetKey(KeyCode.W)) { Controller.MoveForward(); }
		if (Input.GetKey(KeyCode.A)) { Controller.MoveLeft(); }
		if (Input.GetKey(KeyCode.S)) { Controller.MoveBack(); }
		if (Input.GetKey(KeyCode.D)) { Controller.MoveRight(); }

		// you may replace the above code with the following to use
		// input axes instead, though it might feel a bit sluggish
		// and require some tweaking

		//if (Input.GetAxis("Vertical") > 0.01f) { Controller.MoveForward(); }
		//if (Input.GetAxis("Horizontal") < -0.01f) { Controller.MoveLeft(); }
		//if (Input.GetAxis("Vertical") < -0.01f) { Controller.MoveBack(); }
		//if (Input.GetAxis("Horizontal") > 0.01f) { Controller.MoveRight(); }

	}

	
	///////////////////////////////////////////////////////////
	// put player in the 'Run' state while holding 'Left Shift'
	///////////////////////////////////////////////////////////
	protected void InputRun()
	{

		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			SetState("Zoom", false);
			SetState("Run", true);
		}

		if (Input.GetKeyUp(KeyCode.LeftShift))
			SetState("Run", false);

	}


	///////////////////////////////////////////////////////////
	// jump on 'SPACE'. the current controller preset determines
	// the jump force
	///////////////////////////////////////////////////////////
	protected void InputJump()
	{

		if (Input.GetKeyDown(KeyCode.Space))
			Controller.Jump();

	}


	///////////////////////////////////////////////////////////
	// put player in the 'Crouch' state while holding 'C'.
	// also: halve the height of the character controller so it
	// makes a smaller target and may climb through smaller openings
	///////////////////////////////////////////////////////////
	protected void InputCrouch()
	{

		if (Input.GetKeyDown(KeyCode.C))
			SetState("Crouch", true);

		if (Input.GetKeyUp(KeyCode.C))
			SetState("Crouch", false);

		// update height of the character controller collision.
		// NOTE: this must be kept up to date every frame or the
		// character controller may fall through the ground
		if (Controller.StateManager.IsEnabled("Crouch"))
			Controller.Compact = true;
		else
			Controller.Compact = false;

	}


	///////////////////////////////////////////////////////////
	// fire the current weapon when player presses the left
	// mouse button. this temporarily disables the 'Crouch' and
	// 'Run' states on the current weapon
	///////////////////////////////////////////////////////////
	protected void InputFire()
	{

		// fire button pressed / held down
		if (Input.GetMouseButton(0))
		{
			if (CurrentWeapon != null)
			{

				// if firing while crouching or running, point the weapon
				// straight forward
				if (CurrentWeapon.StateEnabled("Crouch"))
					CurrentWeapon.SetState("Crouch", false);
				if (CurrentWeapon.StateEnabled("Run"))
					CurrentWeapon.SetState("Run", false);

				// fire if we have a shooter component and the mouse cursor
				// is not visible
				if (CurrentShooter != null && LockCursor)
					CurrentShooter.Fire();

				// TIP: to play a conventional animation upon firing (typically
				// used for melee weapons) uncomment one of the lines below
				// (see the Unity docs for more available methods & parameters).
				//CurrentWeapon.animation.Play();									// play the default animation
				//CurrentWeapon.animation.Play("your_animation");					// play the animation named 'your_animation' and stop all other animations in the same layer
				//CurrentWeapon.animation.Play("your_animation", PlayMode.StopAll);	// play 'your_animation' and stop all other animations
				//CurrentWeapon.animation.CrossFade("your_animation", 0.2f);		// fade in 'your_animation' and fade out all other animations over 0.2 seconds

			}
		}

		// fire button released
		if (Input.GetMouseButtonUp(0))
		{

			// schedule to reenable 'Crouch' and / or 'Run' in half a second
			ReenableWeaponStatesIn(0.5f);

			// disregard refire delay when firing with no ammo
			if (CurrentShooter != null)
			{
				if (CurrentShooter.AmmoCount == 0)
					CurrentShooter.NextAllowedFireTime = Time.time;
			}

		}

	}


	///////////////////////////////////////////////////////////
	// zoom in using middle and right mouse button
	///////////////////////////////////////////////////////////
	protected void InputZoom()
	{

		if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
			SetState("Zoom", true);
		if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
		{
			SetState("Zoom", false);
			ReenableWeaponStatesIn(0.5f);	// schedule to reenable 'Crouch' and / or 'Run' in half a second
		}

	}


	///////////////////////////////////////////////////////////
	// allow reloading on 'R'
	///////////////////////////////////////////////////////////
	protected void InputReload()
	{

		if (Input.GetKeyDown(KeyCode.R))
		{
			if(CurrentShooter != null)
				Reload(CurrentShooter.AmmoMaxCount);
		}

	}


	///////////////////////////////////////////////////////////
	// switches to weapon 1-10 when pressing the 1-0 buttons,
	// respectively
	///////////////////////////////////////////////////////////
	protected void InputSetWeapon()
	{

		if ((WeaponCount > 0) && (Input.GetKeyDown(KeyCode.Alpha1))) SetWeapon(1);
		if ((WeaponCount > 1) && (Input.GetKeyDown(KeyCode.Alpha2))) SetWeapon(2);
		if ((WeaponCount > 2) && (Input.GetKeyDown(KeyCode.Alpha3))) SetWeapon(3);
		if ((WeaponCount > 3) && (Input.GetKeyDown(KeyCode.Alpha4))) SetWeapon(4);
		if ((WeaponCount > 4) && (Input.GetKeyDown(KeyCode.Alpha5))) SetWeapon(5);
		if ((WeaponCount > 5) && (Input.GetKeyDown(KeyCode.Alpha6))) SetWeapon(6);
		if ((WeaponCount > 6) && (Input.GetKeyDown(KeyCode.Alpha7))) SetWeapon(7);
		if ((WeaponCount > 7) && (Input.GetKeyDown(KeyCode.Alpha8))) SetWeapon(8);
		if ((WeaponCount > 8) && (Input.GetKeyDown(KeyCode.Alpha9))) SetWeapon(9);
		if ((WeaponCount > 9) && (Input.GetKeyDown(KeyCode.Alpha0))) SetWeapon(10);

	}


	///////////////////////////////////////////////////////////
	// cycles through all the weapons on the 'Q' and 'E' keys
	///////////////////////////////////////////////////////////
	protected void InputCycleWeapon()
	{

		// cycle to next weapon
		if (Input.GetKeyDown(KeyCode.E))
			SetNextWeapon();

		// cycle to previous weapon
		if (Input.GetKeyDown(KeyCode.Q))
			SetPrevWeapon();

	}


	///////////////////////////////////////////////////////////
	// toggles to the previous weapon, if currently available,
	// otherwise attempts to skip past it
	///////////////////////////////////////////////////////////
	protected void SetPrevWeapon()
	{

		int i = CurrentWeaponID - 1;

		// skip past weapon '0'
		if (i < 1)
			i = WeaponCount;

		int iterations = 0;
		while (!SetWeapon(i))
		{

			i--;
			if (i < 1)
				i = WeaponCount;

			iterations++;
			if(iterations > WeaponCount)
				break;

		}

	}


	///////////////////////////////////////////////////////////
	// toggles to the next weapon, if currently available,
	// otherwise attempts to skip past it
	///////////////////////////////////////////////////////////
	protected void SetNextWeapon()
	{

		int i = CurrentWeaponID + 1;
		int iterations = 0;
		while (!SetWeapon(i))
		{

			if (i > WeaponCount)
				i = 0;
			i++;

			iterations++;
			if(iterations > WeaponCount)
				break;

		}

	}


	///////////////////////////////////////////////////////////
	// switches to a new weapon (after aborting reloading)
	///////////////////////////////////////////////////////////
	public bool SetWeapon(int weapon)
	{

		// return if the player cannot currently wield this weapon
		if (weapon > 0 && !WeaponAvailable(weapon))
			return false;

		if (weapon == CurrentWeaponID)
			return false;

		SetState("Reload", false);

		Camera.SwitchWeapon(weapon);
		ReenableWeaponStatesIn(0.5f);

		return true;

	}
	

	///////////////////////////////////////////////////////////
	// reloads the current shooter with 'amount' ammo and deals
	// with states and timers concerning reloading
	///////////////////////////////////////////////////////////
	public void Reload(int amount)
	{

		if (CurrentShooter == null)
			return;

		if (Time.time < CurrentShooter.NextAllowedReloadTime)
			return;

		if (m_ReenableWeaponStatesTimer != null)
			m_ReenableWeaponStatesTimer.Cancel();

		SetState("Reload", true);

		CurrentShooter.Reload(amount);

		if (m_DoneReloadingTimer != null)
			m_DoneReloadingTimer.Cancel();
		m_DoneReloadingTimer = vp_Timer.In(CurrentShooter.AmmoReloadTime, delegate()
		{
			SetState("Reload", false);
		});

	}


	///////////////////////////////////////////////////////////
	// sets a state on the controller, camera, current weapon
	// and current shooter. NOTE: SetState does not update
	// currently disabled weapons or shooters
	///////////////////////////////////////////////////////////
	public void SetState(string state, bool isEnabled)
	{

		if (Controller != null)
			Controller.SetState(state, isEnabled);

		if (Camera != null)
		{
			Camera.SetState(state, isEnabled);
			if (CurrentWeapon != null)
				CurrentWeapon.SetState(state, isEnabled);
			if (CurrentShooter != null)
				CurrentShooter.SetState(state, isEnabled);
		}

	}


	///////////////////////////////////////////////////////////
	// allows or disallows a state on all the player's FPS
	// components (NOTE: including those currently disabled).
	// this is useful for having areas or situations where a
	// certain available state cannot be enabled by the player,
	// such as preventing running in a room full of mud (TIP:
	// use triggers) or disabling aiming while shell shocked.
	///////////////////////////////////////////////////////////
	public void AllowState(string state, bool isAllowed)
	{

		// 'AllowStateRecursively' will begin with the Controller
		// and continue down the hierarchy
		if (Controller != null)
			Controller.AllowStateRecursively(state, isAllowed);

	}


	///////////////////////////////////////////////////////////
	// this method schedules a check in 'seconds' to reenable
	// any active states on the controller that may have been
	// temporarily disabled on the current weapon
	///////////////////////////////////////////////////////////
	public void ReenableWeaponStatesIn(float seconds)
	{

		// cancel the timer if it's currently running, to prevent
		// problems with button spamming
		if (m_ReenableWeaponStatesTimer != null)
			m_ReenableWeaponStatesTimer.Cancel();

		m_ReenableWeaponStatesTimer = vp_Timer.In(seconds, delegate()
		{
			if (Controller.StateEnabled("Zoom"))
			{
				if (CurrentWeapon != null)
					CurrentWeapon.SetState("Zoom", true);
				if (CurrentShooter != null)
					CurrentShooter.SetState("Zoom", true);
				return;	// don't reenable 'Crouch' or 'Run' on the weapon while zooming
			}
			if (Controller.StateEnabled("Crouch"))
			{
				if (CurrentWeapon != null)
					CurrentWeapon.SetState("Crouch", true);
				if (CurrentShooter != null)
					CurrentShooter.SetState("Crouch", true);
			}
			if (Controller.StateEnabled("Run"))
			{
				if (CurrentWeapon != null)
					CurrentWeapon.SetState("Run", true);
				if (CurrentShooter != null)
					CurrentShooter.SetState("Run", true);
			}
		});

	}


	///////////////////////////////////////////////////////////
	// disables all states except the default state, and enables
	// the default state for the controller and the current
	// weapon and shooter
	///////////////////////////////////////////////////////////
	public void ResetState()
	{

		if (Controller != null)
			Controller.ResetState();
		if (Camera != null)
		{
			Camera.ResetState();
			if (CurrentWeapon != null)
				CurrentWeapon.ResetState();
			if (CurrentShooter != null)
				CurrentShooter.ResetState();
		}

	}
	
	
	///////////////////////////////////////////////////////////
	// makes a certain weapon available or unavailable to the player.
	// NOTE: this method should not be called from an external Awake
	// or Start call, since vp_FPSCamera:Awake & vp_FPSPlayer:Start
	// must both have run first in order for their weapon lists to
	// be populated
	///////////////////////////////////////////////////////////
	public void SetWeaponAvailable(int weapon, bool isAvailable)
	{

		try
		{

			if (WeaponCount < 1)
			{
				Debug.LogError("Error: Tried to update weapon availability with an empty weapon list.");
				return;
			}

			if (weapon < 1 || weapon > WeaponCount)
			{
				Debug.LogError("Error: No such weapon ID: " + weapon);
				return;
			}

			// update availability of 'weapon'
			if(isAvailable)
			{
				// make 'weapon' available
				if(!m_AvailableWeapons.Contains(Camera.Weapons[weapon - 1]))
					m_AvailableWeapons.Add(Camera.Weapons[weapon - 1]);
				// if no weapon was currently wielded, wield the newly available one
				if(CurrentWeaponID == 0)
					SetNextWeapon();
			}
			else
			{
				// make 'weapon' unavailable
				m_AvailableWeapons.Remove(Camera.Weapons[weapon - 1]);
				// if there are no more available weapons, unwield the
				// current weapon and return
				if(m_AvailableWeapons.Count == 0)
				{
					Camera.SwitchWeapon(0);
					return;
				}
				// if the currently wielded weapon was made unavailable,
				// cycle to the next weapon
				if(CurrentWeaponID != 0 && !WeaponAvailable(CurrentWeaponID))
					SetNextWeapon();
			}

		}
		catch
		{
			Debug.LogError("Error: 'SetWeaponAvailable' failed. NOTE: It's risky to call this method from an external 'Awake' or 'Start' method (weapon lists must be populated first).");
		}

	}


	///////////////////////////////////////////////////////////
	// returns true if the weapon with the corresponding id is
	// present in the list of weapons available to the player.
	// NOTE: this method should not be called from an external Awake
	// or Start call, since vp_FPSCamera:Awake & vp_FPSPlayer:Start
	// must both have run first in order for their weapon lists to
	// be populated
	///////////////////////////////////////////////////////////
	public bool WeaponAvailable(int weapon)
	{

		try
		{
	
			if (WeaponCount < 1)
				return false;

			if (weapon < 1 || weapon > WeaponCount)
				return false;

			return m_AvailableWeapons.Contains(Camera.Weapons[weapon - 1]);

		}
		catch
		{
			Debug.LogError("Error: 'WeaponAvailable' failed. NOTE: It's risky to call this method from an external 'Awake' or 'Start' method (weapon lists must be populated first).");
			return false;
		}

	}


	///////////////////////////////////////////////////////////
	// here's a stub for a Damage method that can be called by
	// damage causing entities like vp_Explosion
	///////////////////////////////////////////////////////////
	public void Damage(float damage)
	{

		//m_Health -= damage;
		//Debug.Log("Health = " + m_Health);
		//if (m_Health < 0.0f)
		//{
		//	Debug.Log("Player died.");
		//	// NOTE: remember to restore the m_Health variable after respawn
		//	// TIP: if health goes far below zero, gib the player!
		//}

	}


	///////////////////////////////////////////////////////////
	//
	///////////////////////////////////////////////////////////
	void OnGUI()
	{

		// uncomment this snippet to display a simple 'Health' HUD
		//GUI.Box(new Rect(10, Screen.height - 30, 100, 20), "Health: " + (int)(m_Health * 10) + "%");

		// uncomment this snippet to display a simple 'Ammo' HUD
		//GUI.Box(new Rect(Screen.width - 110, Screen.height - 30, 100, 20), "Ammo: " + CurrentShooter.AmmoCount);

	}


}

