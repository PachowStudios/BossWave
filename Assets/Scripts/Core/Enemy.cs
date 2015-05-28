using UnityEngine;
using System.Collections;
using DG.Tweening;

public abstract class Enemy : MonoBehaviour
{
	#region Types
	public enum Difficulty
	{
		Easy,
		Normal,
		Difficult,
		Brutal,
		Insane,
		Boss
	};
	#endregion

	#region Events
	public delegate void OnDeathHandler();
	public event OnDeathHandler OnDeath;
	#endregion

	#region Fields
	public bool spawned = true;
	public Difficulty difficulty = Difficulty.Easy;
	public bool immuneToInstantKill = false;
	public bool immuneToKnockback = false;
	public float maxHealth = 10f;
	public float damage = 5f;
	public bool showDrops = true;
	[Range(0f, 100f)]
	public float microchipChance = 25f;
	public int minMicrochips = 1;
	public int maxMicrochips = 3;
	public Microchip.Size smallestMicrochip;
	public Microchip.Size biggestMicrochip;
	public Vector2 knockback = new Vector2(3f, 3f);
	public bool flashOnHit = true;
	public Color flashColor = new Color(1f, 0.47f, 0.47f, 1f);
	public float flashLength = 0.1f;
	public float gravity = -35f;
	public float moveSpeed = 5f;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;

	[HideInInspector]
	public bool invincible = false;
	[HideInInspector]
	public bool ignoreProjectiles = false;

	protected Vector3 velocity;
	protected Vector3 lastGroundedPosition;
	protected float horizontalMovement = 0;
	protected bool disableMovement = false;

	protected SpriteRenderer spriteRenderer;
	protected CharacterController2D controller;
	protected Animator anim;
	protected Transform popupMessagePoint;

	private float health;
	private bool dead = false;
	#endregion

	#region Public Properties
	public float Health
	{
		get { return health; }

		set
		{
			health = Mathf.Clamp(value, 0f, maxHealth);
			CheckDeath();
		}
	}

	public bool Dead
	{ get { return dead; } }

	public float HealthPercentage
	{ get { return Mathf.Clamp01(health / maxHealth); } }

	public Sprite Sprite
	{ get { return spriteRenderer.sprite; } }

	public bool Right
	{ get { return horizontalMovement > 0f; } }

	public bool Left
	{ get { return horizontalMovement < 0f; } }

	public float HorizontalMovement
	{
		get { return horizontalMovement; }
		set { horizontalMovement = value == 0f ? 0f : (value > 0f ? 1f : -1f); }
	}

	public Vector3 Velocity
	{ get { return velocity; } }

	public bool MovementDisabled
	{ get { return disableMovement; } }

	public bool FacingRight
	{ get { return transform.localScale.x < 0; } }

	public bool IsGrounded
	{ get { return controller.IsGrounded; } }

	public bool WasGroundedLastFrame
	{ get { return controller.WasGroundedLastFrame; }}

	public LayerMask CollisionLayers
	{ get { return controller.platformMask; } }

	public bool PlayerIsOnRight
	{ get { return PlayerControl.Instance.transform.position.x > transform.position.x; } }

	public float RelativePlayerLastGrounded
	{ get { return (lastGroundedPosition.y - PlayerControl.Instance.LastGroundedPosition.y).RoundToTenth(); } }

	public float RelativePlayerHeight
	{ get { return transform.position.y - PlayerControl.Instance.transform.position.y; } }
	#endregion

	#region MonoBehaviour
	protected virtual void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		popupMessagePoint = transform.FindChild("popupMessage");

		health = maxHealth;
	}

	protected virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (!ignoreProjectiles && other.tag == "PlayerProjectile")
		{
			if (Health > 0f)
			{
				TakeDamage(other.gameObject);
			}
		}
	}
	#endregion

	#region Internal Update Methods
	protected void GetMovement()
	{
		if (Right && !FacingRight)
			transform.Flip();
		else if (Left && FacingRight)
			transform.Flip();
	}

	protected void ApplyMovement()
	{
		float smoothedMovementFactor = controller.IsGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x, 
								disableMovement ? 0f : (horizontalMovement * moveSpeed), 
								smoothedMovementFactor * Time.deltaTime);
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
		velocity = controller.Velocity;

		if (controller.IsGrounded)
		{
			velocity.y = 0;
			lastGroundedPosition = transform.position;
		}
	}
	#endregion

	#region Internal Helper Methods
	protected virtual void CheckDeath()
	{
		if (Health <= 0f && !dead)
		{
			dead = true;

			if (OnDeath != null)
				OnDeath();

			HandleDeath();
		}
	}

	protected abstract void HandleDeath();

	protected void FacePlayer()
	{
		if (PlayerIsOnRight && !FacingRight)
			transform.Flip();
		else if (!PlayerIsOnRight && FacingRight)
			transform.Flip();
	}
	#endregion

	#region Public Methods
	public virtual void TakeDamage(GameObject enemy, float multiplier = 1f)
	{
		Projectile enemyProjectile = enemy.GetComponent<Projectile>();
		float damage = enemyProjectile.damage * multiplier;
		Vector2 knockback = enemyProjectile.knockback;
		Vector2 knockbackDirection = enemyProjectile.direction.Sign();
		enemyProjectile.CheckDestroyEnemy();

		if (!invincible && damage != 0f)
		{
			Health -= damage;

			if (Health > 0f)
			{
				if (!immuneToKnockback)
				{
					knockback.x += Mathf.Sqrt(Mathf.Abs(Mathf.Pow(knockback.x, 2) * -gravity));
					knockback.y += Mathf.Sqrt(Mathf.Abs(knockback.y * -gravity));
					knockback.Scale(knockbackDirection);

					if (knockback.x != 0 || knockback.y != 0)
					{
						velocity += (Vector3)knockback;
						controller.Move(velocity * Time.deltaTime);
					}
				}

				if (flashOnHit)
					spriteRenderer.FlashColor(flashColor, flashLength);
			}
		}
	}

	public void Kill()
	{
		if (!immuneToInstantKill)
		{
			showDrops = false;
			Health = 0f;
		}
	}

	public void KillNoPoints()
	{
		ExplodeEffect.Instance.Explode(transform, velocity, spriteRenderer.sprite);
		Destroy(gameObject);
	}

	public void Move(Vector3 velocity)
	{
		controller.Move(velocity * Time.deltaTime);
		this.velocity = controller.Velocity;
	}
	#endregion
}