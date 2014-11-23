/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemoManager.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	utility class for Ultimate FPS Camera demo scripts
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_FPSDemoManager : vp_DemoManager
{

	public vp_FPSPlayer Player = null;

	private Vector3 m_UnFreezePosition = Vector3.zero;


	///////////////////////////////////////////////////////////
	// properties
	///////////////////////////////////////////////////////////
	public bool DrawCrosshair
	{
		get
		{
			vp_FPSCrosshair crosshair = (vp_FPSCrosshair)Player.GetComponent(typeof(vp_FPSCrosshair));
			if (crosshair == null)
				return false;
			return crosshair.enabled;
		}
		set
		{
			vp_FPSCrosshair crosshair = (vp_FPSCrosshair)Player.GetComponent(typeof(vp_FPSCrosshair));
			if (crosshair != null)
				crosshair.enabled = value;
		}
	}


	///////////////////////////////////////////////////////////
	// constructor
	///////////////////////////////////////////////////////////
	public vp_FPSDemoManager(vp_FPSPlayer player)
	{

		Player = player;
		// on small screen resolutions the editor preview screenshot
		// panel is minimized by default, otherwise expanded
		if (Screen.width < 1024)
			EditorPreviewSectionExpanded = false;
		
	}


	///////////////////////////////////////////////////////////
	// snaps the controller position and camera angle to a certain
	// coordinate and pitch/yaw, respectively
	///////////////////////////////////////////////////////////
	public void Teleport(Vector3 pos, Vector2 startAngle)
	{
		Player.Controller.SetPosition(pos);
		Player.Camera.SetAngle(startAngle.x, startAngle.y);
	}


	///////////////////////////////////////////////////////////
	// moves the controller to a coordinate in one frame and
	// freezes movement and optionally camera input
	///////////////////////////////////////////////////////////
	public void FreezePlayer(Vector3 pos, Vector2 startAngle, bool freezeCamera)
	{

		m_UnFreezePosition = Player.Controller.transform.position;
		Teleport(pos, startAngle);

		Player.Controller.SetState("Freeze", true);
		Player.Controller.Stop();

		if (freezeCamera)
			Player.Camera.SetState("Freeze", true);

	}

	public void FreezePlayer(Vector3 pos, Vector2 startAngle)
	{
		FreezePlayer(pos, startAngle, false);
	}


	///////////////////////////////////////////////////////////
	// unfreezes and restores a frozen player to the position
	// it had before getting frozen
	///////////////////////////////////////////////////////////
	public void UnFreezePlayer()
	{

		Player.Controller.transform.position = m_UnFreezePosition;
		m_UnFreezePosition = Vector3.zero;

		Player.Controller.SetState("Freeze", false);
		Player.Camera.SetState("Freeze", false);

	}


	///////////////////////////////////////////////////////////
	// sets a new default weapon preset. an optional shooter
	// preset may be provided. if the optional 'smoothFade' bool
	// is set to false, the new preset will snap in place.
	// NOTE: this is just demo walkthrough code. manipulating
	// the default state like this is no longer recommended.
	// use additional states instead.
	///////////////////////////////////////////////////////////
	public void SetWeaponPreset(TextAsset weaponPreset, TextAsset shooterPreset = null, bool smoothFade = true)
	{

		if (Player.CurrentWeapon == null)
			return;

		Player.CurrentWeapon.Load(weaponPreset);

		if (!smoothFade)
		{
			Player.CurrentWeapon.SnapSprings();
			Player.CurrentWeapon.SnapPivot();
			Player.CurrentWeapon.SnapZoom();
		}

		Player.CurrentWeapon.Refresh();

		if (shooterPreset != null)
		{
			if (Player.CurrentShooter != null)
				Player.CurrentShooter.Load(shooterPreset);
		}

	}


	///////////////////////////////////////////////////////////
	// helper method to refresh all default states. for use
	// after applying a preset to a vp_FPSComponent via script
	///////////////////////////////////////////////////////////
	public void RefreshDefaultState()
	{

		if (Player.Controller != null)
			Player.Controller.RefreshDefaultState();
		if (Player.Camera != null)
		{
			Player.Camera.RefreshDefaultState();
			if (Player.CurrentWeapon != null)
				Player.CurrentWeapon.RefreshDefaultState();
			if (Player.CurrentShooter != null)
				Player.CurrentShooter.RefreshDefaultState();
		}

	}

	
	///////////////////////////////////////////////////////////
	// various camera and weapon resets
	///////////////////////////////////////////////////////////
	protected override void Reset()
	{

		base.Reset();

		Player.AllowState("Zoom", true);
		Player.AllowState("Crouch", true);

		Player.ResetState();

		Player.Camera.SetWeaponLayer(vp_Layer.Weapon);

		Player.Camera.SetWeapon(0);
		Player.Camera.StopEarthQuake();
		Player.Camera.BobStepCallback = null;
		Player.Camera.SnapSprings();
		if (Player.Camera.CurrentWeapon != null)
		{
			Player.Camera.CurrentWeapon.SetPivotVisible(false);
			Player.Camera.CurrentWeapon.SnapSprings();
		}

		if (Screen.width < 1024)
			EditorPreviewSectionExpanded = false;
		else
			EditorPreviewSectionExpanded = true;

		if (m_UnFreezePosition != Vector3.zero)
			UnFreezePlayer();

	}


	///////////////////////////////////////////////////////////
	// forces a subtle camera shake on the camera
	///////////////////////////////////////////////////////////
	public void ForceCameraShake()
	{
		Player.Camera.ShakeSpeed = 0.0727273f;
		Player.Camera.ShakeAmplitude = new Vector3(-10, 10, 0);
	}


}

