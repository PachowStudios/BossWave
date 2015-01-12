using UnityEngine;
using System.Collections;
using DG.Tweening;

public abstract class StandardEnemy : Enemy 
{
	public Transform simulateSpawner;
	public LayerMask spawnPlatformMask;
	public string spawnSortingLayer = "Spawn";
	public int spawnSortingOrder = 0;
	public Color spawnColor = new Color(0.133f, 0.137f, 0.153f, 1f);
	public float spawnEntryRange = 1f;
	public float spawnJumpHeight = 4f;
	public float spawnLength = 0.5f;
	public bool checkFrontCollision = false;
	public bool checkLedgeCollision = false;

	protected Transform frontCheck;
	protected Transform ledgeCheck;

	private LayerMask defaultPlatformMask;
	private string defaultSortingLayer;
	private int defaultSortingOrder;
	private Color defaultColor;
	private Vector3 entryPoint;

	public Transform Spawner
	{
		set
		{
			entryPoint = Extensions.Vector3Range(value.FindChild("Entry Start").position,
												 value.FindChild("Entry End").position);
		}
	}

	protected abstract void ApplyAnimation();

	protected abstract void Walk();

	protected abstract void Jump(float height);

	protected abstract void CheckAttack();

	protected override void Awake()
	{
		base.Awake();

		if (checkFrontCollision)
		{
			frontCheck = transform.FindChild("frontCheck");
		}

		ledgeCheck = transform.FindChild("ledgeCheck");

		defaultPlatformMask = controller.platformMask;
		defaultSortingLayer = spriteRenderer.sortingLayerName;
		defaultSortingOrder = spriteRenderer.sortingOrder;
		defaultColor = spriteRenderer.color;

		if (!spawned)
		{
			controller.platformMask = spawnPlatformMask;
			spriteRenderer.sortingLayerName = spawnSortingLayer;
			spriteRenderer.sortingOrder = spawnSortingOrder;
			spriteRenderer.color = spawnColor;
			invincible = true;
			ignoreProjectiles = true;
			left = true;
		}

		if (simulateSpawner != null)
		{
			Spawner = simulateSpawner;
		}
	}

	protected virtual void FixedUpdate()
	{
		InitialUpdate();
		ApplyAnimation();

		if (checkFrontCollision)
		{
			CheckFrontCollision();
		}

		if (!spawned)
		{
			CheckLedgeCollision();

			if (Mathf.Abs(transform.position.x - entryPoint.x) <= spawnEntryRange)
			{
				Spawn();
			}
		}
		else
		{
			if (checkLedgeCollision)
			{
				CheckLedgeCollision();
			}

			Walk();
			CheckAttack();
		}

		GetMovement();
		ApplyMovement();
	}

	public void EnableMovement(bool enable)
	{
		disableMovement = !enable;
	}

	private void EnableMovementAnim(int enable)
	{
		EnableMovement(enable != 0);
	}

	protected virtual void Spawn()
	{
		Jump(entryPoint.y - transform.position.y + spawnJumpHeight);

		controller.platformMask = defaultPlatformMask;
		spriteRenderer.sortingLayerName = defaultSortingLayer;
		spriteRenderer.sortingOrder = defaultSortingOrder;
		invincible = false;
		ignoreProjectiles = false;
		left = right = false;

		spriteRenderer.DOColor(defaultColor, spawnLength)
			.SetEase(Ease.InOutSine);

		spawned = true;
	}

	protected void CheckFrontCollision()
	{
		Collider2D[] frontHits = Physics2D.OverlapPointAll(frontCheck.position, controller.platformMask);

		if (frontHits.Length > 0)
		{
			Flip();

			right = !right;
			left = !right;
		}
	}

	protected void CheckLedgeCollision()
	{
		if (IsGrounded)
		{
			Collider2D[] ledgeHits = Physics2D.OverlapPointAll(ledgeCheck.position, controller.platformMask);

			if (ledgeHits.Length == 0)
			{
				Flip();

				right = !right;
				left = !right;
			}
		}
	}
}
