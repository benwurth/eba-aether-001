/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSShooter.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	this class adds firearm features to a vp_FPSWeapon. it handles
//					recoil, firing rate, projectiles, muzzle flashes, shell casings,
//					and shooting sound
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Reflection;

[RequireComponent(typeof(vp_FPSWeapon))]
[RequireComponent(typeof(AudioSource))]

public class vp_FPSShooter : vp_Component
{

	protected vp_FPSWeapon m_Weapon = null;				// the weapon affected by the shooter
	protected vp_FPSCamera m_Camera = null;				// the main first person camera view

	// projectile
	public GameObject ProjectilePrefab = null;			// prefab with a mesh and projectile script
	public float ProjectileScale = 1.0f;				// scale of the projectile decal
	public float ProjectileFiringRate = 0.3f;			// delay between shots fired
	public int ProjectileCount = 1;						// amount of projectiles to fire at once
	public float ProjectileSpread = 0.0f;				// accuracy deviation in degrees (0 = spot on)
	protected float m_NextAllowedFireTime = 0.0f;			// the next time firing will be allowed after having recently fired a shot

	// motion
	public Vector3 MotionPositionRecoil = new Vector3(0, 0, -0.035f);	// positional force applied to weapon upon firing
	public Vector3 MotionRotationRecoil = new Vector3(-10.0f, 0, 0);	// angular force applied to weapon upon firing
	public float MotionPositionReset = 0.5f;			// how much to reset weapon to its normal position upon firing (0-1)
	public float MotionRotationReset = 0.5f;
	public float MotionPositionPause = 1.0f;			// time interval over which to freeze and fade swaying forces back in upon firing
	public float MotionRotationPause = 1.0f;
	public float MotionDryFireRecoil = -0.1f;			// multiplies recoil when the weapon is out of ammo

	// muzzle flash
	public Vector3 MuzzleFlashPosition = Vector3.zero;	// position of the muzzle in relation to the camera
	public Vector3 MuzzleFlashScale = Vector3.one;		// scale of the muzzleflash
	public float MuzzleFlashFadeSpeed = 0.075f;			// the amount of muzzle flash alpha to deduct each frame
	public GameObject MuzzleFlashPrefab = null;			// muzzleflash prefab with a mesh and vp_MuzzleFlash script
	protected GameObject m_MuzzleFlash = null;			// the instantiated muzzle flash. one per weapon that's always there.
	protected vp_MuzzleFlash m_MuzzleFlashComponent = null;	// the 'vp_MuzzleFlash' script governing muzzleflash behavior

	// shell casing
	public GameObject ShellPrefab = null;				// shell prefab with a mesh and vp_Shell script
	public float ShellScale = 1.0f;						// scale of ejected shell casings
	public Vector3 ShellEjectDirection = new Vector3(1, 1, 1);	// direction of ejected shell casing
	public Vector3 ShellEjectPosition = new Vector3(1, 0, 1);	// position of ejected shell casing in relation to camera
	public float ShellEjectVelocity = 0.2f;				// velocity of ejected shell casing
	public float ShellEjectDelay = 0.0f;				// time to wait before ejecting shell after firing (for shotguns, grenade launchers etc.)
	public float ShellEjectSpin = 0.0f;					// amount of angular rotation of the shell upon spawn

	// sound
	public AudioClip SoundFire = null;							// sound to play upon firing
	public AudioClip SoundDryFire = null;						// out of ammo sound
	public AudioClip SoundReload = null;						// reload sound
	public Vector2 SoundFirePitch = new Vector2(1.0f, 1.0f);	// random pitch range for firing sound
	
	// ammo
	public int AmmoMaxCount = 10;						// the maximum amount of bullets that this weapon can hold
	public float AmmoReloadTime = 1.5f;					// for how long to prevent firing upon reload
	protected int m_AmmoCount = 0;						// current ammo count
	protected float m_NextAllowedReloadTime = 0.0f;		// the next time reloading will be allowed after having recently reloaded


	///////////////////////////////////////////////////////////
	// properties
	///////////////////////////////////////////////////////////
	public GameObject MuzzleFlash { get { return m_MuzzleFlash; } }
	public int AmmoCount { get { return m_AmmoCount; } set { m_AmmoCount = value; } }
	public float NextAllowedReloadTime { get { return m_NextAllowedReloadTime; } set { m_NextAllowedReloadTime = value; } }
	public float NextAllowedFireTime { get { return m_NextAllowedFireTime; } set { m_NextAllowedFireTime = value; } } 

	
	///////////////////////////////////////////////////////////
	// in 'Awake' we do things that need to be run once at the
	// very beginning. NOTE: as of Unity 4, gameobject hierarchy
	// can not be altered in 'Awake'
	///////////////////////////////////////////////////////////
	protected new void Awake()
	{

		base.Awake();

		m_Camera = transform.root.GetComponentInChildren<vp_FPSCamera>();
		if (m_Camera == null)
		{
			Debug.LogError("Camera is null.");
			return;
		}

		// reset the next allowed fire time
		m_NextAllowedFireTime = Time.time;
		m_NextAllowedReloadTime = Time.time;

		m_AmmoCount = AmmoMaxCount;

	}


	///////////////////////////////////////////////////////////
	// in 'Start' we do things that need to be run once at the
	// beginning, but potentially depend on all other scripts
	// first having run their 'Awake' calls.
	// NOTE: don't do anything here that depends on activity
	// in other 'Start' calls
	///////////////////////////////////////////////////////////
	protected new void Start()
	{

		base.Start();

		// instantiate muzzleflash
		if (MuzzleFlashPrefab != null)
		{
			m_MuzzleFlash = (GameObject)Object.Instantiate(MuzzleFlashPrefab,
															m_Camera.transform.position,
															m_Camera.transform.rotation);
			m_MuzzleFlash.name = transform.name + "MuzzleFlash";
			m_MuzzleFlash.transform.parent = m_Camera.transform;
		}

		// store a reference to the FPSWeapon
		m_Weapon = transform.GetComponent<vp_FPSWeapon>();

		// audio defaults
		audio.playOnAwake = false;
		audio.dopplerLevel = 0.0f;

		Refresh();

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	protected new void Update()
	{

		base.Update();

	}


	///////////////////////////////////////////////////////////
	// this method handles firing rate, recoil forces and fire
	// sounds for an FPSWeapon. you should call it from the
	// gameplay class handling player input when the player
	// presses the fire button.
	///////////////////////////////////////////////////////////
	public void Fire()
	{

		if (m_Weapon == null)
			return;

		if (Time.time < m_NextAllowedFireTime)
			return;

		if (AmmoCount < 1)
		{
			DryFire();
			return;
		}

		m_NextAllowedFireTime = Time.time + ProjectileFiringRate;
		m_NextAllowedReloadTime = Time.time;

		m_AmmoCount--;

		// return the weapon to its forward looking state by certain
		// position, rotation and velocity factors
		m_Weapon.ResetSprings(MotionPositionReset, MotionRotationReset,
							MotionPositionPause, MotionRotationPause);
									  
		// add a positional and angular force to the weapon for one frame
		m_Weapon.AddForce2(MotionPositionRecoil, MotionRotationRecoil);

		// play shoot sound
		if (audio != null)
		{
			audio.pitch = Random.Range(SoundFirePitch.x, SoundFirePitch.y);
			audio.clip = SoundFire;
			audio.Play();
			// LORE: we must use 'Play' rather than 'PlayOneShot' for the
			// AudioSource to be regarded as 'isPlaying' which is needed
			// for 'vp_FPSCamera:DeactivateWeaponWhenSilent'
		}
		
		// spawn projectiles
		for (int v = 0; v < ProjectileCount; v++)
		{
			if (ProjectilePrefab != null)
			{
				GameObject p = null;
				p = (GameObject)Object.Instantiate(ProjectilePrefab, m_Camera.transform.position, m_Camera.transform.rotation);
				p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);	// preset defined scale

				// apply conical spread as defined in preset
				p.transform.Rotate(0, 0, Random.Range(0, 360));									// first, rotate up to 360 degrees around z for circular spread
				p.transform.Rotate(0, Random.Range(-ProjectileSpread, ProjectileSpread), 0);		// then rotate around y with user defined deviation
			}
		}

		// spawn shell casing
		if (ShellPrefab != null)
		{
			if (ShellEjectDelay > 0.0f)
				vp_Timer.In(ShellEjectDelay, EjectShell);
			else
				EjectShell();
		}

		// show muzzle flash
		if (m_MuzzleFlashComponent != null)
			m_MuzzleFlashComponent.Shoot();

	}


	///////////////////////////////////////////////////////////
	// applies a scaled version of the recoil to the weapon to
	// signify pulling the trigger with no discharge. then plays
	// a dryfire sound. TIP: make 'MotionDryFireRecoil' about
	// -0.1 for a subtle out-of-ammo effect
	///////////////////////////////////////////////////////////
	public void DryFire()
	{

		// apply dryfire recoil
		m_Weapon.AddForce2(MotionPositionRecoil * MotionDryFireRecoil, MotionRotationRecoil * MotionDryFireRecoil);

		// play the dry fire sound and prevent further firing
		if (audio != null)
		{
			audio.pitch = 1.0f;
			audio.PlayOneShot(SoundDryFire);
			PreventFiring(1000000);
		}

	}


	///////////////////////////////////////////////////////////
	// fills the ammo parameter up to 'ammoCount', but only if
	// we are currently allowed to reload
	///////////////////////////////////////////////////////////
	public void Reload(int ammoCount)
	{

		if (Time.time < m_NextAllowedReloadTime)
			return;

		m_NextAllowedReloadTime = Time.time + AmmoReloadTime;

		m_AmmoCount += ammoCount;
		m_AmmoCount = Mathf.Min(AmmoMaxCount, m_AmmoCount);
		
		if (audio != null)
		{
			audio.pitch = 1.0f;
			audio.PlayOneShot(SoundReload);
			m_NextAllowedFireTime = Time.time + AmmoReloadTime + (AmmoReloadTime * 0.2f);
		}

	}


	///////////////////////////////////////////////////////////
	// spawns the 'ShellPrefab' gameobject and gives it a velocity
	///////////////////////////////////////////////////////////
	protected void EjectShell()
	{

		// spawn the shell
		GameObject s = null;
		s = (GameObject)Object.Instantiate(ShellPrefab, 
										m_Camera.transform.position + m_Camera.transform.TransformDirection(ShellEjectPosition),
										m_Camera.transform.rotation);
		s.transform.localScale = new Vector3(ShellScale, ShellScale, ShellScale);
		vp_Layer.Set(s.gameObject, vp_Layer.Debris);

		// send it flying
		if (s.rigidbody)
			s.rigidbody.AddForce((transform.TransformDirection(ShellEjectDirection) * ShellEjectVelocity), ForceMode.Impulse);

		// add random spin if user defined
		if (ShellEjectSpin > 0.0f)
		{
			if (Random.value > 0.5f)
				s.rigidbody.AddRelativeTorque(-Random.rotation.eulerAngles * ShellEjectSpin);
			else
				s.rigidbody.AddRelativeTorque(Random.rotation.eulerAngles * ShellEjectSpin);
		}

	}


	///////////////////////////////////////////////////////////
	// this method prevents the weapon from firing for 'seconds',
	// useful e.g. while switching weapons.
	///////////////////////////////////////////////////////////
	public void PreventFiring(float seconds)
	{

		m_NextAllowedFireTime = Time.time + seconds;

	}

	
	///////////////////////////////////////////////////////////
	// this method is called to reset various shooter settings,
	// typically after creating or loading a shooter
	///////////////////////////////////////////////////////////
	public override void Refresh()
	{

		// update muzzle flash position, scale and fadespeed from preset
		if (m_MuzzleFlash != null)
		{
			m_MuzzleFlash.transform.localPosition = MuzzleFlashPosition;
			m_MuzzleFlash.transform.localScale = MuzzleFlashScale;
			m_MuzzleFlashComponent = (vp_MuzzleFlash)m_MuzzleFlash.GetComponent("vp_MuzzleFlash");
			if (m_MuzzleFlashComponent != null)
				m_MuzzleFlashComponent.FadeSpeed = MuzzleFlashFadeSpeed;
		}

	}


}

	