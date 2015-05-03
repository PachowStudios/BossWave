using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
	#region Fields
	public Portal exitPortal;

	private bool portalSelected = false;
	#endregion

	#region Public Properties
	public bool HasExit
	{ get { return exitPortal != null; } }

	public bool IsTwoWay
	{ get { return HasExit && exitPortal.exitPortal == this; } }
	#endregion

	#region MonoBehaviour
	private void Update()
	{
		if (portalSelected && CrossPlatformInputManager.GetButtonDown("Interact"))
			EnterPortal();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player")
			portalSelected = true;
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerEnter2D(other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "Player")
			portalSelected = false;
	}

	private void OnDrawGizmosSelected()
	{
		if (HasExit)
		{
			Gizmos.color = IsTwoWay ? Color.green : Color.blue;
			Gizmos.DrawLine(transform.position, exitPortal.transform.position);
		}
	}
	#endregion

	#region Internal Helper Methods
	private void EnterPortal()
	{
		if (exitPortal == null)
			return;

		portalSelected = false;
		PlayerControl.Instance.transform.position = exitPortal.transform.position;
	}
	#endregion
}
