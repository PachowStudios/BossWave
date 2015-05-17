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
	public static bool? OverrideScroll = null;
	public static bool? OverrideReverse = null;

	public float defaultSpeed = 17.5f;
	[Range(0f, 1f)]
	public float relativeSpeed = 1f;
	public ScrollDirection scrollDirection = ScrollDirection.Horizontal;
	public bool scroll = false;
	public bool loop = false;
	public bool reverse = false;
	public bool cameraParallax = false;
	public bool moveLayersSeparately = true;
	public bool destroyAfterScroll = true;

	private List<Transform> layers = new List<Transform>();
	private bool previousReverse;
	#endregion

	#region Public Properties
	public float Speed
	{ get { return (OverrideSpeed ?? defaultSpeed) * (Reverse ? -1f : 1f); } }

	public bool Scroll
	{ get { return OverrideScroll ?? scroll; } }

	public bool Reverse
	{ get { return OverrideReverse ?? reverse; } }
	#endregion

	#region Internal Properties
	private Transform FirstChild
	{ get { return layers.FirstOrDefault(); } }

	private Transform LastChild
	{ get { return layers.LastOrDefault(); } }

	private Vector3 FirstSize
	{ get { return FirstChild.renderer.bounds.max - FirstChild.renderer.bounds.min; } }

	private Vector3 LastSize
	{ get { return LastChild.renderer.bounds.max - LastChild.renderer.bounds.min; } }

	private Vector3 NewLayerPosition
	{
		get
		{
			if (scrollDirection == ScrollDirection.Horizontal)
				return new Vector3(LastChild.position.x + (Reverse ? -LastSize.x : LastSize.x),
								   FirstChild.position.y,
								   FirstChild.position.z);
			else
				return new Vector3(FirstChild.position.x,
								   LastChild.position.y + (Reverse ? -LastSize.y : LastSize.y),
								   FirstChild.position.z);
		}
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		previousReverse = Reverse;

		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			
			if (child.renderer != null && child.gameObject.activeSelf)
				layers.Add(child);	
		}

		SortLayers();
	}

	private void Update()
	{
		if (Reverse && !previousReverse)
			SortLayers();

		if (Scroll)
		{
			Vector2 scrollVector = scrollDirection == ScrollDirection.Horizontal ? new Vector2(-(relativeSpeed * Speed), 0f)
																				 : new Vector2(0f, -(relativeSpeed * Speed));

			if (moveLayersSeparately)
			{
				foreach (Transform layer in layers)
					if (layer != null)
						layer.Translate(scrollVector * Time.deltaTime);
			}
			else
			{
				transform.Translate(scrollVector * Time.deltaTime);
			}

			var nextChild = FirstChild;

			if (nextChild != null)
			{
				if ((scrollDirection == ScrollDirection.Horizontal && (Reverse ? nextChild.position.x > Camera.main.transform.position.x
																			   : nextChild.position.x < Camera.main.transform.position.x)) ||
					(scrollDirection == ScrollDirection.Vertical   && (Reverse ? nextChild.position.y > Camera.main.transform.position.y
																			   : nextChild.position.y < Camera.main.transform.position.y)))
				{
					if (!nextChild.renderer.IsVisibleFrom(Camera.main))
					{
						if (loop)
						{
							nextChild.position = NewLayerPosition;

							if (nextChild.tag == "ScrollOnce")
							{
								layers.Remove(nextChild);
								Destroy(nextChild.gameObject);
							}
							else
							{
								layers.Remove(nextChild);
								layers.Add(nextChild);
							}
						}
						else if (destroyAfterScroll)
						{
							layers.Remove(nextChild);
							Destroy(nextChild.gameObject);
						}

						if (nextChild != null)
							nextChild.SendMessage("OnScrolled", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		else if (cameraParallax)
		{
			transform.Translate((1 - relativeSpeed) * CameraFollow.Instance.DeltaMovement);
		}

		previousReverse = Reverse;
	}
	#endregion

	#region Internal Helper Methods
	private void SortLayers()
	{
		if (scrollDirection == ScrollDirection.Horizontal)
			layers = Reverse ? layers.OrderByDescending(t => t.position.x).ToList()
							 : layers.OrderBy(t => t.position.x).ToList();
		else
			layers = Reverse ? layers.OrderByDescending(t => t.position.y).ToList()
							 : layers.OrderBy(t => t.position.y).ToList();
	}
	#endregion

	#region Public Methods
	public void AddLayers(List<Transform> newLayers, bool instantiate = false, bool sortFirst = false)
	{
		if (sortFirst)
			SortLayers();

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

	public void AddLayer(Transform newLayer, bool scrollOnce = false)
	{
		if (scrollOnce)
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

	public void RemoveLayers()
	{
		foreach (Transform layer in layers)
			layer.tag = "ScrollOnce";
	}

	public void SetLooping(bool loop, bool destroyAfterScroll = true)
	{
		this.loop = loop;
		this.destroyAfterScroll = destroyAfterScroll;
	}

	public void OffsetLayers(Vector3 offset)
	{
		foreach (Transform layer in layers)
			layer.Translate(offset);
	}
	#endregion
}
