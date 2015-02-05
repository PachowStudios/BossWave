using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	private static CameraFollow instance;

	public float defaultYOffset = 10f;
	public float platformYOffset = 5f;
	public float smoothing = 1f;
	public Transform followTarget;
	public bool stayAboveGroundLevel = true;

	private float currentYOffset;
	private bool usePlayerY = false;
	private bool lockX = false;
	private Vector3 targetPosition = new Vector3();
	private Vector3 previousTargetPosition = new Vector3();
	private Vector3 previousPosition;

	public static CameraFollow Instance
	{
		get { return instance; }
	}

	public Vector3 DeltaMovement
	{
		get
		{
			return transform.position - previousPosition;
		}
	}

	private void Awake()
	{
		instance = this;

		currentYOffset = defaultYOffset;

		targetPosition.y = followTarget.position.y + currentYOffset;
		previousPosition = transform.position;
	}

	private void FixedUpdate()
	{
		previousPosition = transform.position;
		previousTargetPosition = targetPosition;

		if (!lockX)
		{
			targetPosition.x = followTarget.position.x;
		}

		targetPosition.z = transform.position.z;

		if (usePlayerY || followTarget.tag == "Player")
		{
			currentYOffset = PlayerControl.Instance.transform.position.y - 1f > LevelManager.Instance.GroundLevel.y ? platformYOffset : defaultYOffset;

			if (PlayerControl.Instance.IsGrounded ||
				(PlayerControl.Instance.Velocity.y < 0f &&
				 PlayerControl.Instance.transform.position.y + currentYOffset < targetPosition.y))
			{
				targetPosition.y = PlayerControl.Instance.transform.position.y + currentYOffset;
			}

			targetPosition.y = Mathf.Max(targetPosition.y, LevelManager.Instance.GroundLevel.y + defaultYOffset);
		}
		else
		{
			targetPosition.y = followTarget.position.y + currentYOffset;
		}

		transform.localPosition = Extensions.SuperSmoothLerp(transform.localPosition, previousTargetPosition, targetPosition, Time.deltaTime, smoothing);
	}

	public void FollowObject(Transform target, bool newUsePlayerY, float newYOffset = -1f, bool newLockX = false)
	{
		currentYOffset = newYOffset == -1f ? defaultYOffset : newYOffset;
		usePlayerY = newUsePlayerY;
		lockX = newLockX;
		followTarget = target;
		targetPosition.x = followTarget.position.x;
	}
}
