using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Parallax : MonoBehaviour 
{
	public float defaultSpeed = 17.5f;
	[Range(0f, 1f)]
	public float relativeSpeed = 1f;
	public bool scroll = false;
	public bool loop = false;
	public bool cameraParallax = false;

	private List<Transform> layers = new List<Transform>();

	void Awake()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			
			if (child.renderer != null)
			{
				layers.Add(child);
			}

			layers = layers.OrderBy(t => t.position.x).ToList();
		}
	}

	void FixedUpdate()
	{
		if (scroll)
		{
			float speed = (LevelManager.Instance == null) ? defaultSpeed : LevelManager.Instance.bossWave.cameraSpeed;

			transform.Translate(new Vector2(-(relativeSpeed * speed), 0) * Time.deltaTime);

			Transform firstChild = layers.FirstOrDefault();

			if (firstChild != null)
			{
				if (firstChild.position.x < Camera.main.transform.position.x)
				{
					if (!firstChild.renderer.IsVisibleFrom(Camera.main))
					{
						if (loop)
						{
							Transform lastChild = layers.LastOrDefault();
							Vector3 lastSize = lastChild.renderer.bounds.max - lastChild.renderer.bounds.min;

							firstChild.position = new Vector3(lastChild.position.x + lastSize.x,
															  firstChild.position.y,
															  firstChild.position.z);

							layers.Remove(firstChild);
							layers.Add(firstChild);
						}
						else
						{
							layers.Remove(firstChild);
							Destroy(firstChild.gameObject);
						}
					}
				}
			}
		}
		else if (cameraParallax)
		{
			transform.Translate((1 - relativeSpeed) * CameraFollow.Instance.DeltaMovement);
		}
	}
}
