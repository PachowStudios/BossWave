using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Parallax : MonoBehaviour
{
	#region Types
	public enum ScrollDirection
	{
		Horizontal,
		Vertical
	};
	#endregion

	#region Fields
	public static float? OverrideSpeed = null;

	public float defaultSpeed = 17.5f;
	[Range(0f, 1f)]
	public float relativeSpeed = 1f;
	public ScrollDirection scrollDirection = ScrollDirection.Horizontal;
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
				layers.Add(child);

			if (scrollDirection == ScrollDirection.Horizontal)
				layers = layers.OrderBy(t => t.position.x).ToList();
			else
				layers = layers.OrderBy(t => t.position.y).ToList();
		}
	}

	private void Update()
	{
		if (scroll)
		{
			float speed = OverrideSpeed ?? defaultSpeed;
			Vector2 scrollVector = scrollDirection == ScrollDirection.Horizontal ? new Vector2(-(relativeSpeed * speed), 0f)
																				 : new Vector2(0f, -(relativeSpeed * speed));
			transform.Translate(scrollVector * Time.deltaTime);

			Transform firstChild = layers.FirstOrDefault();

			if (firstChild != null)
			{
				if ((scrollDirection == ScrollDirection.Horizontal && firstChild.position.x < Camera.main.transform.position.x) ||
					(scrollDirection == ScrollDirection.Vertical   && firstChild.position.y < Camera.main.transform.position.y))
				{
					if (!firstChild.renderer.IsVisibleFrom(Camera.main))
					{
						if (loop)
						{
							Transform lastChild = layers.LastOrDefault();
							Vector3 lastSize = lastChild.renderer.bounds.max - lastChild.renderer.bounds.min;

							if (scrollDirection == ScrollDirection.Horizontal)
								firstChild.position = new Vector3(lastChild.position.x + lastSize.x,
																  firstChild.position.y,
																  firstChild.position.z);
							else
								firstChild.position = new Vector3(lastChild.position.x,
																  firstChild.position.y + lastSize.y,
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
