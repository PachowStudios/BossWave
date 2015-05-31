using UnityEngine;
using System.Collections;
using DG.Tweening;

public sealed class AssemblyLineSpawnAI : SpawnAI
{
	#region Fields
	public LayerMask spawnPlatformMask;
	public string spawnSortingLayer = "Spawn";
	public int spawnSortingOrder = 0;
	public Color spawnColor = new Color(0.133f, 0.137f, 0.153f, 1f);
	public float spawnEntryRange = 1f;
	public float spawnJumpHeight = 4f;
	public float spawnLength = 0.5f;

	private LayerMask defaultPlatformMask;
	private string defaultSortingLayer;
	private int defaultSortingOrder;
	private Color defaultColor;
	private Vector3 entryPoint;
	#endregion

	#region Properties
	public override Transform Spawner
	{
		set
		{
			base.Spawner = value;
			entryPoint = Extensions.Vector3Range(value.FindChild("Entry Start").position,
												 value.FindChild("Entry End").position);

			transform.position = spawnPoint;
			controller.platformMask = spawnPlatformMask;
			spriteRenderer.sortingLayerName = spawnSortingLayer;
			spriteRenderer.sortingOrder = spawnSortingOrder;
			spriteRenderer.color = spawnColor;

			thisEnemy.invincible = true;
			thisEnemy.ignoreProjectiles = true;
			thisEnemy.HorizontalMovement = -1f;
		}
	}
	#endregion

	#region Initialization Methods
	public override void Initialize(StandardEnemy thisEnemy, 
									Animator anim, 
									SpriteRenderer spriteRenderer, 
									CharacterController2D controller)
	{
		base.Initialize(thisEnemy, anim, spriteRenderer, controller);

		defaultPlatformMask = controller.platformMask;
		defaultSortingLayer = spriteRenderer.sortingLayerName;
		defaultSortingOrder = spriteRenderer.sortingOrder;
		defaultColor = spriteRenderer.color;
	}
	#endregion

	#region SpawnMethods
	public override void CheckSpawn()
	{
		thisEnemy.CheckAtWall(true);

		if (Mathf.Abs(transform.position.x - entryPoint.x) <= spawnEntryRange)
			Spawn();
	}

	protected override void Spawn()
	{
		thisEnemy.Jump(Mathf.Max(1f, entryPoint.y - transform.position.y + spawnJumpHeight));

		controller.platformMask = defaultPlatformMask;
		spriteRenderer.sortingLayerName = defaultSortingLayer;
		spriteRenderer.sortingOrder = defaultSortingOrder;
		thisEnemy.invincible = false;
		thisEnemy.ignoreProjectiles = false;

		spriteRenderer.DOColor(defaultColor, spawnLength)
			.SetEase(Ease.InOutSine);

		thisEnemy.spawned = true;
	}
	#endregion
}
