using UnityEngine;
using System.Collections;

public class BuildingFloor : MonoBehaviour
{
	#region Fields
	[SerializeField]
	private Animator elevator;

	private bool elevatorOpen = false;
	#endregion

	#region Public Properties
	public Vector3 ElevatorPosition
	{ get { return elevator.transform.position; } }

	public bool IsElevatorOpen
	{ get { return elevatorOpen; } }
	#endregion

	#region Public Methods
	public void SetElevatorPercentage(float elevatorPercentage)
	{
		elevator.SetFloat("Meter", elevatorPercentage);
	}

	public void OpenElevator()
	{
		if (elevatorOpen)
			return;

		elevatorOpen = true;
		elevator.SetBool("Open", true);
	}

	public void CloseElevator()
	{
		if (!elevatorOpen)
			return;

		elevatorOpen = false;
		elevator.SetBool("Open", false);
	}
	#endregion
}
