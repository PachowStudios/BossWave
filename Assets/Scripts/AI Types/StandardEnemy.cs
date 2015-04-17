using UnityEngine;
using System.Collections;
using DG.Tweening;

public abstract class StandardEnemy : Enemy
{
	#region Fields
	public float maxJumpHeight = 7f;

	private Transform frontCheck;
	private Transform ledgeCheck;
	#endregion

	#region Public Properties
	public SpawnAI SpawnAI
	{ get; private set; }

	public AttackAI AttackAI
	{ get; private set; }

	public bool MovementDisabled
	{ get { return disableMovement; } }
	#endregion

	#region Internal Properties
	protected float RelativePlayerLastGrounded
	{
		get { return (lastGroundedPosition.y - PlayerControl.Instance.LastGroundedPosition.y).RoundToTenth(); }
	}

	protected float RelativePlayerHeight
	{
		get { return transform.position.y - PlayerControl.Instance.transform.position.y; }
	}
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		SpawnAI = GetComponent<SpawnAI>();
		SpawnAI.Initialize(this, anim, spriteRenderer, controller);

		AttackAI = GetComponent<AttackAI>();
		AttackAI.Initialize(this, anim);

		frontCheck = transform.FindChild("frontCheck");
		ledgeCheck = transform.FindChild("ledgeCheck");
	}

	protected virtual void Start()
	{ }

	protected virtual void Update()
	{
		ApplyAnimation();

		if (spawned)
		{
			if (!disableMovement)
			{
				Walk();
			}

			AttackAI.CheckAttack();
		}
		else
		{
			SpawnAI.CheckSpawn();
		}
	}

	protected virtual void LateUpdate()
	{
		GetMovement();
		ApplyMovement();
	}
	#endregion

	#region Internal Update Methods
	protected abstract void ApplyAnimation();
	protected abstract void Walk();
	#endregion

	#region Internal Helper Methods
	private void EnableMovementAnim(int enable)
	{
		EnableMovement(enable != 0);
	}

	protected override void CheckDeath(bool showDrops = true)
	{
		if (Health <= 0f)
		{
			ExplodeEffect.Instance.Explode(transform, velocity, spriteRenderer.sprite);
			int pointsAdded = PlayerControl.Instance.AddPointsFromEnemy(maxHealth, damage);

			if (showDrops)
			{
				PopupMessage.Instance.CreatePopup(popupMessagePoint.position, pointsAdded.ToString());

				if (Random.Range(0f, 100f) <= microchipChance)
				{
					int microchipsToSpawn = Random.Range(minMicrochips, maxMicrochips + 1);

					for (int i = 0; i < microchipsToSpawn; i++)
					{
						Microchip.Size microchipSize = (Microchip.Size)Random.Range((int)smallestMicrochip, (int)biggestMicrochip + 1);
						PowerupSpawner.Instance.SpawnMicrochip(transform.position, microchipSize);
					}
				}
			}

			if (timeWarpAtDeath)
			{
				DeathTimeWarp();
			}

			Destroy(gameObject);
		}
	}

	protected virtual void FollowPlayer(float range)
	{
		if (transform.position.x + range < PlayerControl.Instance.transform.position.x)
		{
			right = true;
			left = !right;
		}
		else if (transform.position.x - range > PlayerControl.Instance.transform.position.x)
		{
			left = true;
			right = !left;
		}
		else
		{
			right = left = false;
			FacePlayer();
		}
	}
	#endregion

	#region Public Methods
	public virtual void Jump(float height)
	{
		if (height > 0f)
		{
			velocity.y = Mathf.Sqrt(2f * Mathf.Min(height, maxJumpHeight) * -gravity);
		}
	}

	public bool CheckFrontCollision(bool flip = false)
	{
		Collider2D frontHit = Physics2D.OverlapPoint(frontCheck.position, controller.platformMask);

		if (frontHit != null && flip)
		{
			right = !right;
			left = !right;
		}

		return frontHit != null;
	}

	public bool CheckLedgeCollision(bool flip = false)
	{
		if (IsGrounded)
		{
			Collider2D ledgeHit = Physics2D.OverlapPoint(ledgeCheck.position, controller.platformMask);

			if (ledgeHit == null && flip)
			{
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

	public void EnableMovement(bool enable)
	{
		disableMovement = !enable;
	}
	#endregion
}
