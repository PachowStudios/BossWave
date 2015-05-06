using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public sealed class Level2 : LevelManager
{
	#region Types
	[System.Serializable]
	public struct FloorWave
	{
		public float elevatorArriveTime;
		public float elevatorOpenTime;
		public float elevatorLeaveTime;
		[Range(0f, 1f)]
		public float switchPercentage;
		public bool layersSwitched;
		public BuildingFloor mainFloor;
		public List<Transform> oldScrolling;
		public List<Transform> newScrolling;
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
	}

	public enum ElevatorState
	{
		Closed,
		Open,
		Moving,
		Arriving
	};
	#endregion

	#region Fields
	public BuildingElevator elevator;
	public Parallax floorScrollingLayer;
	public Parallax coverScrollingLayer;
	public float elevatorSpeed = 100f;
	public float elevatorTransitionTime = 2f;
	public float elevatorDoorTime = 0.6f;
	public float elevatorWarningTime = 3f;
	public Ease elevatorStartEase = Ease.InCubic;
	public List<FloorWave> floorWaves;
	public BossWave bossWave;

	FloorWave currentFloor;
	private int currentFloorIndex = 0;
	private ElevatorState elevatorState = ElevatorState.Closed;
	private float elevatorLeftTime = 0f;
	private bool elevatorComplete = false;

	private Boss bossInstance;
	private Transform bossSpawner;
	private bool bossWaveInitialized = false;
	#endregion

	#region Public Properties
	public static new Level2 Instance
	{ get { return (Level2)instance; } }
	#endregion

	#region Internal Properties
	private float ElevatorMeterPercentage
	{ get { return Extensions.ConvertRange(waveTimer, currentFloor.elevatorArriveTime, currentFloor.elevatorOpenTime, 0f, 1f); } }

	private float ElevatorMovingPercentage
	{ get { return Extensions.ConvertRange(waveTimer, elevatorLeftTime, currentFloor.elevatorArriveTime, 0f, 1f); } }

	private float ElevatorPositionOffset
	{ get { return currentFloor.mainFloor.ElevatorPosition.y - elevator.transform.position.y; } }
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		currentFloor = floorWaves[currentFloorIndex];
	}

	protected override void Update()
	{
		base.Update();

		if (!elevatorComplete)
			UpdateElevator();

		if (waveTimer >= bossWave.startTime && !bossWaveInitialized)
			InitializeBossWave();
	}
	#endregion

	#region Internal Update Methods
	private void UpdateElevator()
	{
		switch (elevatorState)
		{
			case ElevatorState.Closed:
				UpdateElevatorClosed();
				break;
			case ElevatorState.Open:
				UpdateElevatorOpen();
				break;
			case ElevatorState.Moving:
				UpdateElevatorMoving();
				break;
			case ElevatorState.Arriving:
				UpdateElevatorArriving();
				break;
		}
	}

	private void UpdateElevatorClosed()
	{
		if (waveTimer >= currentFloor.elevatorOpenTime)
		{
			elevatorState = ElevatorState.Open;
			currentFloor.mainFloor.OpenElevator();
			Timer.Instance.StartTimer(currentFloor.elevatorLeaveTime - waveTimer, flashTime: elevatorWarningTime, onCompleteCallback: MissElevator);
		}
		else
		{
			currentFloor.mainFloor.SetElevatorPercentage(ElevatorMeterPercentage);
		}
	}

	private void UpdateElevatorOpen()
	{
		if (elevator.IsPlayerInside || waveTimer >= currentFloor.elevatorLeaveTime)
		{
			if (elevator.IsPlayerInside)
			{
				elevatorState = ElevatorState.Moving;
				Timer.Instance.StopTimer(true);
				StartCoroutine(StartElevator());
			}
		}
	}

	private void UpdateElevatorMoving()
	{
		if (!currentFloor.layersSwitched && ElevatorMovingPercentage >= currentFloor.switchPercentage)
		{
			currentFloor.layersSwitched = true;
			floorScrollingLayer.RemoveLayers();
			floorScrollingLayer.AddLayers(currentFloor.newScrolling, true);
		}

		if (waveTimer >= currentFloor.elevatorArriveTime - elevatorTransitionTime)
		{
			elevatorState = ElevatorState.Arriving;
			currentFloor.mainFloor = Instantiate(currentFloor.mainFloor, new Vector3(0f, 20f, 0f), Quaternion.identity) as BuildingFloor;
			elevator.currentFloor = currentFloor.mainFloor;
			floorScrollingLayer.RemoveLayers();
			floorScrollingLayer.AddLayer(currentFloor.mainFloor.transform, true);

			if (currentFloorIndex == floorWaves.Count - 1)
				coverScrollingLayer.SetLooping(false, false);
		}
	}

	private void UpdateElevatorArriving()
	{
		float currentDeceleration = Extensions.CalculateDecelerationRate(floorScrollingLayer.CurrentSpeed, 0f, ElevatorPositionOffset) * Time.deltaTime;
		SetScrollingSpeed(Mathf.Max(floorScrollingLayer.CurrentSpeed - currentDeceleration, 0f));

		if (floorScrollingLayer.CurrentSpeed == 0f || waveTimer >= currentFloor.elevatorArriveTime)
		{
			elevatorState = ElevatorState.Closed;
			StartCoroutine(StopElevator());

			if (currentFloorIndex == floorWaves.Count - 1)
				elevatorComplete = true;
		}
	}
	#endregion

	#region Internal Helper Methods
	private IEnumerator StartElevator()
	{
		currentFloor.mainFloor.CloseElevator();
		currentFloorIndex++;
		currentFloor = floorWaves[currentFloorIndex];
		elevatorLeftTime = waveTimer;

		yield return new WaitForSeconds(elevatorDoorTime);

		elevator.CenterPlayer();
		floorScrollingLayer.RemoveLayers();
		floorScrollingLayer.AddLayers(currentFloor.oldScrolling, true);
		BuildingCover.allowHiding = false;

		DOTween.To(s => SetScrollingSpeed(s), 0f, elevatorSpeed, elevatorTransitionTime)
			.OnStart(() => SetScrollingActive(true))
			.SetEase(elevatorStartEase);
	}

	private IEnumerator StopElevator()
	{
		BuildingCover.allowHiding = true;
		SetScrollingActive(false);
		SetScrollingSpeed(0f);

		if (ElevatorPositionOffset != 0f)
		{
			Vector3 elevatorPositionOffsetVector = new Vector3(0f, -ElevatorPositionOffset, 0f);
			OffsetScrollingLayers(elevatorPositionOffsetVector);
		}

		currentFloor.mainFloor.OpenElevator();

		yield return new WaitForSeconds(elevatorDoorTime);

		elevator.ExitElevator();
		currentFloor.mainFloor.CloseElevator();
	}

	private void MissElevator()
	{
		currentFloor.mainFloor.CloseElevator();
		StartCoroutine(GameMenu.Instance.GameOver("YOU MISSED THE ELEVATOR"));
	}

	private void SetScrollingSpeed(float speed)
	{
		Parallax.OverrideSpeed = speed;
	}

	private void SetScrollingActive(bool active)
	{
		Parallax.OverrideScroll = active;
	}

	private void OffsetScrollingLayers(Vector3 offset)
	{
		floorScrollingLayer.OffsetLayers(offset);
		coverScrollingLayer.OffsetLayers(offset);
	}
	#endregion

	#region Boss Wave Methods
	protected override void InitializeBossWave()
	{
		PowerupSpawner.Instance.spawnPowerups = false;
		bossSpawner = GameObject.FindGameObjectWithTag("BossSpawner").transform;
		bossInstance = Instantiate(bossWave.boss, bossSpawner.position, Quaternion.identity) as Boss;
		bossInstance.Spawn();

		bossWaveInitialized = true;
	}

	public override void StartBossWave()
	{
		
	}

	public override void CompleteBossWave()
	{
		
	}
	#endregion
}
