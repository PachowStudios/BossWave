using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public Vector2 margin = new Vector2(70f, 70f);
	public Vector2 smooth = new Vector2(8f, 8f);
	public Transform follow;

	public Vector2 marginUnits;

	void Awake()
	{
		marginUnits = new Vector2(camera.orthographicSize * camera.aspect * (margin.x / 100f),
								  camera.orthographicSize * (margin.y / 100f));
	}

	void FixedUpdate()
	{
		Vector2 targetPosition = transform.position;

		if (Mathf.Abs(transform.position.x - follow.position.x) > marginUnits.x)
		{
			targetPosition.x = Mathf.Lerp(transform.position.x, follow.position.x, smooth.x * Time.deltaTime);
		}

		if (Mathf.Abs(transform.position.y - follow.position.y) > marginUnits.y)
		{
			targetPosition.y = Mathf.Lerp(transform.position.y, follow.position.y, smooth.y * Time.deltaTime);
		}

		transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
	}

	public void FollowObject(Transform target, bool explicitX = false, bool explicitY = false)
	{
		follow = target;

		marginUnits.x = explicitX ? 0.01f : camera.orthographicSize * camera.aspect * (margin.x / 100f);
		marginUnits.y = explicitY ? 0.01f : camera.orthographicSize * (margin.y / 100f);
	}
}
