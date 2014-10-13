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
	[HideInInspector]
	public Vector3 velocity;

	private CharacterController2D controller;
	private ExplodeEffect explodeEffect;
	private SpriteRenderer spriteRenderer;

	protected virtual void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		explodeEffect = GetComponent<ExplodeEffect>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (playerShot)
		{
			tag = "PlayerProjectile";
		}

		if (autoDestroy)
		{
			Destroy(gameObject, lifetime);
		}
	}

	void OnTriggerEnter2D(Collider2D trigger)
	{
		if (trigger.gameObject.layer == LayerMask.NameToLayer("Collider"))
		{
			CheckDestroy();
		}
	}

	void OnTriggerStay2D(Collider2D trigger)
	{
		OnTriggerEnter2D(trigger);
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
