/////////////////////////////////////////////////////////////////////////////////
//
//	vp_PresetEditorGUIUtility.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	editor gui helper methods for working with states & presets
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class vp_PresetEditorGUIUtility
{

	private static Color m_ColorTransparentWhite = new Color(1, 1, 1, 0.5f);


	///////////////////////////////////////////////////////////
	// draws a check box to toggle persist state ON / OFF for a
	// component. will be disabled if the 'Default' state has
	// been overridden
	///////////////////////////////////////////////////////////
	public static void PersistToggle(vp_ComponentPersister persister)
	{

		bool oldPersistState = persister.Component.Persist;
		GUI.color = Color.white;
		if (persister.Component.DefaultState.TextAsset != null)
		{
			persister.Component.Persist = false;
			GUI.color = m_ColorTransparentWhite;
		}

		persister.Component.Persist = vp_EditorGUIUtility.SmallToggle("Persist Play Mode Changes", persister.Component.Persist);
		if (persister.Component.DefaultState.TextAsset != null && persister.Component.Persist == true)
		{
			string s = "Can't Persist Play Mode Changes when the 'Default' state has been overridden with a text file preset.";
			if (!Application.isPlaying)
				s += "\n\nClick 'Unlock' to reenable inspector changes to this component.";
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Locked", s);
			persister.Component.Persist = false;
		}

		if (oldPersistState != persister.Component.Persist)
			persister.Persist();
		GUI.color = Color.white;
	}
	

	///////////////////////////////////////////////////////////
	// draws a foldout with buttons to load and save a preset
	///////////////////////////////////////////////////////////
	public static bool PresetFoldout(bool foldout, vp_Component component)
	{

		foldout = EditorGUILayout.Foldout(foldout, "Preset");
		if (foldout)
		{

			GUI.enabled = true;
			EditorGUILayout.BeginHorizontal();
			if (component.DefaultState.TextAsset != null)
				GUI.enabled = false;
			if (GUILayout.Button("Load"))
				ShowLoadDialog(component);
			GUI.enabled = true;
			if (GUILayout.Button("Save"))
				ShowSaveDialog(component, false);
			if (!Application.isPlaying)
				GUI.enabled = false;
			if (GUILayout.Button("Save Tweaks"))
				ShowSaveDifferenceDialog(component);
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

		}
		return foldout;

	}


	///////////////////////////////////////////////////////////
	// opens a dialog for loading presets
	///////////////////////////////////////////////////////////
	static private void ShowLoadDialog(vp_Component component)
	{

		string path = Application.dataPath.Replace("\\", "/");
		vp_FileDialog.Create(vp_FileDialog.Mode.Open, "Load Preset", path, delegate(string filename)
		{
			if (!vp_ComponentPreset.Load(component, filename))
				vp_FileDialog.Result = "Failed to load preset '" + vp_FileDialog.ExtractFilenameFromPath(filename) + "'.\n\nIs it the correct component type? (" + component.GetType().ToString() + ")";
			else
				EditorUtility.SetDirty(component);
		}, ".txt");

	}


	///////////////////////////////////////////////////////////
	// opens a dialog for saving presets
	///////////////////////////////////////////////////////////
	static private void ShowSaveDialog(vp_Component component)
	{
		ShowSaveDialog(component, false);
	}
	static private void ShowSaveDialog(vp_Component component, bool showLoadDialogAfterwards)
	{
		string path = Application.dataPath;

		vp_FileDialog.Create(vp_FileDialog.Mode.Save, "Save Preset", path, delegate(string filename)
		{
				vp_FileDialog.Result = vp_ComponentPreset.Save(component, filename);

			if (showLoadDialogAfterwards)
				ShowLoadDialog(component);

		}, ".txt");


	}


	///////////////////////////////////////////////////////////
	// opens a dialog for saving presets
	///////////////////////////////////////////////////////////
	static private void ShowSaveDifferenceDialog(vp_Component component)
	{

		string path = Application.dataPath;

		// LORE: in order not to overwrite existing values in a disk
		// preset, we'll load the disk preset before saving over it.
		// since the file dialog system will execute its callback
		// twice in case of an already existing file (and delete the
		// target file in the process) we need to store the preset
		// in memory outside of the callback and skip loading it on
		// the second iteration
		vp_ComponentPreset diskPreset = null;

		vp_FileDialog.Create(vp_FileDialog.Mode.Save, "Save Tweaks", path, delegate(string filename)
		{

			// only attempt to load the disk preset the first time
			// the callback is executed
			if (diskPreset == null)
			{
				diskPreset = new vp_ComponentPreset();
				// attempt to load target preset into memory, ignoring
				// load errors in the process
				bool logErrorState = vp_ComponentPreset.LogErrors;
				vp_ComponentPreset.LogErrors = false;
				diskPreset.LoadTextStream(filename);
				vp_ComponentPreset.LogErrors = logErrorState;
			}

			vp_FileDialog.Result = vp_ComponentPreset.SaveDifference(component.InitialState.Preset, component, filename, diskPreset);

		}, ".txt");

	}


	///////////////////////////////////////////////////////////
	// draws an info text replacing the component controls, in
	// case the component's 'Default' state has been overridden
	///////////////////////////////////////////////////////////
	public static void DefaultStateOverrideMessage()
	{

		GUI.enabled = false;
		GUILayout.Label("\n'Default' state has been overridden by a text preset. This\ncomponent can now be modified at runtime only, and changes\nwill revert when the app stops (presets can still be saved).\nUnlock the 'Default' state below to re-enable editing.\n", vp_EditorGUIUtility.NoteStyle);
		GUI.enabled = true;

	}


	///////////////////////////////////////////////////////////
	// draws a field allowing the user to create, reorganize,
	// name, assign presets to and delete states on a component
	///////////////////////////////////////////////////////////
	public static bool StateFoldout(bool foldout, vp_Component component, List<vp_StateInfo> stateList, vp_ComponentPersister persister = null)
	{

		bool before = foldout;
		foldout = EditorGUILayout.Foldout(foldout,
			(foldout && !Application.isPlaying) ? "State             Preset" : "States"
			);

		if (foldout != before)
			component.RefreshDefaultState();

		if (foldout)
		{
			
			for (int v = 0; v < stateList.Count; v++)
			{
				int s = v;
				if (!Application.isPlaying)
				{
					vp_PresetEditorGUIUtility.StateField(stateList[s], stateList);
				}
				else
				{
					vp_PresetEditorGUIUtility.RunTimeStateField(component, stateList[s], stateList);
				}
			}

			GUILayout.BeginHorizontal();
			if (!Application.isPlaying)
			{
				if (GUILayout.Button("Add State", GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
				{
					stateList.Add(new vp_StateInfo(component.GetType().Name, "New State", ""));
					component.RefreshDefaultState();
				}
			}
			else
			{
				GUI.color = Color.clear;
				GUILayout.Button("", GUILayout.MinWidth(36), GUILayout.MaxWidth(36));
				GUI.color = Color.white;
			}
			if(!Application.isPlaying)
				GUILayout.EndHorizontal();
			if (persister != null)
				vp_PresetEditorGUIUtility.PersistToggle(persister);
			if (Application.isPlaying)
				GUILayout.EndHorizontal();

			vp_EditorGUIUtility.Separator();
		
		}

		return foldout;

	}


	///////////////////////////////////////////////////////////
	// draws a button showing if a state is on or off, allowing
	// the user to toggle states at runtime. will also show
	// a text saying if the state is currently disallowed
	///////////////////////////////////////////////////////////
	public static void RunTimeStateField(vp_Component component, vp_StateInfo state, List<vp_StateInfo> stateList)
	{

		EditorGUILayout.BeginHorizontal();

		GUI.color = m_ColorTransparentWhite;
		if (!state.Enabled)
		{
			GUILayout.Space(20);
			GUI.enabled = true;
			GUILayout.Label((stateList.Count - stateList.IndexOf(state) - 1).ToString() + ":", vp_EditorGUIUtility.RightAlignedPathStyle, GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(state.Name, vp_EditorGUIUtility.CenteredBoxStyle, GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
			{
				vp_Component[] compos = component.gameObject.GetComponentsInChildren<vp_Component>();
				foreach (vp_Component c in compos)
				{
					c.StateManager.SetState(state.Name, true);
					c.Refresh();
				}
			}
			if (!state.Allowed)
				GUILayout.Label("(Disallowed)");
			GUILayout.EndHorizontal();
			GUI.color = m_ColorTransparentWhite;
		}
		else
		{
			GUILayout.Space(20);
			GUILayout.Label((stateList.Count - stateList.IndexOf(state) - 1).ToString() + ":", vp_EditorGUIUtility.RightAlignedPathStyle, GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
			if (GUILayout.Button(state.Name, vp_EditorGUIUtility.CenteredBoxStyleBold, GUILayout.MinWidth(90), GUILayout.MaxWidth(90)))
			{
				vp_Component[] compos = component.gameObject.GetComponentsInChildren<vp_Component>();
				foreach (vp_Component c in compos)
				{
					c.StateManager.SetState(state.Name, false);
					c.Refresh();
				}
			}
		}

		if (state.Name != "Default" && state.TextAsset == null)
			GUILayout.TextField("<- Warning: No preset!", vp_EditorGUIUtility.NoteStyle, GUILayout.MinWidth(100));

		EditorGUILayout.EndHorizontal();

	}


	///////////////////////////////////////////////////////////
	// draws a row displaying a preset state name, a path and
	// buttons for browsing the path + deleting the state
	///////////////////////////////////////////////////////////
	public static void StateField(vp_StateInfo state, List <vp_StateInfo> stateList)
	{
		
		GUI.enabled = !Application.isPlaying;	// only allow preset field interaction in 'stopped' mode

		EditorGUILayout.BeginHorizontal();

		string orig = state.Name;
		if (state.Name == "Default")
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(state.Name, GUILayout.MinWidth(90), GUILayout.MaxWidth(90));
			GUI.enabled = true;
		}
		else
		{
			state.Name = EditorGUILayout.TextField(state.Name, GUILayout.MinWidth(90), GUILayout.MaxWidth(90));
		}

		if ((orig != state.Name) && (state.Name == "Default"))
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "'Default' is a reserved state name.");
			state.Name = orig;
		}

		PresetField(state);

		if (state.Name == "Default")
		{
			if (state.TextAsset == null)
			{
				GUI.enabled = false;
				GUILayout.TextField("(Inspector)", vp_EditorGUIUtility.NoteStyle, GUILayout.MinWidth(60));
			}
			else
			{
				GUI.enabled = true;
				if (GUILayout.Button("Unlock", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(30), GUILayout.MinHeight(15)))
				{
					state.TextAsset = null;
				}
			}
		}
		else
		{
			if (stateList.IndexOf(state) == 0)
				GUI.enabled = false;

			GUI.SetNextControlName ("state");
			if (GUILayout.Button("^", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
			{
				int i = stateList.IndexOf(state);
				if (i != 0)
				{
					stateList.Remove(state);
					stateList.Insert(i - 1, state);
				}
				// focus this button to get rid of possible textfield focus,
				// or the textfields won't update properly when moving state
				GUI.FocusControl("state");
			}

			GUI.enabled = true;

			if (GUILayout.Button("X", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(15), GUILayout.MaxWidth(15), GUILayout.MinHeight(15)))
				stateList.Remove(state);

		}

		GUI.enabled = true;

		EditorGUILayout.EndHorizontal();

	}


	///////////////////////////////////////////////////////////
	// draws a slot to which the user can drag a preset TextAsset
	///////////////////////////////////////////////////////////
	private static void PresetField(vp_StateInfo state)
	{
		TextAsset orig = state.TextAsset;
		state.TextAsset = (TextAsset)EditorGUILayout.ObjectField(state.TextAsset, typeof(TextAsset), false);
		if (state.TextAsset != orig)
		{
			if (state.TextAsset != null)
			{
				if((vp_ComponentPreset.GetFileTypeFromAsset(state.TextAsset) == null ||
				vp_ComponentPreset.GetFileTypeFromAsset(state.TextAsset).Name != state.TypeName))
				{
					vp_MessageBox.Create(vp_MessageBox.Mode.OK, "Error", "Error: The file '" + state.TextAsset.name + " ' does not contain a preset of type '" + state.TypeName + "'.");
					state.TextAsset = orig;
					return;
				}
			}
		}
	}


	///////////////////////////////////////////////////////////
	// draws a disabled preset field for the built-in default
	// state, making the user aware of the state
	///////////////////////////////////////////////////////////
	public static void DefaultStateField(vp_StateInfo state)
	{

		EditorGUILayout.BeginHorizontal();

		GUI.enabled = false;
		GUILayout.Label("Default", "Textfield", GUILayout.MinWidth(90), GUILayout.MaxWidth(90));

		PresetField(state);

		GUILayout.TextField("(editor)", vp_EditorGUIUtility.NoteStyle, GUILayout.MinWidth(100));

		EditorGUILayout.Space();
		GUI.color = Color.clear;
		GUILayout.Button("...", GUILayout.MinWidth(24), GUILayout.MaxWidth(24));
		GUILayout.Button("X", GUILayout.MinWidth(24), GUILayout.MaxWidth(24));
		GUI.color = Color.white;
		GUI.enabled = true;

		EditorGUILayout.EndHorizontal();

	}


}

