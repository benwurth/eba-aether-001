/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSCrosshair.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	this script is just a stub for a way cooler crosshair system.
//					it simply draws a classic FPS crosshair center screen.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_FPSCrosshair : MonoBehaviour
{

	// crosshair texture
	public Texture m_ImageCrosshair = null;


	///////////////////////////////////////////////////////////
	// draws the crosshair texture smack in the middle of the screen
	///////////////////////////////////////////////////////////
	void OnGUI()
	{

		if (m_ImageCrosshair != null)
		{
			GUI.color = new Color(1, 1, 1, 0.8f);
			GUI.DrawTexture(new Rect((Screen.width * 0.5f) - (m_ImageCrosshair.width * 0.5f),
				(Screen.height * 0.5f) - (m_ImageCrosshair.height * 0.5f), m_ImageCrosshair.width,
				m_ImageCrosshair.height), m_ImageCrosshair);
			GUI.color = Color.white;
		}
	
	}
	

}

