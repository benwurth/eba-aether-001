using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour {
	
	MobBoss m_MobBoss;

	void Awake () {
		// Grab the mobBoss GameObject
		GameObject mobBossObject = GameObject.Find("MobBoss");
		if (!mobBossObject) {
			Debug.LogWarning("Can't find GameObject 'MobBoss'");
		}

		// Find the MobBoss monobehaviour component
		m_MobBoss = mobBossObject.GetComponent<MobBoss>();
		if (!m_MobBoss) {
			Debug.LogWarning("Can't find component of type MobBoss in 'mobBossObject'");
		}

		// Register this SpawnPoint with the MobBoss
		m_MobBoss.RegisterSpawnPoint(this);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SpawnMob (Rigidbody mob) {
		Rigidbody mobClone = (Rigidbody) Instantiate(mob, transform.position, transform.rotation);
	}
}
