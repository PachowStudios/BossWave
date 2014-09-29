using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour 
{
	[System.Serializable]
	public struct Wave
	{
		public float startTime;
		public float amount;
		public float spawnDelay;
		public Enemy.Difficulty difficulty;
	}

	public List<Wave> waves;
	public List<Enemy> enemies;
	public List<Powerup> powerups;
	public float minPowerupTime = 15f;
	public float maxPowerupTime = 25f;
	public float powerupBuffer = 5f;

	private int currentWave = 0;
	private float waveTimer = 0f;

	private List<GameObject> spawners;
	private Transform powerupSpawner;
	private float powerupTimer = 0f;
	private float powerupTime;
	private float powerupRange;


	void Awake()
	{
		spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList<GameObject>();
		powerupSpawner = GameObject.FindGameObjectWithTag("PowerupSpawner").transform;
		powerupTime = Random.Range(minPowerupTime, maxPowerupTime);
		powerupRange = Camera.main.orthographicSize * Camera.main.aspect - powerupBuffer;
	}

	void FixedUpdate()
	{
		waveTimer += Time.deltaTime;

		if (currentWave < waves.Count && waveTimer >= waves[currentWave].startTime)
		{
			StartCoroutine(SpawnWave(currentWave));
			currentWave++;
		}

		powerupTimer += Time.deltaTime;

		if (powerups.Count > 0 && powerupTimer >= powerupTime)
		{
			Vector3 powerupSpawnPoint = powerupSpawner.position + new Vector3(Random.Range(-powerupRange, powerupRange), 0, 0);
			int powerupToSpawn = Mathf.RoundToInt(Random.Range(0f, powerups.Count - 1));

			Instantiate(powerups[powerupToSpawn], powerupSpawnPoint, Quaternion.identity);

			powerupTimer = 0f;
			powerupTime = Random.Range(minPowerupTime, maxPowerupTime);
		}
	}

	IEnumerator SpawnWave(int wave)
	{
		List<Enemy> possibleEnemies = new List<Enemy>();

		foreach (Enemy enemy in enemies)
		{
			if (enemy.difficulty == waves[wave].difficulty)
			{
				possibleEnemies.Add(enemy);
			}
		}

		if (possibleEnemies.Count > 0 && spawners.Count > 0)
		{
			for (int i = 0; i < waves[wave].amount; i++)
			{
				int enemyToSpawn = Mathf.RoundToInt(Random.Range(0f, possibleEnemies.Count - 1));
				int spawnerToUse = Mathf.RoundToInt(Random.Range(0f, spawners.Count - 1));

				Instantiate(possibleEnemies[enemyToSpawn], spawners[spawnerToUse].transform.position, Quaternion.identity);

				yield return new WaitForSeconds(waves[wave].spawnDelay);
			}
		}
	}
}
