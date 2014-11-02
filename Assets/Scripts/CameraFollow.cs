using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public Vector2 margin = new Vector2(70f, 70f);
	public float smoothing = 3f;
	public Transform follow;

	public Vector2 marginUnits;

	private Vector3 velocity = Vector3.zero;

	void Awake()
	{
		marginUnits = new Vector2(camera.orthographicSize * camera.aspect * (margin.x / 100f),
								  camera.orthographicSize * (margin.y / 100f));
	}

	void FixedUpdate()
	{
		Vector3 targetPosition = transform.position;

		if (Mathf.Abs(transform.position.x - follow.position.x) > marginUnits.x)
		{
			targetPosition.x = follow.position.x;
		}

		if (Mathf.Abs(transform.position.y - follow.position.y) > marginUnits.y)
		{
			targetPosition.y = follow.position.y;
		}

		transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothing);
	}

	public void FollowObject(Transform target, bool explicitX = false, bool explicitY = false)
	{
		follow = target;

		marginUnits.x = explicitX ? 0.01f : camera.orthographicSize * camera.aspect * (margin.x / 100f);
		marginUnits.y = explicitY ? 0.01f : camera.orthographicSize * (margin.y / 100f);
	}
}
