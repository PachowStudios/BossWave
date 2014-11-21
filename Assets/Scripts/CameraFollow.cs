using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public Vector2 margin = new Vector2(70f, 70f);
	public float smoothing = 3f;
	public Transform follow;

	private Vector3 velocity = Vector3.zero;

	private float marginUnitsX
	{
		get
		{
			return camera.orthographicSize * camera.aspect * (margin.x / 100f);
		}
	}

	private float marginUnitsY
	{
		get
		{
			return camera.orthographicSize * (margin.y / 100f);
		}
	}

	void FixedUpdate()
	{
		Vector3 targetPosition = transform.position;

		if (Mathf.Abs(transform.position.x - follow.position.x) > marginUnitsX)
		{
			targetPosition.x = follow.position.x;
		}

		if (Mathf.Abs(transform.position.y - follow.position.y) > marginUnitsY)
		{
			targetPosition.y = follow.position.y;
		}

		transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothing);
	}

	public void FollowObject(Transform target, Vector2 newMargin)
	{
		follow = target;

		margin.x = newMargin.x == 0f ? margin.x : newMargin.x;
		margin.y = newMargin.y == 0f ? margin.y : newMargin.y;
	}
}
