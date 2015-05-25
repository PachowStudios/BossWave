using UnityEngine;
using System.Collections;
using DG.Tweening;

public abstract class StandardEnemy : Enemy
{
	#region Fields
	public float maxJumpHeight = 7f;

	public Transform frontCheck;
	public Transform ledgeCheck;
	#endregion

	#region Public Properties
	public SpawnAI SpawnAI
	{ get; private set; }

	public AttackAI AttackAI
	{ get; private set; }
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		SpawnAI = GetComponent<SpawnAI>();
		SpawnAI.Initialize(this, anim, spriteRenderer, controller);

		AttackAI = GetComponent<AttackAI>();
		AttackAI.Initialize(this, anim);
	}

	protected virtual void Start()
	{ }

	protected virtual void Update()
	{
		ApplyAnimation();

		if (spawned)
		{
			if (!disableMovement)
				Walk();

			AttackAI.CheckAttack();
		}
		else
			SpawnAI.CheckSpawn();
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
		EnableMovement(enable == 1);
	}

	protected override void HandleDeath()
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

		Destroy(gameObject);
	}

	protected virtual void FollowPlayer(float range)
	{
		if (transform.position.x + range < PlayerControl.Instance.transform.position.x)
			horizontalMovement = 1f;
		else if (transform.position.x - range > PlayerControl.Instance.transform.position.x)
			horizontalMovement = -1f;
		else
		{
			horizontalMovement = 0f;
			FacePlayer();
		}
	}
	#endregion

	#region Public Methods
	public virtual void Jump(float height)
	{
		if (height > 0f)
			velocity.y = Mathf.Sqrt(2f * Mathf.Min(height, maxJumpHeight) * -gravity);
	}

	public bool CheckAtWall(bool flip = false)
	{
		Collider2D collision = Physics2D.OverlapPoint(frontCheck.position, CollisionLayers);
		bool atWall = collision != null;

		if (atWall && flip)
		{
			horizontalMovement *= -1f;

			if (horizontalMovement == 0f)
				horizontalMovement = 1f;
		}

		return atWall;
	}

	public bool CheckAtLedge(bool flip = false)
	{
		if (!IsGrounded)
			return false;

		Collider2D collision = Physics2D.OverlapPoint(ledgeCheck.position, controller.platformMask);
		bool atLedge = collision == null;

		if (atLedge && flip)
		{
			horizontalMovement *= -1f;

			if (horizontalMovement == 0f)
				horizontalMovement = 1f;
		}

		return atLedge;
	}

	public void EnableMovement(bool enable)
	{
		disableMovement = !enable;
	}
	#endregion
}
