using UnityEngine;
using System.Collections;

public sealed class BuildingElevator : MonoBehaviour
{
	#region Fields
	public float maxHealth = 100f;
	public BuildingFloor currentFloor;

	public LayerMask collisionLayers;
	public string insideSortingLayer = "Back Background";
	public int insideSortingOrder = 1;

	[SerializeField]
	private Transform waitPoint;

	private float health;
	private bool elevatorSelected = false;
	private bool playerInside = false;
	#endregion

	#region Public Properties
	public float Health
	{ get { return health; } }

	public float HealthPercent
	{ get { return Mathf.Clamp01(Health / maxHealth); } }

	public bool IsPlayerInside
	{ get { return playerInside; } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		health = maxHealth;
	}

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
		PlayerControl.Instance.GoToPoint(waitPoint.position, true, false, false);
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
