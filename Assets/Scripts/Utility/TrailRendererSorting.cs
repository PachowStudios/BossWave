using UnityEngine;
using System.Collections;

public class TrailRendererSorting : MonoBehaviour 
{
	public string sortingLayer;
	public int sortingOrder;

	private TrailRenderer trailRenderer;

	private void Awake()
	{
		trailRenderer = GetComponent<TrailRenderer>();
		trailRenderer.sortingLayerName = sortingLayer;
		trailRenderer.sortingOrder = sortingOrder;
	}
}
