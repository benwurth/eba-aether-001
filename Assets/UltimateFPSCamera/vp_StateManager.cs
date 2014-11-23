/////////////////////////////////////////////////////////////////////////////////
//
//	vp_StateManager.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	manages a list of states and corresponding presets for a
//					component. loads the presets into memory on startup, and
//					allows applying them from memory to the component.
//
//					states are enabled in a layered manner, that is: the
//					default state is always alive, and any enabled states are
//					applied on top of it in the order of which they were enabled.
//
//					this class doesn't store any preset data between sessions.
//					it is merely used to manipulate the component using a list
//					of states that is sent along at startup. it is very silent
//					and forgiving. it won't complain if a state isn't found,
//					and it will ignore empty paths.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Reflection;
using System.Collections.Generic;


public class vp_StateManager
{

	private vp_Component m_Component = null;		// component to manipulate
	private List<vp_StateInfo> m_States = null;		// list sent from the component at startup

	private string m_AppNotPlayingMessage = "Error: StateManager can only be accessed when application is playing.";
	private string m_DefaultStateDisableMessage = "Warning: The 'Default' state cannot be disabled.";


	///////////////////////////////////////////////////////////
	// constructor. creates a default state using the values
	// currently set on the component, then loads the presets
	// referenced in the passed list of states.
	///////////////////////////////////////////////////////////
	public vp_StateManager(vp_Component component, List<vp_StateInfo> states)
	{

		m_States = states;
		m_Component = component;

		// create default state and add it to the list
		m_Component.RefreshDefaultState();

		// refresh the initial state, needed for being able to save
		// partial presets in the editor
#if UNITY_EDITOR
		m_Component.RefreshInitialState();
#endif

		// load up the preset of each user assigned state
		foreach (vp_StateInfo s in m_States)
		{
			if(s.Preset == null)
				s.Preset = new vp_ComponentPreset();
			if (s.TextAsset != null)
				s.Preset.LoadFromTextAsset(s.TextAsset);
		}

	}


	///////////////////////////////////////////////////////////
	// if the passed state exists in the state list, enables or
	// disables it and recombines all states in the new order
	///////////////////////////////////////////////////////////
	public void SetState(string state, bool isEnabled)
	{

		if (!Application.isPlaying)
		{
			Debug.LogError(m_AppNotPlayingMessage);
			return;
		}

		// disallow manually disabling the default state
		if (state == "Default" && isEnabled == false)
		{
			Debug.LogWarning(m_DefaultStateDisableMessage);
			return;
		}

		// enable or disable the passed state if we can find it and
		// it is currently allowed
		bool recombine = false;
		foreach(vp_StateInfo s in (m_States))
		{
			if (s.Name == state && s.Allowed)
			{
				recombine = true;
				s.Enabled = isEnabled;
			}
		}

		// apply all states in the new order
		if (recombine)
			CombineStates();

	}


	///////////////////////////////////////////////////////////
	// if the passed state exists in the state list, allows or
	// disallows it and recombines all states in the new order
	///////////////////////////////////////////////////////////
	public void AllowState(string state, bool isAllowed)
	{

		if (!Application.isPlaying)
		{
			Debug.LogError(m_AppNotPlayingMessage);
			return;
		}

		// disallow manually disabling the default state
		if (state == "Default" && isAllowed == false)
		{
			Debug.LogWarning(m_DefaultStateDisableMessage);
			return;
		}

		// allow or disallow the passed state if we can find it
		bool recombine = false;
		foreach (vp_StateInfo s in (m_States))
		{
			if (s.Name == state)
			{
				s.Allowed = isAllowed;
				if (s.Enabled && !s.Allowed)
				{
					s.Enabled = false;
					recombine = true;
				}
			}
		}

		// apply all states in the new order
		if (recombine)
			CombineStates();

	}
	
	
	///////////////////////////////////////////////////////////
	// disables all states except the default state, and enables
	// the default state
	///////////////////////////////////////////////////////////
	public void Reset()
	{

		if (!Application.isPlaying)
		{
			Debug.LogError(m_AppNotPlayingMessage);
			return;
		}

		foreach (vp_StateInfo s in (m_States))
		{
			if (s.Name != "Default")
				s.Enabled = false;
			else
				s.Enabled = true;
		}

		// apply all states in the new order
		CombineStates();

	}

	
	///////////////////////////////////////////////////////////
	// combines all the states in the list to a temporary state,
	// and sets it on the component
	///////////////////////////////////////////////////////////
	private void CombineStates()
	{

		// go backwards so default state is applied first
		for (int v = m_States.Count - 1; v > -1; v--)
		{
			if (m_States[v].Enabled)
			{
				if (m_States[v].Preset != null)
				{
					vp_ComponentPreset.Apply(m_Component, m_States[v].Preset);
				}
			}
		}

#if UNITY_EDITOR
		m_Component.RefreshInitialState();
#endif

	}
	

	///////////////////////////////////////////////////////////
	// moves a state to the top of the list (index 0)
	///////////////////////////////////////////////////////////
	public void BringToFront(vp_StateInfo state)
	{

		m_States.Remove(state);
		m_States.Insert(0, state);

	}


	///////////////////////////////////////////////////////////
	// returns true if the state associated with the passed
	// string is on the list & enabled, otherwise returns false
	///////////////////////////////////////////////////////////
	public bool IsEnabled(string stateName)
	{

		if (!Application.isPlaying)
		{
			Debug.LogError(m_AppNotPlayingMessage);
			return false;
		}

		foreach (vp_StateInfo s in m_States)
		{
			if (s.Name == stateName)
				return s.Enabled;
		}
		return false;

	}


	///////////////////////////////////////////////////////////
	// returns true if the state associated with the passed
	// string is on the list & allowed, otherwise returns false
	///////////////////////////////////////////////////////////
	public bool IsAllowed(string stateName)
	{
		foreach (vp_StateInfo s in m_States)
		{
			if (s.Name == stateName)
				return s.Allowed;
		}
		return false;
	}



}


