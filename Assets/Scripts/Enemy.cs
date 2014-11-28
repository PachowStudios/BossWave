using UnityEngine;
using System.Collections;

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

	public Difficulty difficulty = Difficulty.Easy;
	public float maxHealth = 10f;
	public float damage = 5f;
	[Range(0f, 100f)]
	public float microchipChance = 25f;
	public int minMicrochips = 1;
	public int maxMicrochips = 3;
	public Microchip.Size smallestMicrochip;
	public Microchip.Size biggestMicrochip;
	public float knockback = 3f;
	public Color flashColor = new Color(1f, 0.47f, 0.47f, 1f);
	public float flashLength = 0.1f;
	public float gravity = -35f;
	public float moveSpeed = 5f;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;
	public bool timeWarpAtDeath = false;

	[HideInInspector]
	public float health;
	[HideInInspector]
	public Vector3 velocity;

	protected bool right = false;
	protected bool left = false;
	protected bool invincible = false;

	[HideInInspector]
	protected float normalizedHorizontalSpeed = 0;

	private SpriteRenderer spriteRenderer;

	protected CharacterController2D controller;
	protected Animator anim;
	protected Transform frontCheck;
	protected Transform popupMessagePoint;

	public Sprite Sprite
	{
		get
		{
			return spriteRenderer.sprite;
		}
	}

	protected virtual void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		frontCheck = transform.FindChild("frontCheck");
		popupMessagePoint = transform.FindChild("popupMessage");

		health = maxHealth;
	}

	void OnTriggerEnter2D(Collider2D enemy)
	{
		if (enemy.tag == "PlayerProjectile")
		{
			if (health > 0f)
			{
				TakeDamage(enemy.gameObject);
			}
		}
	}

	public void TakeDamage(GameObject enemy)
	{
		Projectile enemyProjectile = enemy.GetComponent<Projectile>();
		float damage = enemyProjectile.damage;
		float knockback = enemyProjectile.knockback;
		enemyProjectile.CheckDestroyEnemy();

		if (!invincible)
		{
			health -= damage;

			if (health <= 0f)
			{
				ExplodeEffect.Explode(transform, velocity, spriteRenderer.sprite);
				int pointsAdded = PlayerControl.instance.AddPointsFromEnemy(maxHealth, damage);
				PopupMessage.CreatePopup(popupMessagePoint.position, pointsAdded.ToString());

				if (Random.Range(0f, 100f) <= microchipChance)
				{
					int microchipsToSpawn = Random.Range(minMicrochips, maxMicrochips + 1);

					for (int i = 0; i < microchipsToSpawn; i++)
					{
						Microchip.Size microchipSize = (Microchip.Size)Random.Range((int)smallestMicrochip, (int)biggestMicrochip + 1);
						LevelManager.SpawnMicrochip(transform.position, microchipSize);
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
				velocity.x = Mathf.Sqrt(Mathf.Pow(knockback, 2) * -gravity);
				velocity.y = Mathf.Sqrt(knockback * -gravity);

				if (transform.position.x - enemy.transform.position.x < 0)
				{
					velocity.x *= -1;
				}

				if (velocity.x > 0 || velocity.y > 0)
				{
					controller.move(velocity * Time.deltaTime);
				}

				spriteRenderer.color = flashColor;
				Invoke("ResetColor", flashLength);
			}
		}
	}

	public void Kill()
	{
		ExplodeEffect.Explode(transform, velocity, spriteRenderer.sprite);
		PlayerControl.instance.AddPointsFromEnemy(maxHealth, damage);

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
		}
	}

	protected void GetMovement()
	{
		if (right)
		{
			normalizedHorizontalSpeed = 1;

			if (transform.localScale.x < 0f)
			{
				Flip();
			}
		}
		else if (left)
		{
			normalizedHorizontalSpeed = -1;

			if (transform.localScale.x > 0f)
			{
				Flip();
			}
		}
		else
		{
			normalizedHorizontalSpeed = 0;
		}
	}

	protected void ApplyMovement()
	{
		float smoothedMovementFactor = controller.isGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * moveSpeed, Time.fixedDeltaTime * smoothedMovementFactor);
		velocity.y += gravity * Time.fixedDeltaTime;

		controller.move(velocity * Time.fixedDeltaTime);
	}

	protected void CheckFrontCollision()
	{
		Collider2D[] frontHits = Physics2D.OverlapPointAll(frontCheck.position);

		foreach (Collider2D hit in frontHits)
		{
			if (hit.tag == "Obstacle" || hit.tag == "WorldBoundaries")
			{
				Flip();

				right = !right;
				left = !right;
				break;
			}
		}
	}

	protected void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	private void DeathTimeWarp()
	{
		spriteRenderer.enabled = false;
		collider2D.enabled = false;
		TimeWarpEffect.Warp(0.15f, 0f, 0.5f);
	}

	private void ResetColor()
	{
		spriteRenderer.color = Color.white;
	}
}