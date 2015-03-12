using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class DamageParticleEffect : MonoBehaviour
{
	#region Fields
	public ParticleSystem particlePrefab;
	public Vector3 spawnPosition;
	[Range(0f, 1f)]
	public float activateHealth = 0.25f;
	public string sortingLayer = "Effects";
	public int sortingOrder = 0;

	private Enemy thisEnemy;
	private ParticleSystem particleInstance;
	#endregion

	#region Internal Properties
	private bool CanActivate
	{
		get
		{
			return particleInstance == null && 
				   thisEnemy != null &&
				   thisEnemy.HealthPercentage <= activateHealth;
		}
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		thisEnemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		if (CanActivate)
		{
			SpawnParticles();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.TransformPoint(spawnPosition), 0.1f);
	}
	#endregion

	#region Internal Helper Methods
	private void SpawnParticles()
	{
		particleInstance = Instantiate(particlePrefab) as ParticleSystem;
		particleInstance.transform.parent = transform;
		particleInstance.transform.localPosition = spawnPosition;
		particleInstance.renderer.sortingLayerName = sortingLayer;
		particleInstance.renderer.sortingOrder = sortingOrder;
	}
	#endregion
}