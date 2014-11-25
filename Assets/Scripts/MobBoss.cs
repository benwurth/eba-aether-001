using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MobBoss : MonoBehaviour {

	// Stores all SpawnPoints and GenericMobs
	List<SpawnPoint> m_spawnPointList = new List<SpawnPoint>();
	List<GenericMob> m_currentMobList = new List<GenericMob>();
	SpawnPoint currentSpawn;
	// Slot for prefabs to spawn
	public List<MobWave> wave = new List<MobWave>();
	MobWave currentWave;
	Rigidbody currentMob;

	public int Max_Mobs;

	[Serializable]
	public struct MobWave {
		public Rigidbody Prefab;
		public int count;
	}

	enum state {
		Idle,

	}

	// Use this for initialization
	void Start () {
		currentWave = wave[0];
	}
	
	// Update is called once per frame
	void Update () {
//		Debug.Log(m_spawnPointList.Count);
//		if (Input.GetKeyDown(KeyCode.M)) {
//			int r = UnityEngine.Random.Range(0, m_spawnPointList.Count);
//			currentSpawn = m_spawnPointList[r];
//			currentSpawn.GetComponent<SpawnPoint>().SpawnMob(wave);
//		}

		 if (m_currentMobList.Count < Max_Mobs && currentWave.count > 0) {
			int r = UnityEngine.Random.Range(0, m_spawnPointList.Count);
			currentSpawn = m_spawnPointList[r];
			currentMob = currentWave.Prefab;
			currentSpawn.GetComponent<SpawnPoint>().SpawnMob(currentMob);
			currentWave.count -= 1;
//			Debug.Log(m_currentMobList.Count);
		}
	}

	public void RegisterSpawnPoint (SpawnPoint sp) {
		// Adds all spawn points to List m_spawnPointList
		m_spawnPointList.Add(sp);
	}

	public void DeRegisterSpawnPoint (SpawnPoint Space) {

	}

	public void RegisterMob (GenericMob mob) {
		// Adds all mobs to List m_mobList
		m_currentMobList.Add(mob);
	}

	public void DeRegisterMob (GenericMob mob) {
		m_currentMobList.Remove(mob);
	}
}
