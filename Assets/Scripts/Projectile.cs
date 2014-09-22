using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
	public bool playerShot = false;
	public float damage = 5f;
	public float knockback = 2f;
	public float gravity = 0f;
	public float shotSpeed = 15f;
	public float lifetime = 3f;
	public bool autoDestroy = true;

	[HideInInspector]
	public bool right = false;
	[HideInInspector]
	public bool left = false;

	[HideInInspector]
	protected float normalizedHorizontalSpeed = 0;

	protected CharacterController2D controller;
	protected Animator anim;
	protected Vector3 velocity;
	

	protected virtual void Awake()
	{
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();

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

	protected void GetMovement()
	{
		if (right)
		{
			normalizedHorizontalSpeed = 1;

			if (transform.localScale.x < 0)
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
		velocity.x = normalizedHorizontalSpeed * shotSpeed;
		velocity.y += gravity * Time.fixedDeltaTime;

		controller.move(velocity * Time.fixedDeltaTime);
	}

	protected void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}
}
