using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class LevelManager : MonoBehaviour 
{
	private static LevelManager instance;

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
		public Boss boss;
		public float startTime;
		public float totalLength;
		public float cameraSpeed;
		public float fullCameraSpeed;
		public float speedUpTime;
		public Transform spawner;
		public Transform playerWaitPoint;
		public Transform scrollingEndcap;
	}

	public bool introCRT = true;
	public bool spawnEnemies = true;
	public float fadeInTime = 2f;
	public AudioSource mainMusic;
	public List<Wave> waves;
	public BossWave bossWave;
	public List<StandardEnemy> enemies;
	public Transform foregroundLayer;
	public GameObject worldBoundaries;
	public GameObject runningBoundaries;

	[SerializeField]
	private Transform groundLevel;

	[HideInInspector]
	public bool bossWavePlayerMoved = false;

	private Boss bossInstance;
	private bool bossWaveActive = false;
	private bool bossWaveIntroComplete = false;
	private bool bossWaveInitialized = false;
	private int currentWave = 0;
	private float waveTimer;

	private List<GameObject> scrollingElements;
	private List<GameObject> spawners;

	public static LevelManager Instance
	{
		get { return instance; }
	}

	public Vector3 GroundLevel
	{
		get { return groundLevel.position; }
	}

	private void Awake()
	{
		instance = this;

		DOTween.Init();

		scrollingElements = GameObject.FindGameObjectsWithTag("Scrolling").ToList();
		spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList();

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	private void Start()
	{
		mainMusic.pitch = 0f;
		mainMusic.Play();
		mainMusic.time = 120f;
		waveTimer = mainMusic.time;

		GameMenu.Instance.EnablePausing(false);
		DOTween.Sequence()
			.SetUpdate(true)
			.AppendInterval(fadeInTime + 0.1f)
			.AppendCallback(() => GameMenu.Instance.EnablePausing(true));

		if (introCRT)
		{
			Time.timeScale = 0f;
			Time.fixedDeltaTime = 0f;
			TimeWarpEffect.Instance.EndWarp(fadeInTime, new AudioSource[] { mainMusic }, Ease.InOutSine);
			CRTEffect.Instance.EndCRT(fadeInTime, 0f, Screen.height, Ease.InOutSine);
		}
		else
		{
			Time.timeScale = 1f;
			Time.fixedDeltaTime = TimeWarpEffect.Instance.DefaultFixedTimestep;
			mainMusic.pitch = 1f;
		}
	}

	private void FixedUpdate()
	{
		waveTimer = mainMusic.time;

		if (waveTimer >= bossWave.startTime && !bossWaveActive)
		{
			bossWaveActive = true;
			PowerupSpawner.Instance.spawnPowerups = false;
		}

		if (bossWaveActive)
		{
			if (!bossWaveIntroComplete)
			{
				if (!bossWaveInitialized)
				{
					bossInstance = Instantiate(bossWave.boss, bossWave.spawner.position, Quaternion.identity) as Boss;
					bossInstance.Spawn();

					bossWaveInitialized = true;
				}

				if (bossInstance != null && bossInstance.spawned)
				{
					worldBoundaries.SetActive(false);
					runningBoundaries.SetActive(true);

					PlayerControl.Instance.continuouslyRunning = true;
					bossWavePlayerMoved = true;

					Cutscene.Instance.Hide(true);

					foreach (GameObject element in scrollingElements)
					{
						element.GetComponent<Parallax>().scroll = true;
					}

					DOTween.To(() => bossWave.cameraSpeed, x => bossWave.cameraSpeed = x, bossWave.fullCameraSpeed, bossWave.speedUpTime)
						.SetEase(Ease.OutSine);
					BossWaveTimer.Instance.Show();
					BossWaveTimer.Instance.Timer = bossWave.totalLength;
					DOTween.To(() => BossWaveTimer.Instance.Timer, x => BossWaveTimer.Instance.Timer = x, 0f, bossWave.totalLength)
						.SetId("Boss Wave Timer")
						.SetEase(Ease.Linear)
						.OnComplete(() =>
						{
							bossWave.scrollingEndcap.parent.GetComponent<Parallax>().AddEndcap(bossWave.scrollingEndcap);
							bossInstance.End();
						});

					bossWaveIntroComplete = true;
				}
			}
			else if (bossWavePlayerMoved && PlayerControl.Instance.Dead)
			{
				bossWavePlayerMoved = false;
				DOTween.To(() => bossWave.cameraSpeed, x => bossWave.cameraSpeed = x, 0f, 2f)
								.SetEase(Ease.OutSine);
			}
		}
		else if (!PlayerControl.Instance.Dead)
		{
			if (currentWave < waves.Count && waveTimer >= waves[currentWave].startTime && spawnEnemies)
			{
				StartCoroutine(SpawnWave(currentWave));
				currentWave++;
			}
		}
		else
		{
			StopAllCoroutines();
		}
	}

	public void KillAllEnemies()
	{
		foreach (StandardEnemy currentEnemy in GameObject.FindObjectsOfType<StandardEnemy>())
		{
			currentEnemy.KillNoPoints();
		}
	} 

	private IEnumerator SpawnWave(int wave)
	{
		List<StandardEnemy> possibleEnemies = new List<StandardEnemy>();

		foreach (StandardEnemy enemy in enemies)
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

				StandardEnemy currentEnemy = Instantiate(possibleEnemies[enemyToSpawn], Vector3.zero, Quaternion.identity) as StandardEnemy;
				currentEnemy.Spawner = spawners[spawnerToUse].transform;

				yield return new WaitForSeconds(waves[wave].spawnDelay);
			}
		}
	}
}
