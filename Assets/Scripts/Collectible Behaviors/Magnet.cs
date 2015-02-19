using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class Magnet : MonoBehaviour
{
	#region Fields
	public float magnetForce = 50f;
	public float magnetUplift = 10f;
	public float lookDamping = 3f;

	private CircleCollider2D magnetCollider;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		magnetCollider = GetComponent<CircleCollider2D>();
	}

	private void OnTriggerStay2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			rigidbody2D.AddExplosionForce(-magnetForce, PlayerControl.Instance.transform.position + new Vector3(0f, 2f, 0f), magnetCollider.radius * 2, magnetUplift);
			Vector3 newRotation = transform.position.LookAt2D(PlayerControl.Instance.transform.position).eulerAngles;
			newRotation.z -= 90f;
			gameObject.RotateUpdate(newRotation, lookDamping);
		}
	}

	private void OnTriggerExit2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			gameObject.RotateTo(new Vector3(0, 0, 0), lookDamping / 2f, 0f);
		}
	}
	#endregion
}
