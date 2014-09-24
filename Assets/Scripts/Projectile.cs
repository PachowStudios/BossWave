using UnityEngine;
using System.Collections;

public abstract class Projectile : MonoBehaviour 
{
	public bool playerShot = false;
	public float damage = 5f;
	public float knockback = 2f;
	public float gravity = 0f;
	public float shotSpeed = 15f;
	public float lifetime = 3f;
	public bool autoDestroy = true;
	public bool destroyOnCollision = true;

	[HideInInspector]
	public Vector3 direction;

	private CharacterController2D controller;
	private SpriteRenderer spriteRenderer;
	private ExplodeEffect explodeEffect;
	private Vector3 velocity;

	protected virtual void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		explodeEffect = GetComponent<ExplodeEffect>();

		if (playerShot)
		{
			tag = "PlayerProjectile";
		}

		if (autoDestroy)
		{
			Destroy(gameObject, lifetime);
		}
	}

	protected void InitialUpdate()
	{
		velocity = controller.velocity;
	}

	protected void ApplyMovement()
	{
		velocity.x = direction.x * shotSpeed;
		direction.y += (gravity * Time.fixedDeltaTime) / 10f;
		velocity.y = direction.y * shotSpeed;

		controller.move(velocity * Time.fixedDeltaTime);
	}

	protected void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	public void CheckDestroy()
	{
		if (destroyOnCollision)
		{
			explodeEffect.Explode(velocity, spriteRenderer.sprite);
			Destroy(gameObject);
		}
	}
}
