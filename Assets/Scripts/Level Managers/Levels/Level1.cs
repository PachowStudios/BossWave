using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public sealed class Level1 : LevelManager
{
	#region Types
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
		public Transform scrollingEndcap;
	}
	#endregion

	#region Fields
	public BossWave bossWave;
	public GameObject worldBoundaries;
	public GameObject runningBoundaries;

	private Boss bossInstance;
	private bool bossWaveInitialized = false;
	private List<GameObject> scrollingElements;
	#endregion

	#region Public Properties
	public static new Level1 Instance
	{ get { return (Level1)instance; } }
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		scrollingElements = GameObject.FindGameObjectsWithTag("Scrolling").ToList();
	}

	protected override void Update()
	{
		base.Update();

		if (waveTimer >= bossWave.startTime && !bossWaveInitialized)
		{
			InitializeBossWave();
		}

		if (BossWaveActive && PlayerControl.Instance.IsDead)
		{
			BossWaveTimer.Instance.Hide();
			DOTween.To(() => Parallax.OverrideSpeed.Value, x => Parallax.OverrideSpeed = x, 0f, 2f)
							.SetEase(Ease.OutSine);
		}
	}
	#endregion

	#region Boss Wave Methods
	protected override void InitializeBossWave()
	{
		PowerupSpawner.Instance.spawnPowerups = false;
		bossInstance = Instantiate(bossWave.boss, bossWave.spawner.position, Quaternion.identity) as Boss;
		bossInstance.Spawn();

		bossWaveInitialized = true;
	}

	public override void StartBossWave()
	{
		worldBoundaries.SetActive(false);
		runningBoundaries.SetActive(true);

		PlayerControl.Instance.continuouslyRunning = true;

		Cutscene.Instance.Hide(true);

		foreach (GameObject element in scrollingElements)
		{
			element.GetComponent<Parallax>().scroll = true;
		}

		DOTween.To(x => Parallax.OverrideSpeed = x, bossWave.cameraSpeed, bossWave.fullCameraSpeed, bossWave.speedUpTime)
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

		BossWaveActive = true;
	}

	public override void CompleteBossWave()
	{
		PlayerControl.Instance.DisableInput();
		PlayerControl.Instance.AddPoints(Mathf.RoundToInt(BossWaveTimer.Instance.Timer * timeBonusMultiplier), true);
		DOTween.To(() => BossWaveTimer.Instance.Timer, x => BossWaveTimer.Instance.Timer = x, 0f, 1f);
		DOTween.To(() => Parallax.OverrideSpeed.Value, x => Parallax.OverrideSpeed = x, 0f, 0.5f)
					.SetEase(Ease.OutQuint)
					.OnComplete(() => PlayerControl.Instance.continuouslyRunning = false);

		StartCoroutine(GameMenu.Instance.GameWin());
	}
	#endregion
}
