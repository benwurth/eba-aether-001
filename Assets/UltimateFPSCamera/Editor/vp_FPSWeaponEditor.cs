/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSWeaponEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the vp_FPSWeapon class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_FPSWeapon))]
public class vp_FPSWeaponEditor : Editor
{

	// target component
	private vp_FPSWeapon m_Component = null;

	// weapon foldouts
	private static bool m_WeaponRenderingFoldout;
	private static bool m_WeaponRotationFoldout;
	private static bool m_WeaponPositionFoldout;
	private static bool m_WeaponShakeFoldout;
	private static bool m_WeaponBobFoldout;
	private static bool m_WeaponSoundFoldout;
	private static bool m_StateFoldout;
	private static bool m_PresetFoldout = true;

	// pivot
	private bool m_WeaponPivotVisible = false;

	private static vp_ComponentPersister m_Persister = null;


	///////////////////////////////////////////////////////////
	// hooks up the FPSCamera object to the inspector target
	///////////////////////////////////////////////////////////
	void OnEnable()
	{

		m_Component = (vp_FPSWeapon)target;

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

		GUILayout.Label(objectInfo);
		vp_EditorGUIUtility.Separator();

		if (!m_Component.gameObject.active)
		{
			GUI.enabled = true;
			return;
		}

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			// --- Rendering ---
			m_WeaponRenderingFoldout = EditorGUILayout.Foldout(m_WeaponRenderingFoldout, "Rendering");
			if (m_WeaponRenderingFoldout)
			{

				// weapon fov
				Vector2 fovDirty = new Vector2(0.0f, m_Component.RenderingFieldOfView);
				m_Component.RenderingFieldOfView = EditorGUILayout.Slider("Field of View", m_Component.RenderingFieldOfView, 1, 179);
				if (fovDirty != new Vector2(0.0f, m_Component.RenderingFieldOfView))
					m_Component.Zoom();
				m_Component.RenderingZoomDamping = EditorGUILayout.Slider("Zoom Damping", m_Component.RenderingZoomDamping, 0.1f, 5.0f);
				m_Component.RenderingClippingPlanes = EditorGUILayout.Vector2Field("Clipping Planes (Near:Far)", m_Component.RenderingClippingPlanes);
				GUI.enabled = false;
				GUILayout.Label("To add weapons, add child GameObjects to the\nFPSCamera transform and add FPSWeapon\nscripts to them. See the docs for more info.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;

				vp_EditorGUIUtility.Separator();

			}

			// --- Position ---
			m_WeaponPositionFoldout = EditorGUILayout.Foldout(m_WeaponPositionFoldout, "Position");
			if (m_WeaponPositionFoldout)
			{
				m_Component.PositionOffset = EditorGUILayout.Vector3Field("Offset", m_Component.PositionOffset);
				m_Component.PositionExitOffset = EditorGUILayout.Vector3Field("Exit Offset", m_Component.PositionExitOffset);
				Vector3 currentPivot = m_Component.PositionPivot;
				m_Component.PositionPivot = EditorGUILayout.Vector3Field("Pivot", m_Component.PositionPivot);
				m_Component.PositionPivotSpringStiffness = EditorGUILayout.Slider("Pivot Stiffness", m_Component.PositionPivotSpringStiffness, 0, 1);
				m_Component.PositionPivotSpringDamping = EditorGUILayout.Slider("Pivot Damping", m_Component.PositionPivotSpringDamping, 0, 1);

				if (!Application.isPlaying)
					GUI.enabled = false;
				bool currentPivotVisible = m_WeaponPivotVisible;
				m_WeaponPivotVisible = EditorGUILayout.Toggle("Show Pivot", m_WeaponPivotVisible);
				if (Application.isPlaying)
				{
					if (m_Component.PositionPivot != currentPivot)
					{
						m_Component.SnapPivot();
						m_WeaponPivotVisible = true;
					}
					if (currentPivotVisible != m_WeaponPivotVisible)
						m_Component.SetPivotVisible(m_WeaponPivotVisible);
					GUI.enabled = false;
					GUILayout.Label("Set Pivot Z to about -0.5 to bring it into view.", vp_EditorGUIUtility.NoteStyle);
					GUI.enabled = true;
				}
				else
					GUILayout.Label("Pivot can be shown when the game is playing.", vp_EditorGUIUtility.NoteStyle);

				GUI.enabled = true;

				m_Component.PositionSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.PositionSpringStiffness, 0, 1);
				m_Component.PositionSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.PositionSpringDamping, 0, 1);
				m_Component.PositionSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.PositionSpring2Stiffness, 0, 1);
				m_Component.PositionSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.PositionSpring2Damping, 0, 1);
				GUI.enabled = false;
				GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.PositionKneeling = EditorGUILayout.Slider("Kneeling", m_Component.PositionKneeling, 0, 1);
				m_Component.PositionFallRetract = EditorGUILayout.Slider("Fall Retract", m_Component.PositionFallRetract, 0, 10);
				m_Component.PositionWalkSlide = EditorGUILayout.Vector3Field("Walk Sliding", m_Component.PositionWalkSlide);
				m_Component.PositionInputVelocityScale = EditorGUILayout.Slider("Input Vel. Scale", m_Component.PositionInputVelocityScale, 0, 10);
				m_Component.PositionMaxInputVelocity = EditorGUILayout.FloatField("Max Input Vel.", m_Component.PositionMaxInputVelocity);

				vp_EditorGUIUtility.Separator();
			}

			// --- Rotation ---
			m_WeaponRotationFoldout = EditorGUILayout.Foldout(m_WeaponRotationFoldout, "Rotation");
			if (m_WeaponRotationFoldout)
			{
				m_Component.RotationOffset = EditorGUILayout.Vector3Field("Offset", m_Component.RotationOffset);
				m_Component.RotationExitOffset = EditorGUILayout.Vector3Field("Exit Offset", m_Component.RotationExitOffset);
				m_Component.RotationSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.RotationSpringStiffness, 0, 1);
				m_Component.RotationSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.RotationSpringDamping, 0, 1);
				m_Component.RotationSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.RotationSpring2Stiffness, 0, 1);
				m_Component.RotationSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.RotationSpring2Damping, 0, 1);
				GUI.enabled = false;
				GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.RotationLookSway = EditorGUILayout.Vector3Field("Look Sway", m_Component.RotationLookSway);
				m_Component.RotationStrafeSway = EditorGUILayout.Vector3Field("Strafe Sway", m_Component.RotationStrafeSway);
				m_Component.RotationFallSway = EditorGUILayout.Vector3Field("Fall Sway", m_Component.RotationFallSway);
				m_Component.RotationSlopeSway = EditorGUILayout.Slider("Slope Sway", m_Component.RotationSlopeSway, 0, 1);
				GUI.enabled = false;
				GUILayout.Label("SlopeSway multiplies FallSway when grounded\nand will take effect on slopes.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
				m_Component.RotationInputVelocityScale = EditorGUILayout.Slider("Input Rot. Scale", m_Component.RotationInputVelocityScale, 0, 10);
				m_Component.RotationMaxInputVelocity = EditorGUILayout.FloatField("Max Input Rot.", m_Component.RotationMaxInputVelocity);

				vp_EditorGUIUtility.Separator();
			}

			// --- Shake ---
			m_WeaponShakeFoldout = EditorGUILayout.Foldout(m_WeaponShakeFoldout, "Shake");
			if (m_WeaponShakeFoldout)
			{
				m_Component.ShakeSpeed = EditorGUILayout.Slider("Speed", m_Component.ShakeSpeed, 0, 1);
				m_Component.ShakeAmplitude = EditorGUILayout.Vector3Field("Amplitude", m_Component.ShakeAmplitude);

				vp_EditorGUIUtility.Separator();
			}

			// --- Bob ---
			m_WeaponBobFoldout = EditorGUILayout.Foldout(m_WeaponBobFoldout, "Bob");
			if (m_WeaponBobFoldout)
			{
				m_Component.BobRate = EditorGUILayout.Vector4Field("Rate", m_Component.BobRate);
				m_Component.BobAmplitude = EditorGUILayout.Vector4Field("Amplitude", m_Component.BobAmplitude);
				m_Component.BobInputVelocityScale = EditorGUILayout.Slider("Input Vel. Scale", m_Component.BobInputVelocityScale, 0, 10);
				m_Component.BobMaxInputVelocity = EditorGUILayout.FloatField("Max Input Vel.", m_Component.BobMaxInputVelocity);
				GUI.enabled = false;
				GUILayout.Label("XYZ is angular bob... W is position along the\nforward vector. X & Z rate should be (Y/2) for a\nclassic weapon bob.", vp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;

				vp_EditorGUIUtility.Separator();
			}

			// --- Sound ---
			m_WeaponSoundFoldout = EditorGUILayout.Foldout(m_WeaponSoundFoldout, "Sound");
			if (m_WeaponSoundFoldout)
			{
				m_Component.SoundWield = (AudioClip)EditorGUILayout.ObjectField("Wield", m_Component.SoundWield, typeof(AudioClip), false);
				m_Component.SoundUnWield = (AudioClip)EditorGUILayout.ObjectField("Unwield", m_Component.SoundUnWield, typeof(AudioClip), false);

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

