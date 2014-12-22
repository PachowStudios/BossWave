using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class Magnet : MonoBehaviour 
{
	public float magnetForce = 50f;
	public float magnetUplift = 10f;
	public float lookDamping = 3f;

	private CircleCollider2D magnetCollider;

	void Awake()
	{
		magnetCollider = GetComponent<CircleCollider2D>();
	}

	void OnTriggerStay2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			rigidbody2D.AddExplosionForce(-magnetForce, PlayerControl.instance.transform.position + new Vector3(0f, 2f, 0f), magnetCollider.radius * 2, magnetUplift);
			Vector3 newRotation = transform.position.LookAt2D(PlayerControl.instance.transform.position).eulerAngles;
			newRotation.z -= 90f;
			gameObject.RotateUpdate(newRotation, lookDamping);
		}
	}

	void OnTriggerExit2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			gameObject.RotateTo(new Vector3(0, 0, 0), lookDamping / 2f, 0f);
		}
	}
}
