using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Enemy))]
public class DeathSpriteEffect : MonoBehaviour
{
	#region Types
	[Serializable]
	public struct Effect
	{
		public string deathEffect;
		public Vector3 spawnPosition;
		public float delay;
	}
	#endregion

	#region Fields
	public List<Effect> deathEffects = new List<Effect>();
	public bool parentEffects = false;

	private Enemy thisEnemy;
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		thisEnemy = GetComponent<Enemy>();
	}

	private void OnEnable()
	{
		if (thisEnemy != null)
			thisEnemy.OnDeath += SpawnEffects;
	}

	private void OnDisable()
	{
		if (thisEnemy != null)
			thisEnemy.OnDeath -= SpawnEffects;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.grey;

		foreach (var effect in deathEffects)
			Gizmos.DrawSphere(transform.TransformPoint(effect.spawnPosition), 0.1f);
	}
	#endregion

	#region Internal Helper Methods
	private void SpawnEffects()
	{
		foreach (var effect in deathEffects)
			SpriteEffect.Instance.SpawnEffect(effect.deathEffect,
											  (parentEffects ? effect.spawnPosition : transform.TransformPoint(effect.spawnPosition)),
											  positionIsLocal: parentEffects,
											  parent: (parentEffects ? transform : null),
											  delay: effect.delay);
	}
	#endregion
}
