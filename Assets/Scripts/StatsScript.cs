using UnityEngine;
using System.Collections;

public class StatsScript : MonoBehaviour {

	public int _killCount;
	public int _playerHealth;

	// Use this for initialization
	void Start () {
		_killCount = 0;
		_playerHealth = 100;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void AddKill() {
		_killCount += 1;
		Debug.Log("Kill Count = " + _killCount);
	}

	public void TakeDamage(int damage) {
		_playerHealth -= damage;
	}

	void OnGUI () {
		string statsString = "Kill Count: " + _killCount + "\n"
			+ "Health: " + _playerHealth;
		GUI.Label (new Rect(0,0,100,100), statsString);
	}
}
