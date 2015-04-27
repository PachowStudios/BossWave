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
				StartNextFloorWave();
			}
		}
		else if (elevatorState == ElevatorState.Moving)
		{
			if (!currentFloor.layersSwitched && ElevatorMovingPercentage >= currentFloor.switchPercentage)
			{
				currentFloor.layersSwitched = true;
				floorScrolling.AddLayers(currentFloor.newScrolling);
			}

			if (waveTimer >= currentFloor.elevatorArriveTime - elevatorTransitionTime)
			{
				elevatorState = ElevatorState.Arriving;
				currentFloor.mainFloor = Instantiate(currentFloor.mainFloor, new Vector3(0f, 20f, 0f), Quaternion.identity) as BuildingFloor;
				floorScrolling.AddLayerOnce(currentFloor.mainFloor.transform);
			}
		}
		else if (elevatorState == ElevatorState.Arriving)
		{

		}
	}
	#endregion

	#region Internal Helper Methods
	private void StartNextFloorWave()
	{
		currentFloor.mainFloor.CloseElevator();
		elevatorLeftTime = waveTimer;

		currentFloorIndex++;
		currentFloor = floorWaves[currentFloorIndex];

		DOTween.To(s => coverScrolling.defaultSpeed = s, 0f, elevatorSpeed, elevatorTransitionTime)
			.SetEase(elevatorStartEase);
		DOTween.To(s => floorScrolling.defaultSpeed = s, 0f, elevatorSpeed, elevatorTransitionTime)
			.SetEase(elevatorStartEase);

		floorScrolling.AddLayers(currentFloor.oldScrolling);
	}

	private float CalculateArrivalSpeed()
	{
		float timeToArrival = (currentFloor.elevatorArriveTime - elevatorTransitionTime) - waveTimer;
		return 0f;
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
