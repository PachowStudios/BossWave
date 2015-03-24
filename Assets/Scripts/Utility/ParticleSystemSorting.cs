using UnityEngine;
using System.Collections;

public class ParticleSystemSorting : MonoBehaviour
{
	#region Fields
	public string sortingLayer;
	public int sortingOrder;

	ParticleSystem partSystem;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		partSystem = GetComponent<ParticleSystem>();
		partSystem.renderer.sortingLayerName = sortingLayer;
		partSystem.renderer.sortingOrder = sortingOrder;
	}
	#endregion
}
