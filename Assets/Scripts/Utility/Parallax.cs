using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Parallax : MonoBehaviour
{
	#region Fields
	public static float? OverrideSpeed = null;

	public float defaultSpeed = 17.5f;
	[Range(0f, 1f)]
	public float relativeSpeed = 1f;
	public bool scroll = false;
	public bool loop = false;
	public bool cameraParallax = false;

	private List<Transform> layers = new List<Transform>();
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			
			if (child.renderer != null && child.gameObject.activeSelf)
			{
				layers.Add(child);
			}

			layers = layers.OrderBy(t => t.position.x).ToList();
		}
	}

	private void Update()
	{
		if (scroll)
		{
			float speed = OverrideSpeed ?? defaultSpeed;

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
	#endregion

	#region Public Methods
	public void AddEndcap(Transform endcap)
	{
		layers.RemoveAll(l => 
		{
			if (!l.renderer.IsVisibleFrom(Camera.main))
			{
				Destroy(l.gameObject);
				return true;
			}
			else
			{
				return false;
			}
		});

		Transform lastChild = layers.LastOrDefault();
		Vector3 lastSize = lastChild.renderer.bounds.max - lastChild.renderer.bounds.min;

		endcap.position = new Vector3(lastChild.position.x + lastSize.x,
									  endcap.position.y,
									  endcap.position.z);
		endcap.gameObject.SetActive(true);

		layers.Add(endcap);
		loop = false;
	}
	#endregion
}
