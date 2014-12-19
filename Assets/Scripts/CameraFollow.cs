using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public float yOffset = 0f;
	public float smoothing = 1f;
	public Transform follow;

	private float currentYOffset;
	private bool usePlayerY = false;
	private Vector3 targetPosition = new Vector3();
	private Vector3 previousTargetPosition = new Vector3();

	void Awake()
	{
		currentYOffset = yOffset;

		targetPosition.y = follow.position.y + currentYOffset;
	}

	void FixedUpdate()
	{
		previousTargetPosition = targetPosition;

		targetPosition.x = follow.position.x;
		targetPosition.z = transform.position.z;

		if (usePlayerY || follow.tag == "Player")
		{
			if (PlayerControl.instance.IsGrounded)
			{
				targetPosition.y = PlayerControl.instance.transform.position.y + currentYOffset;
			}
		}
		else
		{
			targetPosition.y = follow.position.y + currentYOffset;
		}

		transform.position = Extensions.SuperSmoothLerp(transform.position, previousTargetPosition, targetPosition, Time.deltaTime, smoothing);
	}

	public void FollowObject(Transform target, bool newUsePlayerY, float newYOffset = -1f)
	{
		currentYOffset = newYOffset == -1f ? yOffset : newYOffset;
		usePlayerY = newUsePlayerY;
		follow = target;
	}
}
