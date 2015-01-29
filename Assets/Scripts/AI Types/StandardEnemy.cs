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

	protected int RelativePlayerHeight
	{
		get { return (int)(lastGroundedPosition.y - PlayerControl.instance.LastGroundedPosition.y); }
	}

	protected abstract void ApplyAnimation();

	protected abstract void Walk();

	protected abstract void CheckAttack();

	protected override void Awake()
	{
		base.Awake();

		frontCheck = transform.FindChild("frontCheck");
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
			transform.position = simulateSpawner.FindChild("Spawn").position;
		}
	}

	protected virtual void FixedUpdate()
	{
		InitialUpdate();
		ApplyAnimation();

		if (!spawned)
		{
			CheckLedgeCollision(true);

			if (Mathf.Abs(transform.position.x - entryPoint.x) <= spawnEntryRange)
			{
				Spawn();
			}
		}
		else
		{
			if (!disableMovement)
			{
				Walk();
			}

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
		Jump(Mathf.Max(1f, entryPoint.y - transform.position.y + spawnJumpHeight));

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

	protected virtual void Jump(float height)
	{
		if (height >= 0)
		{
			velocity.y = Mathf.Sqrt(2f * height * -gravity);
		}
	}

	protected bool CheckFrontCollision(bool flip = false)
	{
		Collider2D frontHit = Physics2D.OverlapPoint(frontCheck.position, controller.platformMask);

		if (frontHit != null && flip)
		{
			Flip();

			right = !right;
			left = !right;
		}

		return frontHit != null;
	}

	protected bool CheckLedgeCollision(bool flip = false)
	{
		if (IsGrounded)
		{
			Collider2D ledgeHit = Physics2D.OverlapPoint(ledgeCheck.position, controller.platformMask);

			if (ledgeHit == null && flip)
			{
				Flip();

				right = !right;
				left = !right;
			}

			return ledgeHit != null;
		}
		else
		{
			return false;
		}
	}

	protected bool IsPlayerInRange(float min, float max)
	{
		int direction = FacingRight ? 1 : -1;
		Vector3 startPoint = new Vector3(transform.position.x + (min * direction), collider2D.bounds.center.y, 0f);
		Vector3 endPoint = startPoint + new Vector3((max - min) * direction, 0f, 0f);
		RaycastHit2D linecast = Physics2D.Linecast(startPoint, endPoint, LayerMask.GetMask("Player"));

		return linecast.collider != null;
	}
}
