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
		public Enemy.Difficulty difficulty;
	}

	public List<Wave> waves;
	public List<Enemy> enemies;

	private int currentWave = 0;
	private float waveTimer = 0f;

	private List<GameObject> spawners;

	void Awake()
	{
		spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList<GameObject>();
	}

	void FixedUpdate()
	{
		waveTimer += Time.deltaTime;

		if (currentWave < waves.Count && waveTimer >= waves[currentWave].startTime)
		{
			StartCoroutine(SpawnWave(currentWave));
			currentWave++;
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

				yield return new WaitForSeconds(0.5f);
			}
		}
	}
}
