using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour 
{
	public static LevelManager instance;

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

	public bool spawnEnemies = true;
	public bool spawnPowerups = true;
	public bool introCRT = true;
	public float fadeInTime = 2f;
	public AudioSource mainMusic;
	public List<Wave> waves;
	public BossWave bossWave;
	public List<Enemy> enemies;
	public List<Gun> guns;
	public List<Powerup> powerups;
	public List<Microchip> microchips;
	public float minPowerupTime = 15f;
	public float maxPowerupTime = 25f;
	public float powerupBuffer = 5f;
	public GameObject worldBoundaries;
	public GameObject runningBoundaries;
	public int runningFOV = 400;

	[HideInInspector]
	public bool bossWavePlayerMoved = false;

	private Enemy bossInstance;
	private bool bossWaveActive = false;
	private bool bossWaveIntroComplete = false;
	private bool bossWaveInitialized = false;
	private int currentWave = 0;
	private float waveTimer;

	private float powerupTimer = 0f;
	private float powerupTime;
	private float powerupRange;

	private CameraFollow mainCamera;
	private Transform cameraWrapper;
	private List<GameObject> scrollingElements;
	private List<GameObject> spawners;
	private Transform powerupSpawner;

	void Awake()
	{
		instance = this;

		mainCamera = Camera.main.GetComponent<CameraFollow>();
		cameraWrapper = GameObject.FindGameObjectWithTag("CameraWrapper").transform;
		scrollingElements = GameObject.FindGameObjectsWithTag("Scrolling").ToList<GameObject>();
		spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList<GameObject>();
		powerupSpawner = GameObject.FindGameObjectWithTag("PowerupSpawner").transform;

		powerupTime = Random.Range(minPowerupTime, maxPowerupTime);
		powerupRange = Camera.main.orthographicSize * Camera.main.aspect - powerupBuffer;

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	void Start()
	{
		mainMusic.pitch = 0f;
		mainMusic.Play();
		waveTimer = mainMusic.time;

		if (introCRT)
		{
			Time.timeScale = 0f;
			Time.fixedDeltaTime = 0f;
			TimeWarpEffect.EndWarp(fadeInTime, new AudioSource[] { mainMusic }, iTween.EaseType.easeInOutSine);
			CRTEffect.EndCRT(fadeInTime, Screen.height, 0f, iTween.EaseType.easeInOutSine);
		}
		else
		{
			Time.timeScale = 1f;
			Time.fixedDeltaTime = TimeWarpEffect.instance.defaultFixedTimestep;
			mainMusic.pitch = 1f;
		}
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
					ScaleWidthCamera.AnimateFOV(runningFOV, 1f);
					bossInstance = Instantiate(bossWave.boss, bossWave.bossSpawner.position, Quaternion.identity) as Enemy;
					mainCamera.FollowObject(cameraWrapper, new Vector2(0.01f, 0.01f));
					PlayerControl.instance.GoToPoint(bossWave.playerWaitPoint.position, false, false);

					bossWaveInitialized = true;
				}

				if (waveTimer >= bossWave.startTime + bossWave.introLength)
				{
					worldBoundaries.SetActive(false);
					runningBoundaries.SetActive(true);

					PlayerControl.instance.continuouslyRunning = true;
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
		else if (!PlayerControl.instance.Dead)
		{
			if (currentWave < waves.Count && waveTimer >= waves[currentWave].startTime && spawnEnemies)
			{
				StartCoroutine(SpawnWave(currentWave));
				currentWave++;
			}

			powerupTimer += Time.deltaTime;

			if (powerups.Count > 0 && powerupTimer >= powerupTime && spawnPowerups)
			{
				Vector3 powerupSpawnPoint = powerupSpawner.position + new Vector3(Random.Range(-powerupRange, powerupRange), 0, 0);
				int powerupToSpawn = Random.Range(0, powerups.Count);

				Instantiate(powerups[powerupToSpawn], powerupSpawnPoint, Quaternion.identity);

				powerupTimer = 0f;
				powerupTime = Random.Range(minPowerupTime, maxPowerupTime);
			}
		}
		else
		{
			StopAllCoroutines();
		}
	}

	public static void SpawnMicrochip(Vector3 position, Microchip.Size size = Microchip.Size.Small, bool randomVelocity = true)
	{
		Microchip microchipToSpawn = instance.microchips[(int)size];

		Microchip microchipInstance = Instantiate(microchipToSpawn, position, Quaternion.identity) as Microchip;
		
		if (randomVelocity)
		{
			microchipInstance.GetComponent<Scatter>().DoScatter();
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
}
