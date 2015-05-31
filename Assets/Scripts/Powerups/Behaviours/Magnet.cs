using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public class Magnet : MonoBehaviour
{
	#region Fields
	public float magnetForce = 50f;
	public float magnetUplift = 10f;
	public float lookDamping = 3f;
	public Vector3 playerPositionOffset = new Vector3(0f, 2f, 0f);

	private bool inRange = false;
	private float rotateVelocity = 0f;

	private CircleCollider2D magnetCollider;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		magnetCollider = GetComponent<CircleCollider2D>();
	}

	private void FixedUpdate()
	{
		float newRotation;

		if (inRange)
		{
			rigidbody2D.AddExplosionForce(-magnetForce, 
										  PlayerControl.Instance.transform.position + playerPositionOffset, 
										  magnetCollider.radius * 2, 
										  magnetUplift);
			newRotation = transform.position.LookAt2D(PlayerControl.Instance.transform.position + playerPositionOffset).eulerAngles.z - 90f;
			
		}
		else
		{
			newRotation = 0f;
		}

		transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x,
														  transform.rotation.eulerAngles.y,
														  Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, 
																				newRotation, 
																				ref rotateVelocity, 
																				lookDamping)));
	}

	private void OnTriggerEnter2D(Collider2D trigger)
	{
		OnTriggerStay2D(trigger);
	}

	private void OnTriggerStay2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			inRange = true;
		}
	}

	private void OnTriggerExit2D(Collider2D trigger)
	{
		if (trigger.tag == "Player")
		{
			inRange = false;
		}
	}
	#endregion
}
