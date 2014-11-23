/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Bullet.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	a generic class for hitscan projectiles. this script should be
//					attached to a gameobject with a mesh to be used as the impact
//					decal (bullet hole).
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class vp_Bullet : MonoBehaviour
{
	
	// gameplay
	public float Range = 100.0f;			// max travel distance of this type of bullet in meters
	public float Force = 100.0f;			// force applied to any rigidbody hit by the bullet
	public float Damage = 1.0f;				// the damage transmitted to target by the bullet
	public string DamageMethodName = "Damage";	// user defined name of damage method on target
												// TIP: this can be used to apply different types of damage, i.e
												// magical, freezing, poison, electric

	public float m_SparkFactor = 0.5f;		// chance of bullet impact generating a spark

	// these gameobjects will all be spawned at the point and moment
	// of impact. technically they could be anything, but their
	// intended uses are as follows:
	public GameObject m_ImpactPrefab = null;	// a flash or burst illustrating the shock of impact
	public GameObject m_DustPrefab = null;		// evaporating dust / moisture from the hit material
	public GameObject m_SparkPrefab = null;		// a quick spark, as if hitting stone or metal
	public GameObject m_DebrisPrefab = null;	// pieces of material thrust out of the bullet hole and / or falling to the ground

	// sound
	public List<AudioClip> m_ImpactSounds = new List<AudioClip>();	// list of impact sounds to be randomly played
	public Vector2 SoundImpactPitch = new Vector2(1.0f, 1.5f);	// random pitch range for impact sounds


	///////////////////////////////////////////////////////////
	// everything happens in the Start method. the script that
	// spawns the bullet is responsible for setting its position 
	// and angle. after being instantiated, the bullet immediately
	// raycasts ahead for its full range, then snaps itself to
	// the surface of the first object hit. it then spawns a
	// number of particle effects and plays a random impact sound.
	///////////////////////////////////////////////////////////
	void Start()
	{

		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;

		// raycast against everything except the player itself and
		// debris such as shell cases
		if(Physics.Raycast(ray, out hit, Range, ~((1 << vp_Layer.Player) | (1 << vp_Layer.Debris))))
		{

			// move this gameobject instance to the hit object
			Vector3 scale = transform.localScale;	// save scale for 
			transform.parent = hit.transform;
			transform.localPosition = hit.transform.InverseTransformPoint(hit.point);
			transform.rotation = Quaternion.LookRotation(hit.normal);				// face away from hit surface
			if (hit.transform.lossyScale == Vector3.one)							// if hit object has normal scale
				transform.Rotate(Vector3.forward, Random.Range(0, 360), Space.Self);	// spin randomly
			else
			{
				// rotated child objects will get skewed if the parent
				// object has been unevenly scaled in the editor, so on
				// scaled objects we don't support spin, and we need to
				// unparent, rescale and reparent the decal.
				transform.parent = null;
				transform.localScale = scale;
				transform.parent = hit.transform;
			}

			// if hit object has physics, add the bullet force to it
			Rigidbody body = hit.collider.attachedRigidbody;
			if (body != null && !body.isKinematic)
			{
				Vector3 pushDir = ray.direction * Force;
				body.AddForceAtPosition(pushDir, hit.point);
			}

			// spawn impact effect
			if (m_ImpactPrefab != null)
				Object.Instantiate(m_ImpactPrefab, transform.position, transform.rotation);

			// spawn dust effect
			if (m_DustPrefab != null)
				Object.Instantiate(m_DustPrefab, transform.position, transform.rotation);

			// spawn spark effect
			if (m_SparkPrefab != null)
			{
				if (Random.value < m_SparkFactor)
					Object.Instantiate(m_SparkPrefab, transform.position, transform.rotation);
			}

			// spawn debris particle fx
			if (m_DebrisPrefab != null)
				Object.Instantiate(m_DebrisPrefab, transform.position, transform.rotation);

			// play impact sound
			if (m_ImpactSounds.Count > 0)
			{
				audio.playOnAwake = false;
				audio.minDistance = 3;
				audio.maxDistance = 50;
				audio.dopplerLevel = 0.0f;
				audio.pitch = Random.Range(SoundImpactPitch.x, SoundImpactPitch.y);
				audio.PlayOneShot(m_ImpactSounds[(int)Random.Range(0, (m_ImpactSounds.Count))]);
			}

			// do damage on the target
			hit.collider.SendMessage(DamageMethodName, Damage, SendMessageOptions.DontRequireReceiver);

			// if bullet is visible (i.e. has a decal), cueue it for deletion later
			if (gameObject.renderer != null)
				vp_DecalManager.Add(gameObject);
			else
				vp_Timer.In(1, TryDestroy);		// we have no renderer, so destroy object in 1 sec
		}
		else
			Object.Destroy(gameObject);	// hit nothing, so self destruct immediately

	}


	///////////////////////////////////////////////////////////
	// sees if the impact sound is still playing and, if not,
	// destroys the object. otherwise tries again in 1 sec
	///////////////////////////////////////////////////////////
	private void TryDestroy()
	{
		if (!audio.isPlaying)
			Object.Destroy(gameObject);
		else
			vp_Timer.In(1, TryDestroy);
	}


}

