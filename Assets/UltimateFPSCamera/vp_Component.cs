/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Component.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	base class for extended MonoBehaviours. these components have
//					the added functionality of support for the VisionPunk Preset
//					and StateManager systems, along with an 'Init' method that gets
//					executed once after all 'Start' calls. it also adds an optional,
//					more classic delta time measurement where '1' equals '60 FPS'
//
//					NOTE: it is not recommended to modify this class since many other
//					classes depend on it
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class vp_Component : MonoBehaviour
{

	public bool Persist = false;

	protected vp_StateManager m_StateManager = null;

	public List<vp_StateInfo> States = new List<vp_StateInfo>();	// list of state presets for this component
	protected vp_StateInfo m_DefaultState = null;

	protected bool m_Initialized = false;

#if UNITY_EDITOR
	// initial state is needed because we refresh default state upon
	// inspector changes, and this will mess with our ability to save
	// difference presets (save tweaks) by breaking the compare values.
	// on the other hand we need to be able to refresh default state in
	// order not to loose inspector changes (i.e. if we accidentally
	// press zoom or crouch when tweaking components in the inspector)
	protected vp_StateInfo m_InitialState = null;		// used at editor runtime only
	public vp_StateInfo InitialState { get { return m_InitialState; } }
#endif


	//////////////////////////////////////////////////////////
	// properties
	//////////////////////////////////////////////////////////
	public vp_StateManager StateManager { get { return m_StateManager; } }
	public vp_StateInfo DefaultState { get { return m_DefaultState; } }
	public float Delta { get { return (Time.deltaTime * 60.0f); } }		// NOTE: you may want to alter this depending on your
																		// application target framerate or other preference

	//////////////////////////////////////////////////////////
	// in 'Awake' we do things that need to be run once at the
	// very beginning. NOTE: 1) this method must be run using
	// 'base.Awake();' on the first line of the 'Awake' method
	// in any derived class. 2) keep in mind that as of Unity 4,
	// gameobject hierarchy can not be altered in 'Awake'
	//////////////////////////////////////////////////////////
	public void Awake()
	{

		m_StateManager = new vp_StateManager(this, States);
		StateManager.SetState("Default", enabled);

	}


	//////////////////////////////////////////////////////////
	// in 'Start' we do things that need to be run once at the
	// beginning, but potentially depend on all other scripts
	// first having run their 'Awake' calls.
	// NOTE: 1) don't do anything here that depends on activity
	// in other 'Start' calls. 2) if adding code here, remember
	// to call it using 'base.Start();' on the first line of
	// the 'Start' method in the derived classes
	//////////////////////////////////////////////////////////
	public void Start()
	{
	}


	//////////////////////////////////////////////////////////
	// in 'Init' we do things that must be run once at the
	// beginning - but only after all other components have
	// run their 'Start' calls. this method is called once
	// in the first 'Update'. NOTE: 1) unlike the standard Unity
	// methods, this is a virtual method. 2) if adding code here,
	// remember to call it using 'base.Init();' on the first
	// line of the 'Init' method in the derived classes
	//////////////////////////////////////////////////////////
	protected virtual void Init()
	{
	}


	//////////////////////////////////////////////////////////
	// NOTE: to provide the 'Init' functionality, this method
	// must be called using 'base.Update();' on the first line
	// of the 'Update' method in the derived class
	//////////////////////////////////////////////////////////
	protected void Update()
	{

		// initialize if needed. NOTE: this will run the inherited
		// 'Init' method (and if non-present: the one above)
		if (!m_Initialized)
		{
			Init();
			m_Initialized = true;
		}

	}


	///////////////////////////////////////////////////////////
	// sets 'state' true / false on the component and refreshes it
	///////////////////////////////////////////////////////////
	public void SetState(string state, bool enabled)
	{

		m_StateManager.SetState(state, enabled);
		Refresh();

	}


	///////////////////////////////////////////////////////////
	// asks statemanager to disable all states except the default
	// state, and enables the default state. then refreshes.
	///////////////////////////////////////////////////////////
	public void ResetState()
	{

		m_StateManager.Reset();
		Refresh();

	}


	///////////////////////////////////////////////////////////
	// allows or disallows 'state' on this object
	///////////////////////////////////////////////////////////
	public virtual void AllowState(string state, bool isAllowed)
	{

		m_StateManager.AllowState(state, isAllowed);
		Refresh();

	}


	///////////////////////////////////////////////////////////
	// allows or disallows 'state' on this object, then scans
	// the underlying hierarchy for vp_Components and does the
	// same on every object found
	///////////////////////////////////////////////////////////
	public void AllowStateRecursively(string state, bool isAllowed)
	{

		AllowState(state, isAllowed);

		Component[] components;
		components = GetComponentsInChildren<vp_Component>();
		foreach (Component c in components)
		{
			vp_Component vc = (vp_Component)c;
			vc.AllowState(state, isAllowed);
		}

	}


	///////////////////////////////////////////////////////////
	// returns true if the state associated with the passed
	// string is on the list & enabled, otherwise returns null
	///////////////////////////////////////////////////////////
	public bool StateEnabled(string stateName)
	{

		return m_StateManager.IsEnabled(stateName);

	}


	///////////////////////////////////////////////////////////
	// sees if the component has a default state. if so, makes
	// sure it's in index zero of the list, if not, creates it.
	///////////////////////////////////////////////////////////
	public void RefreshDefaultState()
	{

		vp_StateInfo defaultState = null;

		if (States.Count == 0)
		{
			// there are no states, so create default state
			defaultState = new vp_StateInfo(GetType().Name, "Default", null);
			States.Add(defaultState);
		}
		else
		{
			for (int v = States.Count - 1; v > -1; v--)
			{
				if (States[v].Name == "Default")
				{
					// found default state, make sure it's in the back
					defaultState = States[v];
					States.Remove(defaultState);
					States.Add(defaultState);
				}
			}
			if (defaultState == null)
			{
				// there are states, but no default state so create it
				defaultState = new vp_StateInfo(GetType().Name, "Default", null);
				States.Add(defaultState);
			}
		}

		if (defaultState.Preset == null || defaultState.Preset.ComponentType == null)
			defaultState.Preset = new vp_ComponentPreset();

		if(defaultState.TextAsset == null)
			defaultState.Preset.InitFromComponent(this);

		defaultState.Enabled = true;	// default state is always enabled

		m_DefaultState = defaultState;

	}


	///////////////////////////////////////////////////////////
	// copies component values into the default state's preset.
	// if needed, creates & adds default state to the state list.
	// to be called on app startup and statemanager recombine
	///////////////////////////////////////////////////////////
#if UNITY_EDITOR
	public void RefreshInitialState()
	{

		m_InitialState = null;
		m_InitialState = new vp_StateInfo(GetType().Name, "Internal_Initial", null);
		m_InitialState.Preset = new vp_ComponentPreset();
		m_InitialState.Preset.InitFromComponent(this);

	}
#endif



	///////////////////////////////////////////////////////////
	// helper method to apply a preset from memory and refresh
	// settings. for cleaner syntax
	///////////////////////////////////////////////////////////
	public void ApplyPreset(vp_ComponentPreset preset)
	{
		vp_ComponentPreset.Apply(this, preset);
		RefreshDefaultState();
		Refresh();
	}


	///////////////////////////////////////////////////////////
	// helper method to load a preset from the resources folder,
	// and refresh settings. for cleaner syntax
	///////////////////////////////////////////////////////////
	public vp_ComponentPreset Load(string path)
	{
		vp_ComponentPreset preset = vp_ComponentPreset.LoadFromResources(this, path);
		RefreshDefaultState();
		Refresh();
		return preset;
	}


	///////////////////////////////////////////////////////////
	// helper method to load a preset from a text asset,
	// and refresh settings. for cleaner syntax
	///////////////////////////////////////////////////////////
	public vp_ComponentPreset Load(TextAsset asset)
	{
		vp_ComponentPreset preset = vp_ComponentPreset.LoadFromTextAsset(this, asset);
		RefreshDefaultState();
		Refresh();
		return preset;
	}


	///////////////////////////////////////////////////////////
	// to be overridden in inherited classes, for resetting
	// various important variables on the component
	///////////////////////////////////////////////////////////
	public virtual void Refresh()
	{
	}


}


