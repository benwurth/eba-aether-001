/////////////////////////////////////////////////////////////////////////////////
//
//	vp_StateInfo.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	contains information on a temporary preset to be set on a
//					component when a certain state is enabled
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[System.Serializable]	// retain instance upon build and save

public class vp_StateInfo
{

	public string TypeName = null;
	public string Name = null;
	public TextAsset TextAsset = null;
	public vp_ComponentPreset Preset = null;		// this is only used at runtime
	public bool Enabled = false;
	public bool Allowed = true;


	///////////////////////////////////////////////////////////
	// constructor
	///////////////////////////////////////////////////////////
	public vp_StateInfo(string typeName, string name = "New State", string path = null, TextAsset asset = null)
	{

		TypeName = typeName;
		Name = name;
		TextAsset = asset;
	
	}


}


