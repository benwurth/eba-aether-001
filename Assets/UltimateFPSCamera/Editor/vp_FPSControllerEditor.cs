/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSControllerEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the vp_FPSController class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_FPSController))]
public class vp_FPSControllerEditor : Editor
{

	// target component
	private vp_FPSController m_Component;

	// foldouts
	private static bool m_MotorFoldout;
	private static bool m_PhysicsFoldout;
	private static bool m_StateFoldout;
	private static bool m_PresetFoldout = true;
	
	private static vp_ComponentPersister m_Persister = null;


	///////////////////////////////////////////////////////////
	// hooks up the FPSController object as the inspector target
	///////////////////////////////////////////////////////////
	void OnEnable()
	{

		m_Component = (vp_FPSController)target;

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

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			// --- Motor ---
			m_MotorFoldout = EditorGUILayout.Foldout(m_MotorFoldout, "Motor");

			if (m_MotorFoldout)
			{

				m_Component.MotorAcceleration = EditorGUILayout.Slider("Acceleration", m_Component.MotorAcceleration, 0, 1);
				m_Component.MotorDamping = EditorGUILayout.Slider("Damping", m_Component.MotorDamping, 0, 1);
				m_Component.MotorJumpForce = EditorGUILayout.Slider("Jump Force", m_Component.MotorJumpForce, 0, 10);
				m_Component.MotorAirSpeed = EditorGUILayout.Slider("Air Speed", m_Component.MotorAirSpeed, 0, 1);
				m_Component.MotorSlopeSpeedUp = EditorGUILayout.Slider("Slope Speed Up", m_Component.MotorSlopeSpeedUp, 0, 2);
				m_Component.MotorSlopeSpeedDown = EditorGUILayout.Slider("Slope Sp. Down", m_Component.MotorSlopeSpeedDown, 0, 2);

				vp_EditorGUIUtility.Separator();

			}

			// --- Physics ---
			m_PhysicsFoldout = EditorGUILayout.Foldout(m_PhysicsFoldout, "Physics");
			if (m_PhysicsFoldout)
			{

				m_Component.PhysicsForceDamping = EditorGUILayout.Slider("Force Damping", m_Component.PhysicsForceDamping, 0, 1);
				m_Component.PhysicsPushForce = EditorGUILayout.Slider("Push Force", m_Component.PhysicsPushForce, 0, 100);
				m_Component.PhysicsGravityModifier = EditorGUILayout.Slider("Gravity Modifier", m_Component.PhysicsGravityModifier, 0, 1);
				m_Component.PhysicsWallBounce = EditorGUILayout.Slider("Wall Bounce", m_Component.PhysicsWallBounce, 0, 1);

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
			if(Application.isPlaying)
				m_Component.RefreshDefaultState();

			if (m_Component.Persist)
				m_Persister.Persist();

		}

	}
	

	
}

