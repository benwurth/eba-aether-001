/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSShooterEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the vp_FPSShooter class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_FPSShooter))]
public class vp_FPSShooterEditor : Editor
{

	// target component
	private vp_FPSShooter m_Component = null;

	// foldouts
	private static bool m_ProjectileFoldout;
	private static bool m_MotionFoldout;
	private static bool m_MuzzleFlashFoldout;
	private static bool m_ShellFoldout;
	private static bool m_AmmoFoldout;
	private static bool m_SoundFoldout;
	private static bool m_StateFoldout;
	private static bool m_PresetFoldout = true;

	// muzzleflash
	private bool m_MuzzleFlashVisible = false;		// display the muzzle flash in the editor?

	private static vp_ComponentPersister m_Persister = null;


	///////////////////////////////////////////////////////////
	// hooks up the FPSCamera object to the inspector target
	///////////////////////////////////////////////////////////
	void OnEnable()
	{

		m_Component = (vp_FPSShooter)target;

		if (m_Persister == null)
			m_Persister = new vp_ComponentPersister();
		m_Persister.Component = m_Component;
		m_Persister.IsActive = true;

		if (m_Component.DefaultState == null)
			m_Component.RefreshDefaultState();

	}


	///////////////////////////////////////////////////////////
	// disables the persister and removes its reference
	///////////////////////////////////////////////////////////
	void OnDestroy()
	{

		m_Persister.IsActive = false;

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	public override void OnInspectorGUI()
	{

		GUI.color = Color.white;

		string objectInfo = m_Component.gameObject.name;

		if (m_Component.gameObject.active)
			GUI.enabled = true;
		else
		{
			GUI.enabled = false;
			objectInfo += " (INACTIVE)";
		}

		if (!m_Component.gameObject.active)
		{
			GUI.enabled = true;
			return;
		}

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			// --- Projectile ---
			m_ProjectileFoldout = EditorGUILayout.Foldout(m_ProjectileFoldout, "Projectile");
			if (m_ProjectileFoldout)
			{

				m_Component.ProjectileFiringRate = EditorGUILayout.FloatField("Firing Rate", m_Component.ProjectileFiringRate);
				m_Component.ProjectilePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.ProjectilePrefab, typeof(GameObject), false);
				GUI.enabled = false;
				GUILayout.Label("Prefab should be a gameobject with a projectile\nlogic script added to it (such as vp_Bullet).", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.ProjectileScale = EditorGUILayout.Slider("Scale", m_Component.ProjectileScale, 0, 2);
				m_Component.ProjectileCount = EditorGUILayout.IntField("Count", m_Component.ProjectileCount);
				m_Component.ProjectileSpread = EditorGUILayout.Slider("Spread", m_Component.ProjectileSpread, 0, 360);

				vp_EditorGUIUtility.Separator();

			}

			// --- Motion ---

			m_MotionFoldout = EditorGUILayout.Foldout(m_MotionFoldout, "Motion");
			if (m_MotionFoldout)
			{

				m_Component.MotionPositionRecoil = EditorGUILayout.Vector3Field("Position Recoil", m_Component.MotionPositionRecoil);
				m_Component.MotionRotationRecoil = EditorGUILayout.Vector3Field("Rotation Recoil", m_Component.MotionRotationRecoil);
				GUI.enabled = false;
				GUILayout.Label("Recoil forces are added to the secondary\nposition and rotation springs of the weapon.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.MotionPositionReset = EditorGUILayout.Slider("Position Reset", m_Component.MotionPositionReset, 0, 1);
				m_Component.MotionRotationReset = EditorGUILayout.Slider("Rotation Reset", m_Component.MotionRotationReset, 0, 1);
				GUI.enabled = false;
				GUILayout.Label("Upon firing, primary position and rotation springs\nwill snap back to their rest state by this factor.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.MotionPositionPause = EditorGUILayout.Slider("Position Pause", m_Component.MotionPositionPause, 0, 5);
				m_Component.MotionRotationPause = EditorGUILayout.Slider("Rotation Pause", m_Component.MotionRotationPause, 0, 5);
				GUI.enabled = false;
				GUILayout.Label("Upon firing, primary spring forces will pause and\nease back in over this time interval in seconds.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.MotionDryFireRecoil = EditorGUILayout.Slider("Dry Fire Recoil", m_Component.MotionDryFireRecoil, -1, 1);
			
				vp_EditorGUIUtility.Separator();

			}

			// --- MuzzleFlash ---
			m_MuzzleFlashFoldout = EditorGUILayout.Foldout(m_MuzzleFlashFoldout, "Muzzle Flash");
			if (m_MuzzleFlashFoldout)
			{

				m_Component.MuzzleFlashPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.MuzzleFlashPrefab, typeof(GameObject), false);
				GUI.enabled = false;
				GUILayout.Label("Prefab should be a mesh with a Particles/Additive\nshader and a vp_MuzzleFlash script added to it.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				Vector3 currentPosition = m_Component.MuzzleFlashPosition;
				m_Component.MuzzleFlashPosition = EditorGUILayout.Vector3Field("Position", m_Component.MuzzleFlashPosition);
				Vector3 currentScale = m_Component.MuzzleFlashScale;
				m_Component.MuzzleFlashScale = EditorGUILayout.Vector3Field("Scale", m_Component.MuzzleFlashScale);
				m_Component.MuzzleFlashFadeSpeed = EditorGUILayout.Slider("Fade Speed", m_Component.MuzzleFlashFadeSpeed, 0.001f, 0.2f);
				if (!Application.isPlaying)
					GUI.enabled = false;
				bool currentMuzzleFlashVisible = m_MuzzleFlashVisible;
				m_MuzzleFlashVisible = EditorGUILayout.Toggle("Show Muzzle Fl.", m_MuzzleFlashVisible);
				if (Application.isPlaying)
				{
					if (m_Component.MuzzleFlashPosition != currentPosition ||
						m_Component.MuzzleFlashScale != currentScale)
						m_MuzzleFlashVisible = true;

					vp_MuzzleFlash mf = (vp_MuzzleFlash)m_Component.MuzzleFlash.GetComponent("vp_MuzzleFlash");
					if (mf != null)
						mf.ForceShow = currentMuzzleFlashVisible;

					GUI.enabled = false;
					GUILayout.Label("Set Muzzle Flash Z to about 0.5 to bring it into view.", vp_EditorGUIUtility.NoteStyle);
					GUI.enabled = true;
				}
				else
					GUILayout.Label("Muzzle Flash can be shown when the game is playing.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;

				vp_EditorGUIUtility.Separator();

			}

			// --- Shell ---
			m_ShellFoldout = EditorGUILayout.Foldout(m_ShellFoldout, "Shell");
			if (m_ShellFoldout)
			{

				m_Component.ShellPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.ShellPrefab, typeof(GameObject), false);
				GUI.enabled = false;
				GUILayout.Label("Prefab should be a mesh with a collider, a rigidbody\nand a vp_Shell script added to it.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.ShellScale = EditorGUILayout.Slider("Scale", m_Component.ShellScale, 0, 1);
				m_Component.ShellEjectPosition = EditorGUILayout.Vector3Field("Eject Position", m_Component.ShellEjectPosition);
				m_Component.ShellEjectDirection = EditorGUILayout.Vector3Field("Eject Direction", m_Component.ShellEjectDirection);
				m_Component.ShellEjectVelocity = EditorGUILayout.Slider("Eject Velocity", m_Component.ShellEjectVelocity, 0, 0.5f);
				m_Component.ShellEjectSpin = EditorGUILayout.Slider("Eject Spin", m_Component.ShellEjectSpin, 0, 1.0f);
				m_Component.ShellEjectDelay = Mathf.Abs(EditorGUILayout.FloatField("Eject Delay", m_Component.ShellEjectDelay));
				vp_EditorGUIUtility.Separator();

			}


			// --- Ammo ---
			m_AmmoFoldout = EditorGUILayout.Foldout(m_AmmoFoldout, "Ammo");
			if (m_AmmoFoldout)
			{

				m_Component.AmmoMaxCount = EditorGUILayout.IntField("Max Count", m_Component.AmmoMaxCount);
				m_Component.AmmoReloadTime = EditorGUILayout.FloatField("Reload Time", m_Component.AmmoReloadTime);
			
				//GUI.enabled = false;
				//GUILayout.Label("Prefab should be a mesh with a collider, a rigidbody\nand a vp_Shell script added to it.", vp_EditorGUIUtility.NoteStyle);
				//GUI.enabled = true;
				vp_EditorGUIUtility.Separator();

			}

			// --- Sound ---
			m_SoundFoldout = EditorGUILayout.Foldout(m_SoundFoldout, "Sound");
			if (m_SoundFoldout)
			{
				m_Component.SoundFire = (AudioClip)EditorGUILayout.ObjectField("Fire", m_Component.SoundFire, typeof(AudioClip), false);
				m_Component.SoundDryFire = (AudioClip)EditorGUILayout.ObjectField("Dry Fire", m_Component.SoundDryFire, typeof(AudioClip), false);
				m_Component.SoundReload = (AudioClip)EditorGUILayout.ObjectField("Reload", m_Component.SoundReload, typeof(AudioClip), false);
				m_Component.SoundFirePitch = EditorGUILayout.Vector2Field("Fire Pitch (Min:Max)", m_Component.SoundFirePitch);
				EditorGUILayout.MinMaxSlider(ref m_Component.SoundFirePitch.x, ref m_Component.SoundFirePitch.y, 0.5f, 1.5f);
				vp_EditorGUIUtility.Separator();
			}

		}
		else
			vp_PresetEditorGUIUtility.DefaultStateOverrideMessage();

		// --- State ---
		m_StateFoldout = vp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		// --- Preset ---
		m_PresetFoldout = vp_PresetEditorGUIUtility.PresetFoldout(m_PresetFoldout, m_Component);

		// --- Update ---
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

			// update the default state in order not to loose inspector tweaks
			// due to state switches during runtime
			if (Application.isPlaying)
				m_Component.RefreshDefaultState();

			if (m_Component.Persist)
				m_Persister.Persist();

			m_Component.Refresh();

		}

	}

	
}

