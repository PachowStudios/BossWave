using UnityEngine;
using UnityEngine.UI;
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
		public float totalLength;
		public float cameraMoveSpeed;
		public Transform bossSpawner;
		public Transform playerWaitPoint;
		public Scrollbar progressBar;
	}

	public AudioSource mainMusic;

	public List<Wave> waves;
	public BossWave bossWave;
	public List<Enemy> enemies;
	public List<Powerup> powerups;
	public float minPowerupTime = 15f;
	public float maxPowerupTime = 25f;
	public float powerupBuffer = 5f;

	[HideInInspector]
	public bool bossWavePlayerMoved = false;

	private Enemy bossInstance;
	private bool bossWaveActive = false;
	private bool bossWaveIntroComplete = false;
	private bool bossWaveInitialized = false;
	private int currentWave = 0;
	private float waveTimer;

	private CameraFollow mainCamera;
	private Transform cameraWrapper;
	//private Transform worldBoundaries;
	private PlayerControl playerControl;
	private List<GameObject> scrollingElements;
	private List<GameObject> spawners;
	private Transform powerupSpawner;
	private float powerupTimer = 0f;
	private float powerupTime;
	private float powerupRange;

	void Awake()
	{
		LoadPrefs();

		mainCamera = Camera.main.GetComponent<CameraFollow>();
		cameraWrapper = GameObject.FindGameObjectWithTag("CameraWrapper").transform;
		//worldBoundaries = GameObject.FindGameObjectWithTag("WorldBoundaries").transform;
		playerControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		scrollingElements = GameObject.FindGameObjectsWithTag("Scrolling").ToList<GameObject>();
		spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList<GameObject>();
		powerupSpawner = GameObject.FindGameObjectWithTag("PowerupSpawner").transform;
		powerupTime = Random.Range(minPowerupTime, maxPowerupTime);
		powerupRange = Camera.main.orthographicSize * Camera.main.aspect - powerupBuffer;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		mainMusic.Play();
		waveTimer = mainMusic.time;
	}

	void Start()
	{
		CRTEffect.EndCRT(2f, Screen.height, 0f, iTween.EaseType.easeInOutSine);
	}

	void FixedUpdate()
	{
		waveTimer = mainMusic.time;

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
					Cutscene.StartCutscene();
					bossInstance = Instantiate(bossWave.boss, bossWave.bossSpawner.position, Quaternion.identity) as Enemy;
					mainCamera.FollowObject(cameraWrapper, true, true);
					//worldBoundaries.localScale = new Vector3(Camera.main.aspect, worldBoundaries.localScale.y, worldBoundaries.localScale.z);
					playerControl.GoToPoint(bossWave.playerWaitPoint.position, false);

					bossWaveInitialized = true;
				}

				if (waveTimer >= bossWave.startTime + bossWave.introLength)
				{
					playerControl.continuouslyRunning = true;
					bossWavePlayerMoved = true;

					Cutscene.EndCutscene(true);
					bossInstance.GetComponent<Enemy>().enabled = true;
					bossInstance.GetComponent<Collider2D>().enabled = true;

					foreach(GameObject element in scrollingElements)
					{
						element.GetComponent<ScrollInfinite>().scroll = true;
					}

					bossWave.progressBar.GetComponent<Animator>().SetTrigger("Show");

					iTween.ValueTo(gameObject, iTween.Hash("from", 0f,
														   "to", 1f,
														   "time", bossWave.totalLength,
														   "easetype", iTween.EaseType.linear,
														   "onupdate", "BossWaveProgressBarUpdate"));

					bossWaveIntroComplete = true;
				}
			}
		}
		else
		{
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
	}

	private IEnumerator SpawnWave(int wave)
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

	private void BossWaveProgressBarUpdate(float newValue)
	{
		bossWave.progressBar.value = newValue;
	}

	private void LoadPrefs()
	{
		if (PlayerPrefs.HasKey("Settings/Volume"))
		{
			AudioListener.volume = PlayerPrefs.GetFloat("Settings/Volume");
		}
	}
}
