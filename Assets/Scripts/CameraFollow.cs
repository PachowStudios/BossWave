using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
	public Vector2 margin = new Vector2(1f, 1f);
	public Vector2 smooth = new Vector2(8f, 8f);
	public Vector2 minPosition;
	public Vector2 maxPosition;

	private Transform player;

	void Awake()
	{
		player = GameObject.Find("Player").transform;

		minPosition.x += camera.orthographicSize * camera.aspect;
		maxPosition.x -= camera.orthographicSize * camera.aspect;

		minPosition.y += camera.orthographicSize;
		maxPosition.y -= camera.orthographicSize;

		if (minPosition.x < 0.1f)
		{
			minPosition.x = 0f;
		}

		if (maxPosition.x < 0.1f)
		{
			maxPosition.x = 0f;
		}

		if (minPosition.y < 0.1f)
		{
			minPosition.y = 0f;
		}

		if (maxPosition.y < 0.1f)
		{
			minPosition.y = 0f;
		}
	}

	void FixedUpdate()
	{
		Vector2 targetPosition = transform.position;

		if (Mathf.Abs(transform.position.x - player.position.x) > margin.x)
		{
			targetPosition.x = Mathf.Lerp(transform.position.x, player.position.x, smooth.x * Time.deltaTime);
		}

		if (Mathf.Abs(transform.position.y - player.position.y) > margin.y)
		{
			targetPosition.y = Mathf.Lerp(transform.position.y, player.position.y, smooth.y * Time.deltaTime);
		}

		targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
		targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);

		transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
	}
}
