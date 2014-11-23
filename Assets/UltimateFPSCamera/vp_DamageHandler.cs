/////////////////////////////////////////////////////////////////////////////////
//
//	vp_DamageHandler.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	class for having a gameobject take damage, die and respawn.
//					any other object can do damage on this monobehaviour like so:
//					    hitObject.SendMessage(Damage, 1.0f, SendMessageOptions.DontRequireReceiver);
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public class vp_DamageHandler : MonoBehaviour
{

	// health and death
	public float Health = 10.0f;			// initial health of the object instance, to be reset on respawn
	public GameObject DeathEffect = null;	// gameobject to spawn when object dies.
												// TIP: could be fx, could also be rigidbody rubble
	public float MinDeathDelay = 0.0f;		// random timespan in seconds to delay death. good for cool serial explosions
	public float MaxDeathDelay = 0.0f;
	protected float m_CurrentHealth = 0.0f;	// current health of the object instance

	// respawn
	public bool Respawns = true;			// whether to respawn object or just delete it
	public float MinRespawnTime = 3.0f;		// random timespan in seconds to delay respawn
	public float MaxRespawnTime = 3.0f;
	public float RespawnCheckRadius = 1.0f;	// area around object which must be clear of other objects before respawn
	public AudioClip RespawnSound = null;	// sound to play upon respawn
	protected Vector3 m_StartPosition = Vector3.zero;				// initial position detected and used for respawn
	protected Quaternion m_StartRotation = Quaternion.identity;		// initial rotation detected and used for respawn

	StatsScript m_statsScript;
	

	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Awake()
	{

		m_CurrentHealth = Health;
		m_StartPosition = transform.position;
		m_StartRotation = transform.rotation;

		// Connect the KillCountScript
		GameObject statsBot = GameObject.Find("StatsBot");
		if (statsBot == null) {
//			Debug.Log("No GameObject 'StatsBot' found.");
			return;
		}

		m_statsScript = statsBot.GetComponent<StatsScript>();
		if (m_statsScript == null) {
//			Debug.Log("No StatsScript found.");
			return;
		}
//		Debug.Log("StatsScript found!");
	}


	///////////////////////////////////////////////////////////
	// reduces current health by 'damage' points and kills the
	// object if health runs out
	///////////////////////////////////////////////////////////
	public void Damage(float damage)
	{

		if (!enabled || !gameObject.active)
			return;

		m_CurrentHealth -= damage;

		if (m_CurrentHealth <= 0.0f)
		{
			// TIP: if you want to display an effect on the object just
			// before it dies - such as putting a crack in an armor the
			// second before it shatters - this is the place
			vp_Timer.In(Random.Range(MinDeathDelay, MaxDeathDelay), Die);
			return;
		}

		// TIP: if you want to do things like play a special impact
		// sound upon every hit (but only if the object survives)
		// this is the place

	}


	///////////////////////////////////////////////////////////
	// removes the object, plays the death effect and schedules
	// a respawn if enabled, otherwise destroys the object
	///////////////////////////////////////////////////////////
	public void Die()
	{

		if (!enabled || !gameObject.active)
			return;

			m_statsScript.AddKill();

			RemoveBulletHoles();
			gameObject.SetActiveRecursively(false);

			if (DeathEffect != null)
				Object.Instantiate(DeathEffect, transform.position, transform.rotation);

			if (Respawns)
				vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
			else
				Object.Destroy(gameObject);
		
	}


	///////////////////////////////////////////////////////////
	// respawns the object if no other object is occupying the
	// respawn area. otherwise reschedules respawning
	///////////////////////////////////////////////////////////
	protected void Respawn()
	{

		// return if the object has been destroyed (for example
		// as a result of loading a new level while it was gone)
		if (this == null)
			return;

		// don't respawn if player is within checkradius
		// TIP: this can be expanded upon to check for additional object layers
		if (Physics.CheckSphere(m_StartPosition, RespawnCheckRadius, ((1 << vp_Layer.Player) | (1 << vp_Layer.Props))))
		{
			// attempt to respawn again until the checkradius is clear
			vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
			return;
		}

		// reset health, position, angle and motion
		m_CurrentHealth = Health;
		transform.position = m_StartPosition;
		transform.rotation = m_StartRotation;
		if (rigidbody != null)
		{
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.velocity = Vector3.zero;
		}

		// reactivate object and play spawn sound
		gameObject.SetActiveRecursively(true);
		if (audio != null)
			audio.PlayOneShot(RespawnSound);
		
	}


	///////////////////////////////////////////////////////////
	// removes any bullet decals currently childed to this object
	///////////////////////////////////////////////////////////
	protected void RemoveBulletHoles()
	{

		foreach (Transform t in transform)
		{
			Component[] c;
			c = t.GetComponents<vp_Bullet>();
			if (c.Length != 0)
				Object.Destroy(t.gameObject);
		}

	}
	
}

