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

	#region Internal Properties
	private Transform LastChild
	{ get { return layers.LastOrDefault(); } }

	private Vector3 LastSize
	{ get { return LastChild.renderer.bounds.max - LastChild.renderer.bounds.min; } }

	private Vector3 NewLayerPosition
	{
		get
		{
			Transform firstChild = layers.FirstOrDefault();

			if (scrollDirection == ScrollDirection.Horizontal)
				return new Vector3(LastChild.position.x + LastSize.x,
								   firstChild.position.y,
								   firstChild.position.z);
			else
				return new Vector3(firstChild.position.x,
								   LastChild.position.y + LastSize.y,
								   firstChild.position.z);
		}
	}
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

			foreach (Transform layer in layers)
				layer.Translate(scrollVector * Time.deltaTime);

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
							firstChild.position = NewLayerPosition;

							if (firstChild.tag == "ScrollOnce")
							{
								layers.Remove(firstChild);
								Destroy(firstChild.gameObject);
							}
							else
							{
								layers.Remove(firstChild);
								layers.Add(firstChild);
							}
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
	public void AddLayers(List<Transform> newLayers, bool replace = true, bool instantiate = false)
	{
		if (replace)
			foreach (Transform layer in layers)
				layer.tag = "ScrollOnce";

		foreach (Transform layer in newLayers)
		{
			Transform newLayer;

			if (instantiate)
			{
				newLayer = Instantiate(layer, NewLayerPosition, Quaternion.identity) as Transform;
				newLayer.parent = transform;
			}
			else
			{
				newLayer = layer;
				newLayer.parent = transform;
				newLayer.position = NewLayerPosition;
			}

			layers.Add(newLayer);
		}
	}

	public void AddLayerOnce(Transform newLayer)
	{
		newLayer.tag = "ScrollOnce";
		newLayer.parent = transform;
		newLayer.position = NewLayerPosition;
		layers.Add(newLayer);
	}

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

		endcap.position = NewLayerPosition;
		endcap.gameObject.SetActive(true);

		layers.Add(endcap);
		loop = false;
	}

	public void OffsetLayers(Vector3 offset)
	{
		foreach (Transform layer in layers)
			layer.Translate(offset);
	}
	#endregion
}
