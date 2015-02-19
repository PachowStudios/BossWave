using UnityEngine;
using System.Collections;

public class TrailRendererSorting : MonoBehaviour
{
	#region Fields
	public string sortingLayer;
	public int sortingOrder;

	private TrailRenderer trailRenderer;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		trailRenderer = GetComponent<TrailRenderer>();
		trailRenderer.sortingLayerName = sortingLayer;
		trailRenderer.sortingOrder = sortingOrder;
	}
	#endregion
}
