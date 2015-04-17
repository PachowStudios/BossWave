using UnityEngine;
using System.Collections;

[RequireComponent(typeof(StandardEnemy))]
public abstract class SpawnAI : MonoBehaviour
{
	#region Fields
	public Transform simulateSpawner;

	protected Vector3 spawnPoint;

	protected StandardEnemy thisEnemy;
	protected Animator anim;
	protected SpriteRenderer spriteRenderer;
	protected CharacterController2D controller;
	#endregion

	#region Properties
	public virtual Transform Spawner
	{ 
		set 
		{ 
			thisEnemy.spawned = false;
			spawnPoint = value.FindChild("Spawn").position;
		}
	}
	#endregion

	#region Initialization Methods
	public virtual void Initialize(StandardEnemy thisEnemy, 
								   Animator anim, 
								   SpriteRenderer spriteRenderer,
								   CharacterController2D controller)
	{
		this.thisEnemy = thisEnemy;
		this.anim = anim;
		this.spriteRenderer = spriteRenderer;
		this.controller = controller;

		if (simulateSpawner != null)
		{
			Spawner = simulateSpawner;
		}
	}
	#endregion

	#region Spawn Methods
	public abstract void CheckSpawn();
	protected abstract void Spawn();
	#endregion
}
