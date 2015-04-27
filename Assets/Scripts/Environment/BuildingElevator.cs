using UnityEngine;
using System.Collections;

public sealed class BuildingElevator : MonoBehaviour
{
	#region Fields
	public BuildingFloor currentFloor;

	[SerializeField]
	private Transform waitPoint;

	public LayerMask collisionLayers;
	public string insideSortingLayer = "Back Background";
	public int insideSortingOrder = 1;

	private bool elevatorSelected = false;
	private bool playerInside = false;
	#endregion

	#region Public Properties
	public bool IsPlayerInside
	{ get { return playerInside; } }
	#endregion

	#region MonoBehaviour
	private void Update()
	{
		if (elevatorSelected && CrossPlatformInputManager.GetButtonDown("Interact"))
			EnterElevator();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player")
			elevatorSelected = true;
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerEnter2D(other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "Player")
			elevatorSelected = false;
	}
	#endregion

	#region Internal Helper Methods
	private void EnterElevator()
	{
		if (playerInside || !currentFloor.IsElevatorOpen)
			return;

		playerInside = true;
		PlayerControl.Instance.DisableInput();
		PlayerControl.Instance.SetFullSorting(insideSortingLayer, insideSortingOrder);
		PlayerControl.Instance.SetCollisionLayers(collisionLayers);
	}
	#endregion

	#region Public Methods
	public void CenterPlayer()
	{
		PlayerControl.Instance.transform.position = waitPoint.position;
	}

	public void ExitElevator()
	{
		if (!playerInside || !currentFloor.IsElevatorOpen)
			return;

		playerInside = false;
		PlayerControl.Instance.RestoreDefaultCollisionLayers();
		PlayerControl.Instance.RestoreDefaultFullSorting();
		PlayerControl.Instance.EnableInput();
	}
	#endregion
}
