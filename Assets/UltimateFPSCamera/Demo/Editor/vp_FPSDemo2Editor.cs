/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemo2Editor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the second demo class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(vp_FPSDemo2))]
public class vp_FPSDemo2Editor : Editor
{

	// target component
	private vp_FPSDemo2 m_Component = null;

	// foldouts
	private static bool m_ImagesFoldout;


	///////////////////////////////////////////////////////////
	// hooks up the vp_FPSDemo2 object as the inspector target
	///////////////////////////////////////////////////////////
	void OnEnable()
	{

		m_Component = (vp_FPSDemo2)target;

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	public override void OnInspectorGUI()
	{

		GUI.color = Color.white;

		GUI.enabled = false;
		GUILayout.Label("\nNOTE: This script is specific to the demo\nwalkthrough. You probably want to instead\nstudy the 'FPSPlayer' prefab in 'DemoScene2'.\n", vp_EditorGUIUtility.NoteStyle);
		GUI.enabled = true;

		m_Component.PlayerGameObject = (GameObject)EditorGUILayout.ObjectField("Player Object", m_Component.PlayerGameObject, typeof(GameObject), true);

		// --- Images ---
		m_ImagesFoldout = EditorGUILayout.Foldout(m_ImagesFoldout, "Images");
		if (m_ImagesFoldout)
		{

			m_Component.ImageLeftArrow = (Texture)EditorGUILayout.ObjectField("LeftArrow", m_Component.ImageLeftArrow, typeof(Texture), false);
			m_Component.ImageRightArrow = (Texture)EditorGUILayout.ObjectField("RightArrow", m_Component.ImageRightArrow, typeof(Texture), false);
			m_Component.ImageCheckmark = (Texture)EditorGUILayout.ObjectField("Checkmark", m_Component.ImageCheckmark, typeof(Texture), false);
			m_Component.ImagePresetDialogs = (Texture)EditorGUILayout.ObjectField("PresetDialogs", m_Component.ImagePresetDialogs, typeof(Texture), false);
			m_Component.ImageShooter = (Texture)EditorGUILayout.ObjectField("Shooter", m_Component.ImageShooter, typeof(Texture), false);
			m_Component.ImageAllParams = (Texture)EditorGUILayout.ObjectField("AllParams", m_Component.ImageAllParams, typeof(Texture), false);
			
		}
		
	}

	
}

