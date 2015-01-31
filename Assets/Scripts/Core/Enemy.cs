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

	protected float health;
	protected bool right = false;
	protected bool left = false;
	protected bool disableMovement = false;
	protected float normalizedHorizontalSpeed = 0;

	protected Vector3 lastGroundedPosition;

	protected SpriteRenderer spriteRenderer;
	protected CharacterController2D controller;
	protected Animator anim;
	protected Transform popupMessagePoint;

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

	protected bool IsPlayerOnRightSide
	{
		get { return PlayerControl.Instance.transform.position.x > transform.position.x; }
	}

	protected virtual void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		popupMessagePoint = transform.FindChild("popupMessage");

		health = maxHealth;
	}

	protected virtual void OnTriggerEnter2D(Collider2D enemy)
	{
		if (!ignoreProjectiles && enemy.tag == "PlayerProjectile")
		{
			if (health > 0f)
			{
				TakeDamage(enemy.gameObject);
			}
		}
	}

	public virtual void TakeDamage(GameObject enemy)
	{
		Projectile enemyProjectile = enemy.GetComponent<Projectile>();
		float damage = enemyProjectile.damage;
		Vector2 knockback = enemyProjectile.knockback;
		Vector2 knockbackDirection = enemyProjectile.direction.Sign();
		enemyProjectile.CheckDestroyEnemy();

		if (!invincible)
		{
			health -= damage;

			if (health <= 0f)
			{
				ExplodeEffect.Instance.Explode(transform, velocity, spriteRenderer.sprite);
				int pointsAdded = PlayerControl.Instance.AddPointsFromEnemy(maxHealth, damage);
				PopupMessage.Instance.CreatePopup(popupMessagePoint.position, pointsAdded.ToString());

				if (Random.Range(0f, 100f) <= microchipChance)
				{
					int microchipsToSpawn = Random.Range(minMicrochips, maxMicrochips + 1);

					for (int i = 0; i < microchipsToSpawn; i++)
					{
						Microchip.Size microchipSize = (Microchip.Size)Random.Range((int)smallestMicrochip, (int)biggestMicrochip + 1);
						LevelManager.Instance.SpawnMicrochip(transform.position, microchipSize);
					}
				}

				if (timeWarpAtDeath)
				{
					DeathTimeWarp();
				}

				Destroy(gameObject);
			}
			else
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
				Invoke("ResetColor", flashLength);
			}
		}
	}

	public void Kill()
	{
		ExplodeEffect.Instance.Explode(transform, velocity, spriteRenderer.sprite);
		PlayerControl.Instance.AddPointsFromEnemy(maxHealth, damage);

		if (timeWarpAtDeath)
		{
			DeathTimeWarp();
		}

		Destroy(gameObject);
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

	private void DeathTimeWarp()
	{
		spriteRenderer.enabled = false;
		collider2D.enabled = false;
		TimeWarpEffect.Instance.Warp(0.15f, 0f, 0.5f);
	}

	private void ResetColor()
	{
		spriteRenderer.color = Color.white;
	}
}