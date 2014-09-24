using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour 
{
	public float damage = 5f;
	public float knockback = 3f;
	public float gravity = -35f;
	public float moveSpeed = 5f;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;
	public float maxHealth = 10f;

	[HideInInspector]
	public float health;

	protected bool right = false;
	protected bool left = false;

	[HideInInspector]
	protected float normalizedHorizontalSpeed = 0;

	private SpriteRenderer spriteRenderer;
	private ExplodeEffect explodeEffect;

	protected CharacterController2D controller;
	protected Animator anim;
	protected Vector3 velocity;
	protected Transform frontCheck;
	protected PlayerControl playerControl;

	protected virtual void Awake()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		explodeEffect = GetComponent<ExplodeEffect>();
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		frontCheck = transform.FindChild("frontCheck");
		playerControl = GameObject.Find("Player").GetComponent<PlayerControl>();

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

	void TakeDamage(GameObject enemy)
	{
		float damage = enemy.GetComponent<Projectile>().damage;
		float knockback = enemy.GetComponent<Projectile>().knockback;
		enemy.GetComponent<Projectile>().CheckDestroy();

		health -= damage;

		if (health <= 0f)
		{
			explodeEffect.Explode(velocity, spriteRenderer.sprite);
			playerControl.AddPoints(maxHealth, damage);
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

			controller.move(velocity * Time.deltaTime);
		}
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
			if (hit.tag == "Obstacle")
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
}
