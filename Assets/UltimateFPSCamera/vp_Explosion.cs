/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Explosion.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	death effect for exploding objects. applies damage and a
//					physical force to all rigidbodies and FPSPlayers within its
//					range, plays a sound and instantiates a list of gameobjects
//					for special effects (e.g. particle fx). destroys itself when
//					the sound has stopped playing
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class vp_Explosion : MonoBehaviour
{

	// gameplay
	public float Radius = 15.0f;					// any objects within radius will be affected by the explosion
	public float Force = 1000.0f;					// amount of motion force to apply to affected objects
	public float UpForce = 10.0f;					// how much to push affected objects up in the air
	public float Damage = 10;						// amount of damage to apply to objects via their 'Damage' method
	public float CameraShake = 1.0f;				// how much of a shockwave impulse to apply to the camera
	public string DamageMethodName = "Damage";		// user defined name of damage method on affected object
														// TIP: this can be used to apply different types of damage, i.e
														// magical, freezing, poison, electric
	
	// sound
	public AudioClip Sound = null;
	public float SoundMinPitch = 0.8f;				// random pitch range for explosion sound
	public float SoundMaxPitch = 1.2f;

	// fx
	public List <GameObject> FXPrefabs = new List<GameObject>();	// list of special effects objects to spawn


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Awake()
	{

		// spawn effects gameobjects
		foreach(GameObject fx in FXPrefabs)
		{
			Component[] c;
			c = fx.GetComponents<vp_Explosion>();
			if (c.Length == 0)
				Object.Instantiate(fx, transform.position, transform.rotation);
			else
				Debug.LogError("Error: vp_Explosion->FXPrefab must not be a vp_Explosion (risk of infinite loop).");
		}

		// apply shockwave to all rigidbodies and FPSPlayers within range
		Collider[] colliders = Physics.OverlapSphere(transform.position, Radius);
		foreach (Collider hit in colliders)
		{
			if (hit != this.collider)
			{

				//Debug.Log(hit.transform.name + Time.time);	// snippet to dump affected objects
				if (hit.rigidbody)
				{

					float distanceModifier = (1 - Vector3.Distance(transform.position,
																	hit.transform.position) / Radius);

					// explosion up force should only work on grounded objects,
					// otherwise object may acquire extreme speeds
					Ray ray = new Ray(hit.transform.position, -Vector3.up);
					RaycastHit hit2;
					if (!Physics.Raycast(ray, out hit2, 1))
						UpForce = 0.0f;

					// bash all objects within range
					hit.rigidbody.AddExplosionForce(Force, transform.position, Radius, UpForce);

					if (hit.gameObject.layer != vp_Layer.Debris)
						hit.gameObject.SendMessage(DamageMethodName, distanceModifier * Damage,
																	SendMessageOptions.DontRequireReceiver);

				}
				else
				{

					vp_FPSController controller = hit.GetComponent<vp_FPSController>();
					if (controller)
					{

						float distanceModifier = (1 - Vector3.Distance(transform.position,
																		hit.transform.position) / Radius) * 0.5f;

						controller.AddForce((controller.transform.position - transform.position).normalized *
																				Force * 0.001f * distanceModifier);
						vp_FPSCamera camera = hit.GetComponentInChildren<vp_FPSCamera>();
						if (camera)
							camera.DoBomb(distanceModifier * CameraShake);

						if (hit.gameObject.layer != vp_Layer.Debris)
							hit.gameObject.SendMessage(DamageMethodName, distanceModifier * Damage,
																		SendMessageOptions.DontRequireReceiver);

					}

				}

			}

		}

		// play explosion sound
		audio.clip = Sound;
		audio.rolloffMode = AudioRolloffMode.Linear;
		audio.minDistance = 3;
		audio.maxDistance = 250;
		audio.dopplerLevel = 2.5f;
		audio.pitch = Random.Range(SoundMinPitch, SoundMaxPitch);
		audio.playOnAwake = false;	// 'playOnAwake' seems unreliable, so play manually
		audio.Play();

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Update()
	{

		// the explosion should be removed as soon as the sound has
		// stopped playing. NOTE: this implementation assumes that
		// the sound is always longer in seconds than the explosion
		// effect. should be OK in most cases.
		if (!audio.isPlaying)
			Object.Destroy(gameObject);

	}
	

}

	