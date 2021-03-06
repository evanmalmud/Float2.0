﻿using UnityEngine;
using System.Collections.Generic;


//Code from Unity tutorial: Survival shooter game video 9/10
public class enemySpawn : MonoBehaviour
{
	public GameObject enemy;                // The enemy prefab to be spawned.
	public float spawnTime = 3f;            // How long between each spawn.
	public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.
	public List<GameObject> listOfEnemies = new List<GameObject> ();

	void Awake() {
		//Spawn ();
	}

	void Start ()
	{
		// Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
		InvokeRepeating ("Spawn", spawnTime, spawnTime);
	}


	void Spawn ()
	{
		//Debug.Log ("blah: " + spawnPoints.Length);
		// Find a random index between zero and one less than the number of spawn points.
		int spawnPointIndex = Random.Range (0, spawnPoints.Length);

		//Debug.Log("blah: "+spawnPointIndex);
		// Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
		GameObject eneOb = (GameObject) Instantiate (enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
		listOfEnemies.Add (eneOb);

		//Debug.Log(listOfEnemies);
	}
		
}