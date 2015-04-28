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
	public Parallax coverScrolling;
	public Parallax floorScrolling;
	public List<FloorWave> floorWaves;
	public float elevatorSpeed = 100f;
	public float elevatorTransitionTime = 2f;
	public float elevatorOpenTime = 0.6f;
	public Ease elevatorStartEase = Ease.InCubic;

	FloorWave currentFloor;
	private int currentFloorIndex = 0;
	private ElevatorState elevatorState = ElevatorState.Closed;
	private float elevatorLeftTime = 0f;
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

		if (elevatorState == ElevatorState.Closed)
		{
			if (waveTimer >= currentFloor.elevatorOpenTime)
			{
				elevatorState = ElevatorState.Open;
				currentFloor.mainFloor.OpenElevator();
			}
			else
			{
				currentFloor.mainFloor.SetElevatorPercentage(ElevatorMeterPercentage);
			}
		}
		else if (elevatorState == ElevatorState.Open)
		{
			if (elevator.IsPlayerInside || waveTimer >= currentFloor.elevatorLeaveTime)
			{
				elevatorState = ElevatorState.Moving;
				StartCoroutine(StartElevator());
			}
		}
		else if (elevatorState == ElevatorState.Moving)
		{
			if (!currentFloor.layersSwitched && ElevatorMovingPercentage >= currentFloor.switchPercentage)
			{
				currentFloor.layersSwitched = true;
				floorScrolling.AddLayers(currentFloor.newScrolling, instantiate: true);
			}

			if (waveTimer >= currentFloor.elevatorArriveTime - elevatorTransitionTime)
			{
				elevatorState = ElevatorState.Arriving;
				currentFloor.mainFloor = Instantiate(currentFloor.mainFloor, new Vector3(0f, 20f, 0f), Quaternion.identity) as BuildingFloor;
				elevator.currentFloor = currentFloor.mainFloor;
				floorScrolling.AddLayerOnce(currentFloor.mainFloor.transform);
			}
		}
		else if (elevatorState == ElevatorState.Arriving)
		{
			float currentDeceleration = Extensions.CalculateDecelerationRate(coverScrolling.defaultSpeed, 0f, ElevatorPositionOffset) * Time.deltaTime;
			coverScrolling.defaultSpeed = Mathf.Max(coverScrolling.defaultSpeed - currentDeceleration, 0f);
			floorScrolling.defaultSpeed = Mathf.Max(floorScrolling.defaultSpeed - currentDeceleration, 0f);

			if (coverScrolling.defaultSpeed == 0f)
			{
				elevatorState = ElevatorState.Closed;
				StartCoroutine(StopElevator());
			}
		}
	}
	#endregion

	#region Internal Helper Methods
	private IEnumerator StartElevator()
	{
		currentFloor.mainFloor.CloseElevator();
		currentFloorIndex++;
		currentFloor = floorWaves[currentFloorIndex];

		yield return new WaitForSeconds(elevatorOpenTime);

		elevator.CenterPlayer();
		elevatorLeftTime = waveTimer;

		DOTween.To(s => coverScrolling.defaultSpeed = s, 0f, elevatorSpeed, elevatorTransitionTime)
			.OnStart(() => coverScrolling.scroll = true)
			.SetEase(elevatorStartEase);
		DOTween.To(s => floorScrolling.defaultSpeed = s, 0f, elevatorSpeed, elevatorTransitionTime)
			.OnStart(() => floorScrolling.scroll = true)
			.SetEase(elevatorStartEase);

		floorScrolling.AddLayers(currentFloor.oldScrolling, instantiate: true);
		TransparentForeground.allowHiding = false;
	}

	private IEnumerator StopElevator()
	{
		TransparentForeground.allowHiding = true;
		coverScrolling.scroll = false;
		floorScrolling.scroll = false;

		if (ElevatorPositionOffset != 0f)
		{
			Vector3 elevatorPositionOffsetVector = new Vector3(0f, -ElevatorPositionOffset, 0f);
			coverScrolling.OffsetLayers(elevatorPositionOffsetVector);
			floorScrolling.OffsetLayers(elevatorPositionOffsetVector);
		}

		currentFloor.mainFloor.OpenElevator();

		yield return new WaitForSeconds(elevatorOpenTime);

		elevator.ExitElevator();
		currentFloor.mainFloor.CloseElevator();
	}
	#endregion

	#region Boss Wave Methods
	protected override void InitializeBossWave()
	{
		
	}

	public override void StartBossWave()
	{
		
	}

	public override void CompleteBossWave()
	{
		
	}
	#endregion
}
