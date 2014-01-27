using UnityEngine;
using System.Collections;

public class Spawning : MonoBehaviour 
{ 
	
	public int NumberEnemies;
	public float SpawnTimer;
	public float SpawnCountdown = 20.0f;
	 
	public Transform[] SpawnPoints;
	public Transform[] Enemy;
	public bool canSpawn = false;
	 
	void Start()
	{
	 
	}
	 
	void SpawnEnemy()
	{   
	    // Mettre un index en paramètre de mes tableaux.
	    Instantiate(Enemy[0], SpawnPoints[0].position, Quaternion.identity);
	    NumberEnemies += 1;
	    SpawnTimer = 0.0f;
	}
	 
	void Update () 
	{
	    while(NumberEnemies != 7)
	    {
	       SpawnTimer = 0.0f;
	       SpawnTimer += Time.deltaTime;
	 
	       if(SpawnTimer >= SpawnCountdown)
	       {
	         SpawnEnemy();
	       }
	    }
	}
}