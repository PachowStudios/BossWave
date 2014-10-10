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

	[System.Serializable]
	public struct BossWave
	{
		public Enemy boss;
		public float startTime;
		public float introLength;
		public float cameraMoveSpeed;
		public Transform bossSpawner;
		public Transform playerWaitPoint;
		public Transform scrollPoint;
	}

	public AudioSource mainMusic;
	public AudioSource bossMusic;

	public List<Wave> waves;
	public BossWave bossWave;
	public List<Enemy> enemies;
	public List<Powerup> powerups;
	public float minPowerupTime = 15f;
	public float maxPowerupTime = 25f;
	public float powerupBuffer = 5f;
	
	private bool bossWaveActive = false;
	private bool bossWaveIntroComplete = false;
	private bool bossWaveInitialized = false;
	private bool bossWavePlayerMoved = false;
	private int currentWave = 0;
	private float waveTimer;
	private float musicStartTime;

	private CameraFollow mainCamera;
	private Transform cameraWrapper;
	private Transform worldBoundaries;
	private Cutscene cutscene;
	private PlayerControl playerControl;
	private List<GameObject> scrollingElements;
	private List<GameObject> spawners;
	private Transform powerupSpawner;
	private float powerupTimer = 0f;
	private float powerupTime;
	private float powerupRange;


	void Awake()
	{
		mainCamera = Camera.main.GetComponent<CameraFollow>();
		cameraWrapper = GameObject.FindGameObjectWithTag("CameraWrapper").transform;
		worldBoundaries = GameObject.FindGameObjectWithTag("WorldBoundaries").transform;
		cutscene = GameObject.FindGameObjectWithTag("UI").GetComponent<Cutscene>();
		playerControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		scrollingElements = GameObject.FindGameObjectsWithTag("Scrolling").ToList<GameObject>();
		spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList<GameObject>();
		powerupSpawner = GameObject.FindGameObjectWithTag("PowerupSpawner").transform;
		powerupTime = Random.Range(minPowerupTime, maxPowerupTime);
		powerupRange = Camera.main.orthographicSize * Camera.main.aspect - powerupBuffer;

		mainMusic.Play();
		musicStartTime = Time.time;
		waveTimer = musicStartTime;
	}

	void FixedUpdate()
	{
		waveTimer += Time.deltaTime;

		if (waveTimer >= bossWave.startTime)
		{
			bossWaveActive = true;
		}

		if (bossWaveActive)
		{
			if (!bossWaveIntroComplete)
			{
				if (!bossWaveInitialized)
				{
					cutscene.StartCutscene();
					Instantiate(bossWave.boss, bossWave.bossSpawner.position, Quaternion.identity);
					mainCamera.FollowObject(cameraWrapper, true, true);
					worldBoundaries.localScale = new Vector3(Camera.main.aspect, worldBoundaries.localScale.y, worldBoundaries.localScale.z);
					playerControl.GoToPoint(bossWave.playerWaitPoint.position, false);

					bossWaveInitialized = true;
				}

				if (waveTimer >= bossWave.startTime + bossWave.introLength)
				{
					if (!bossWavePlayerMoved)
					{
						playerControl.continuouslyRunning = true;
						playerControl.GoToPoint(bossWave.scrollPoint.position);

						bossWavePlayerMoved = true;
					}

					if (cameraWrapper.position.x < bossWave.scrollPoint.position.x)
					{
						cameraWrapper.position += new Vector3(bossWave.cameraMoveSpeed * Time.deltaTime, 0);
					}
					else
					{
						cutscene.EndCutscene();
						playerControl.cancelGoTo = true;

						foreach(GameObject element in scrollingElements)
						{
							element.GetComponent<ScrollInfinite>().loop = true;
						}

						bossWaveIntroComplete = true;
					}
				}
			}
		}
		else
		{
			if (currentWave < waves.Count && waveTimer >= waves[currentWave].startTime + musicStartTime)
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
