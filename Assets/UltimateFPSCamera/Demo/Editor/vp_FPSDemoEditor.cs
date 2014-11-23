/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemoEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the first demo class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(vp_FPSDemo))]
public class vp_FPSDemoEditor : Editor
{

	// target component
	private vp_FPSDemo m_Component = null;

	// foldouts
	private static bool m_ImagesFoldout;
	private static bool m_SoundsFoldout;
	private static bool m_PresetsFoldout;


	///////////////////////////////////////////////////////////
	// hooks up the vp_FPSDemo object as the inspector target
	///////////////////////////////////////////////////////////
	void OnEnable()
	{

		m_Component = (vp_FPSDemo)target;

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

			m_Component.m_ImageEditorPreview = (Texture)EditorGUILayout.ObjectField("EditorPreview", m_Component.m_ImageEditorPreview, typeof(Texture), false);
			m_Component.m_ImageEditorPreviewShow = (Texture)EditorGUILayout.ObjectField("EditorPreviewShow", m_Component.m_ImageEditorPreviewShow, typeof(Texture), false);
			m_Component.m_ImageCameraMouse = (Texture)EditorGUILayout.ObjectField("CameraMouse", m_Component.m_ImageCameraMouse, typeof(Texture), false);
			m_Component.m_ImageWeaponPosition = (Texture)EditorGUILayout.ObjectField("WeaponPosition", m_Component.m_ImageWeaponPosition, typeof(Texture), false);
			m_Component.m_ImageWeaponPerspective = (Texture)EditorGUILayout.ObjectField("WeaponPerspective", m_Component.m_ImageWeaponPerspective, typeof(Texture), false);
			m_Component.m_ImageWeaponPivot = (Texture)EditorGUILayout.ObjectField("WeaponPivot", m_Component.m_ImageWeaponPivot, typeof(Texture), false);
			m_Component.m_ImageEditorScreenshot = (Texture)EditorGUILayout.ObjectField("EditorScreenshot", m_Component.m_ImageEditorScreenshot, typeof(Texture), false);
			m_Component.m_ImageLeftArrow = (Texture)EditorGUILayout.ObjectField("LeftArrow", m_Component.m_ImageLeftArrow, typeof(Texture), false);
			m_Component.m_ImageRightArrow = (Texture)EditorGUILayout.ObjectField("RightArrow", m_Component.m_ImageRightArrow, typeof(Texture), false);
			m_Component.m_ImageCheckmark = (Texture)EditorGUILayout.ObjectField("Checkmark", m_Component.m_ImageCheckmark, typeof(Texture), false);
			m_Component.m_ImageLeftPointer = (Texture)EditorGUILayout.ObjectField("LeftPointer", m_Component.m_ImageLeftPointer, typeof(Texture), false);
			m_Component.m_ImageRightPointer = (Texture)EditorGUILayout.ObjectField("RightPointer", m_Component.m_ImageRightPointer, typeof(Texture), false);
			m_Component.m_ImageUpPointer = (Texture)EditorGUILayout.ObjectField("UpPointer", m_Component.m_ImageUpPointer, typeof(Texture), false);
			m_Component.m_ImageCrosshair = (Texture)EditorGUILayout.ObjectField("Crosshair", m_Component.m_ImageCrosshair, typeof(Texture), false);
			m_Component.m_ImageFullScreen = (Texture)EditorGUILayout.ObjectField("FullScreen", m_Component.m_ImageFullScreen, typeof(Texture), false);

		}

		// --- Sounds ---
		m_SoundsFoldout = EditorGUILayout.Foldout(m_SoundsFoldout, "Sounds");
		if (m_SoundsFoldout)
		{
			m_Component.m_EarthquakeSound = (AudioClip)EditorGUILayout.ObjectField("EarthQuake", m_Component.m_EarthquakeSound, typeof(AudioClip), false);
			m_Component.m_ExplosionSound = (AudioClip)EditorGUILayout.ObjectField("Explosion", m_Component.m_ExplosionSound, typeof(AudioClip), false);
			m_Component.m_StompSound = (AudioClip)EditorGUILayout.ObjectField("Stomp", m_Component.m_StompSound, typeof(AudioClip), false);
		}

		// --- Presets ---
		m_PresetsFoldout = EditorGUILayout.Foldout(m_PresetsFoldout, "Presets");
		if (m_PresetsFoldout)
		{

			PresetField("ArtilleryCamera", ref m_Component.ArtilleryCamera);
			PresetField("ArtilleryController", ref m_Component.ArtilleryController);
			PresetField("AstronautCamera", ref m_Component.AstronautCamera);
			PresetField("AstronautController", ref m_Component.AstronautController);
			PresetField("CowboyCamera", ref m_Component.CowboyCamera);
			PresetField("CowboyController", ref m_Component.CowboyController);
			PresetField("CowboyWeapon", ref m_Component.CowboyWeapon);
			PresetField("CowboyShooter", ref m_Component.CowboyShooter);
			PresetField("CrouchController", ref m_Component.CrouchController);
			PresetField("DrunkCamera", ref m_Component.DrunkCamera);
			PresetField("DrunkController", ref m_Component.DrunkController);
			PresetField("DefaultCamera", ref m_Component.DefaultCamera);
			PresetField("DefaultController", ref m_Component.DefaultController);
			PresetField("DefaultWeapon", ref m_Component.DefaultWeapon);
			PresetField("ImmobileController", ref m_Component.ImmobileController);
			PresetField("ImmobileCamera", ref m_Component.ImmobileCamera);
			PresetField("MaceCamera", ref m_Component.MaceCamera);
			PresetField("MaceWeapon", ref m_Component.MaceWeapon);
			PresetField("MafiaCamera", ref m_Component.MafiaCamera);
			PresetField("MafiaWeapon", ref m_Component.MafiaWeapon);
			PresetField("MafiaShooter", ref m_Component.MafiaShooter);
			PresetField("MechCamera", ref m_Component.MechCamera);
			PresetField("MechController", ref m_Component.MechController);
			PresetField("MechWeapon", ref m_Component.MechWeapon);
			PresetField("MechShooter", ref m_Component.MechShooter);
			PresetField("ModernCamera", ref m_Component.ModernCamera);
			PresetField("ModernController", ref m_Component.ModernController);
			PresetField("ModernWeapon", ref m_Component.ModernWeapon);
			PresetField("ModernShooter", ref m_Component.ModernShooter);
			PresetField("MouseLowSensCamera", ref m_Component.MouseLowSensCamera);
			PresetField("MouseRawUnityCamera", ref m_Component.MouseRawUnityCamera);
			PresetField("MouseSmoothingCamera", ref m_Component.MouseSmoothingCamera);
			PresetField("OldSchoolCamera", ref m_Component.OldSchoolCamera);
			PresetField("OldSchoolController", ref m_Component.OldSchoolController);
			PresetField("OldSchoolWeapon", ref m_Component.OldSchoolWeapon);
			PresetField("OldSchoolShooter", ref m_Component.OldSchoolShooter);
			PresetField("Persp1999Camera", ref m_Component.Persp1999Camera);
			PresetField("Persp1999Weapon", ref m_Component.Persp1999Weapon);
			PresetField("PerspModernCamera", ref m_Component.PerspModernCamera);
			PresetField("PerspModernWeapon", ref m_Component.PerspModernWeapon);
			PresetField("PerspOldCamera", ref m_Component.PerspOldCamera);
			PresetField("PerspOldWeapon", ref m_Component.PerspOldWeapon);
			PresetField("PivotChestWeapon", ref m_Component.PivotChestWeapon);
			PresetField("PivotElbowWeapon", ref m_Component.PivotElbowWeapon);
			PresetField("PivotMuzzleWeapon", ref m_Component.PivotMuzzleWeapon);
			PresetField("PivotWristWeapon", ref m_Component.PivotWristWeapon);
			PresetField("SmackController", ref m_Component.SmackController);
			PresetField("SniperCamera", ref m_Component.SniperCamera);
			PresetField("SniperWeapon", ref m_Component.SniperWeapon);
			PresetField("SniperShooter", ref m_Component.SniperShooter);
			PresetField("StompingCamera", ref m_Component.StompingCamera);
			PresetField("SystemOFFCamera", ref m_Component.SystemOFFCamera);
			PresetField("SystemOFFController", ref m_Component.SystemOFFController);
			PresetField("SystemOFFShooter", ref m_Component.SystemOFFShooter);
			PresetField("SystemOFFWeapon", ref m_Component.SystemOFFWeapon);
			PresetField("SystemOFFWeaponGlideIn", ref m_Component.SystemOFFWeaponGlideIn);
			PresetField("TurretCamera", ref m_Component.TurretCamera);
			PresetField("TurretWeapon", ref m_Component.TurretWeapon);
			PresetField("TurretShooter", ref m_Component.TurretShooter);
			PresetField("WallFacingCamera", ref m_Component.WallFacingCamera);
			PresetField("WallFacingWeapon", ref m_Component.WallFacingWeapon);
			
		}

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	private void PresetField(string name, ref TextAsset asset)
	{

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(name);
		asset = (TextAsset)EditorGUILayout.ObjectField(asset, typeof(TextAsset), false);
		EditorGUILayout.EndHorizontal();

	}

	
}

