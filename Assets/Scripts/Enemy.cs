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

	public bool spawned = false;
	public LayerMask spawnPlatformMask;
	public LayerMask spawnTriggerMask;
	public string spawnSortingLayer = "Spawn";
	public int spawnSortingOrder = 0;
	public Color spawnColor = new Color(0.102f, 0.11f, 0.22f, 1f);
	public float spawnTime = 1f;
	public float spawnLength = 0.5f;
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
	protected bool disableMovement = false;
	protected bool invincible = false;
	protected float normalizedHorizontalSpeed = 0;

	protected SpriteRenderer spriteRenderer;
	protected CharacterController2D controller;
	protected Animator anim;
	protected Transform frontCheck;
	protected Transform popupMessagePoint;

	private LayerMask defaultPlatformMask;
	private LayerMask defaultTriggerMask;
	private string defaultSortingLayer;
	private int defaultSortingOrder;
	private Color defaultColor;
	private float spawnTimer = 0f;

	public Sprite Sprite
	{
		get
		{
			return spriteRenderer.sprite;
		}
	}

	protected abstract void ApplyAnimation();

	protected abstract void Walk();

	protected abstract void CheckAttack();

	protected virtual void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		frontCheck = transform.FindChild("frontCheck");
		popupMessagePoint = transform.FindChild("popupMessage");

		defaultPlatformMask = controller.platformMask;
		defaultTriggerMask = controller.triggerMask;
		defaultSortingLayer = spriteRenderer.sortingLayerName;
		defaultSortingOrder = spriteRenderer.sortingOrder;
		defaultColor = spriteRenderer.color;

		if (!spawned)
		{
			controller.platformMask = spawnPlatformMask;
			controller.triggerMask = spawnTriggerMask;
			spriteRenderer.sortingLayerName = spawnSortingLayer;
			spriteRenderer.sortingOrder = spawnSortingOrder;
			spriteRenderer.color = spawnColor;
		}

		health = maxHealth;
	}

	protected virtual void FixedUpdate()
	{
		InitialUpdate();
		ApplyAnimation();

		if (!spawned)
		{
			spawnTimer += Time.deltaTime;

			if (spawnTimer >= spawnTime)
			{
				Spawn();
			}

			left = true;
		}
		else
		{
			Walk();
			CheckAttack();
		}

		GetMovement();
		ApplyMovement();
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

	public virtual void TakeDamage(GameObject enemy)
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

	public void EnableMovement(bool enable)
	{
		disableMovement = !enable;
	}

	private void EnableMovementAnim(int enable)
	{
		EnableMovement(enable != 0);
	}

	protected void InitialUpdate()
	{
		velocity = controller.velocity;

		if (controller.isGrounded)
		{
			velocity.y = 0;
		}
	}

	protected void Spawn()
	{
		controller.platformMask = defaultPlatformMask;
		controller.triggerMask = defaultTriggerMask;
		spriteRenderer.sortingLayerName = defaultSortingLayer;
		spriteRenderer.sortingOrder = defaultSortingOrder;

		iTween.ValueTo(gameObject, iTween.Hash("from", spawnColor,
											   "to", defaultColor,
											   "time", spawnLength,
											   "easetype", iTween.EaseType.easeInOutSine,
											   "onupdate", "UpdateColor"));

		spawned = true;
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

	private void UpdateColor(Color newValue)
	{
		spriteRenderer.color = newValue;
	}

	private void ResetColor()
	{
		spriteRenderer.color = Color.white;
	}
}