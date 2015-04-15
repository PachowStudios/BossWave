using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
	#region Types
	[System.Serializable]
	public struct Wave
	{
		public float startTime;
		public float amount;
		public float spawnDelay;
		public Enemy.Difficulty difficulty;
		public List<GameObject> spawners;
	}
	#endregion

	#region Fields
	private static LevelManager instance;	

	public bool introCRT = true;
	public bool spawnEnemies = true;
	public float timeOverride = 0f;
	public float fadeInTime = 2f;
	public int killAllEnemiesBonus = 100000;
	public int timeBonusMultiplier = 10;
	public List<Wave> waves;
	public List<StandardEnemy> enemies;
	public Transform foregroundLayer;

	[SerializeField]
	protected AudioSource mainMusic;
	[SerializeField]
	protected Transform groundLevel;
	[SerializeField]
	protected Transform bossWaveWaitPoint;

	protected int currentWave = 0;
	protected float waveTimer;

	protected List<GameObject> scrollingElements;
	protected List<GameObject> spawners;
	#endregion

	#region Public Properties
	public static LevelManager Instance
	{ get { return instance; } }

	public float MusicTime
	{ get { return mainMusic.time; } }

	public Vector3 GroundLevel
	{ get { return groundLevel.position; } }

	public Vector3 BossWaveWaitPoint
	{ get { return bossWaveWaitPoint.position; } }

	public bool BossWaveActive { get; protected set; }
	#endregion

	#region MonoBehaviour
	protected virtual void Awake()
	{
		instance = this;

		DOTween.Init();

		scrollingElements = GameObject.FindGameObjectsWithTag("Scrolling").ToList();
		spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList();

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	protected virtual void Start()
	{
		if (timeOverride != 0f)
			mainMusic.time = timeOverride;

		mainMusic.pitch = 0f;
		mainMusic.Play();
		waveTimer = mainMusic.time;

		if (introCRT)
		{
			DOTween.Sequence()
				.SetUpdate(true)
				.AppendCallback(() => GameMenu.Instance.EnablePausing(false))
				.AppendInterval(fadeInTime + 0.1f)
				.AppendCallback(() => GameMenu.Instance.EnablePausing(true));

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

	protected virtual void Update()
	{
		waveTimer = mainMusic.time;

		if (!BossWaveActive && !PlayerControl.Instance.IsDead)
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
	#endregion

	#region Internal Helper Methods
	protected virtual IEnumerator SpawnWave(int waveIndex)
	{
		Wave wave = waves[waveIndex];
		List<StandardEnemy> possibleEnemies = new List<StandardEnemy>();

		foreach (StandardEnemy enemy in enemies)
		{
			if (enemy.difficulty == wave.difficulty)
			{
				possibleEnemies.Add(enemy);
			}
		}

		List<GameObject> waveSpawners = wave.spawners.Count > 0 ? wave.spawners : spawners;

		if (possibleEnemies.Count > 0 && waveSpawners.Count > 0)
		{
			for (int i = 0; i < wave.amount; i++)
			{
				StandardEnemy enemyToSpawn = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
				Transform spawnerToUse = waveSpawners[Random.Range(0, waveSpawners.Count)].transform;

				StandardEnemy currentEnemy = Instantiate(enemyToSpawn, Vector3.zero, Quaternion.identity) as StandardEnemy;
				currentEnemy.Spawner = spawnerToUse;

				yield return new WaitForSeconds(wave.spawnDelay);
			}
		}
	}
	#endregion

	#region Public Methods
	public void KillAllEnemies()
	{
		StandardEnemy[] allEnemies = GameObject.FindObjectsOfType<StandardEnemy>();

		if (allEnemies.Count() == 0)
		{
			PlayerControl.Instance.AddPoints(killAllEnemiesBonus, true);
		}
		else
		{
			foreach (StandardEnemy currentEnemy in allEnemies)
			{
				currentEnemy.KillNoPoints();
			}
		}
	}
	#endregion

	#region Boss Wave Methods
	protected virtual void InitializeBossWave()
	{ }

	public virtual void StartBossWave()
	{ }

	public virtual void CompleteBossWave()
	{ }
	#endregion
}
