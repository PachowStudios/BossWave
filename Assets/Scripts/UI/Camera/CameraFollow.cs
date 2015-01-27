using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public static CameraFollow instance;

	public float yOffset = 0f;
	public float smoothing = 1f;
	public Transform follow;

	private float currentYOffset;
	private bool usePlayerY = false;
	private bool lockX = false;
	private Vector3 targetPosition = new Vector3();
	private Vector3 previousTargetPosition = new Vector3();
	private Vector3 previousPosition;

	public Vector3 DeltaMovement
	{
		get
		{
			return transform.position - previousPosition;
		}
	}

	void Awake()
	{
		instance = this;

		currentYOffset = yOffset;

		targetPosition.y = follow.position.y + currentYOffset;
		previousPosition = transform.position;
	}

	void FixedUpdate()
	{
		previousPosition = transform.position;
		previousTargetPosition = targetPosition;

		if (!lockX)
		{
			targetPosition.x = follow.position.x;
		}

		targetPosition.z = transform.position.z;

		if (usePlayerY || follow.tag == "Player")
		{
			if (PlayerControl.instance.IsGrounded ||
				(PlayerControl.instance.transform.position.y + currentYOffset < targetPosition.y &&
				PlayerControl.instance.Velocity.y < 0f))
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

	public void FollowObject(Transform target, bool newUsePlayerY, float newYOffset = -1f, bool newLockX = false)
	{
		currentYOffset = newYOffset == -1f ? yOffset : newYOffset;
		usePlayerY = newUsePlayerY;
		lockX = newLockX;
		follow = target;
		targetPosition.x = follow.position.x;
	}
}
