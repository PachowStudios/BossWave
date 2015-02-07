using UnityEngine;
using System.Collections;
using DG.Tweening;

public abstract class Enemy : MonoBehaviour
{
	public enum Difficulty
	{
		Easy,
		Normal,
		Difficult,
		Brutal,
		Insane,
		Boss
	};

	public bool spawned = true;
	public Difficulty difficulty = Difficulty.Easy;
	public bool immuneToInstantKill = false;
	public float maxHealth = 10f;
	public float damage = 5f;
	[Range(0f, 100f)]
	public float microchipChance = 25f;
	public int minMicrochips = 1;
	public int maxMicrochips = 3;
	public Microchip.Size smallestMicrochip;
	public Microchip.Size biggestMicrochip;
	public Vector2 knockback = new Vector2(3f, 3f);
	public Color flashColor = new Color(1f, 0.47f, 0.47f, 1f);
	public float flashLength = 0.1f;
	public float gravity = -35f;
	public float moveSpeed = 5f;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;
	public bool timeWarpAtDeath = false;

	[HideInInspector]
	public Vector3 velocity;
	[HideInInspector]
	public bool invincible = false;
	[HideInInspector]
	public bool ignoreProjectiles = false;

	protected bool right = false;
	protected bool left = false;
	protected bool disableMovement = false;
	protected float normalizedHorizontalSpeed = 0;

	protected Vector3 lastGroundedPosition;

	protected SpriteRenderer spriteRenderer;
	protected CharacterController2D controller;
	protected Animator anim;
	protected Transform popupMessagePoint;

	private float health;

	public float Health
	{
		get { return health; }

		set
		{
			health = Mathf.Clamp(value, 0f, maxHealth);
			CheckDeath();
		}
	}

	public Sprite Sprite
	{
		get { return spriteRenderer.sprite; }
	}

	public bool FacingRight
	{
		get { return transform.localScale.x < 0; }
	}

	public bool IsGrounded
	{
		get { return controller.isGrounded; }
	}

	public bool WasGroundedLastFrame
	{
		get { return controller.wasGroundedLastFrame; }
	}

	protected bool IsPlayerOnRightSide
	{
		get { return PlayerControl.Instance.transform.position.x > transform.position.x; }
	}

	protected abstract void CheckDeath(bool showDrops = true);

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

		if (other.tag == "Killzone")
		{
			Kill();
		}
	}

	public virtual void TakeDamage(GameObject enemy)
	{
		Projectile enemyProjectile = enemy.GetComponent<Projectile>();
		float damage = enemyProjectile.damage;
		Vector2 knockback = enemyProjectile.knockback;
		Vector2 knockbackDirection = enemyProjectile.direction.Sign();
		enemyProjectile.CheckDestroyEnemy();

		if (!invincible && damage != 0f)
		{
			Health -= damage;

			if (Health > 0f)
			{
				knockback.x += Mathf.Sqrt(Mathf.Abs(Mathf.Pow(knockback.x, 2) * -gravity));
				knockback.y += Mathf.Sqrt(Mathf.Abs(knockback.y * -gravity));
				knockback.Scale(knockbackDirection);

				if (knockback.x != 0 || knockback.y != 0)
				{
					velocity += (Vector3)knockback;
					controller.move(velocity * Time.deltaTime);
				}

				spriteRenderer.color = flashColor;

				DOTween.Sequence()
					.AppendInterval(flashLength)
					.AppendCallback(() =>
					{
						if (this != null)
						{
							ResetColor();
						}
					});
			}
		}
	}

	public void Kill()
	{
		if (!immuneToInstantKill)
		{
			health = 0f;
			CheckDeath(false);
		}
	}

	public void Move(Vector3 velocity)
	{
		controller.move(velocity * Time.deltaTime);
	}

	protected void InitialUpdate()
	{
		velocity = controller.velocity;

		if (controller.isGrounded)
		{
			velocity.y = 0;
			lastGroundedPosition = transform.position;
		}
	}

	protected void GetMovement()
	{
		if (right)
		{
			normalizedHorizontalSpeed = 1;

			if (transform.localScale.x > 0f)
			{
				Flip();
			}
		}
		else if (left)
		{
			normalizedHorizontalSpeed = -1;

			if (transform.localScale.x < 0f)
			{
				Flip();
			}
		}
		else
		{
			normalizedHorizontalSpeed = 0;
		}

		normalizedHorizontalSpeed = disableMovement ? 0 : normalizedHorizontalSpeed;
	}

	protected void ApplyMovement()
	{
		float smoothedMovementFactor = controller.isGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * moveSpeed, Time.fixedDeltaTime * smoothedMovementFactor);
		velocity.y += gravity * Time.fixedDeltaTime;

		controller.move(velocity * Time.fixedDeltaTime);
	}

	protected void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	protected void FacePlayer()
	{
		if ((PlayerControl.Instance.transform.position.x < transform.position.x && FacingRight) ||
			(PlayerControl.Instance.transform.position.x > transform.position.x && !FacingRight))
		{
			Flip();
		}
	}

	protected void DeathTimeWarp()
	{
		TimeWarpEffect.Instance.Warp(0.15f, 0f, 0.5f);
	}

	protected void ResetColor()
	{
		spriteRenderer.color = Color.white;
	}
}