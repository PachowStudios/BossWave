using UnityEngine;
using System.Collections;

public abstract class Projectile : MonoBehaviour
{
	#region Fields
	public float damage = 5f;
	public Vector2 knockback = new Vector2(2f, 2f);
	public float gravity = 0f;
	public float shotSpeed = 15f;
	public float lifetime = 3f;
	public bool autoDestroy = true;
	public bool destroyOnEnemy = true;
	public bool destroyOnWorld = true;
	public bool correctRotation = true;
	public bool destroyShake = false;
	public float shakeDuration = 0.5f;
	public Vector3 shakeIntensity = new Vector3(0f, 0.5f, 0f);
	public string destroyEffect;

	[HideInInspector]
	public Vector3 direction;

	protected Vector3 velocity;
	protected CharacterController2D controller;
	protected SpriteRenderer spriteRenderer;
	protected Animator anim;
	#endregion

	#region Public Properties
	public Sprite Sprite
	{
		get
		{
			if (spriteRenderer)
				return spriteRenderer.sprite;
			else
				return null;
		}
	}

	public Color SpriteColor
	{
		get { return spriteRenderer == null ? Color.clear : spriteRenderer.color; }

		set
		{
			if (spriteRenderer != null)
				spriteRenderer.color = value;
		}
	}
	#endregion

	#region MonoBehaviour
	protected virtual void Awake()
	{
		controller = GetComponent<CharacterController2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();

		if (autoDestroy)
			StartCoroutine(FailsafeDestroy());
	}

	protected virtual void OnEnable()
	{
		SpriteColor = Color.clear;
	}

	protected virtual void OnTriggerEnter2D(Collider2D trigger)
	{
		if ((trigger.gameObject.layer == LayerMask.NameToLayer("Collider") || trigger.gameObject.layer == LayerMask.NameToLayer("RunningCollider")) &&
			trigger.tag != "RunningBoundaries")
			CheckDestroyWorld();
	}

	protected virtual void OnTriggerStay2D(Collider2D trigger)
	{
		OnTriggerEnter2D(trigger);
	}
	#endregion

	#region Internal Update Methods
	protected void DoMovement()
	{
		velocity.x = direction.x * shotSpeed;
		direction.y += (gravity * Time.deltaTime) / 10f;
		velocity.y = direction.y * shotSpeed;

		if (correctRotation)
			transform.CorrectScaleForRotation(direction.DirectionToRotation2D());

		transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
		controller.move(velocity * Time.deltaTime);
		velocity = controller.velocity;
	}
	#endregion

	#region Internal Helper Methods
	protected void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	protected IEnumerator FailsafeDestroy()
	{
		yield return new WaitForSeconds(lifetime);

		DoDestroy();
	}
	#endregion

	#region Public Methods
	public virtual void Initialize(Vector3 newDirection)
	{
		if (direction == Vector3.zero)
		{
			direction = newDirection;

			if (correctRotation)
				transform.CorrectScaleForRotation(direction.DirectionToRotation2D());

			SpriteColor = Color.white;
		}
	}

	public void Move(Vector3 velocity)
	{
		controller.move(velocity * Time.deltaTime);
		this.velocity = controller.velocity;
	}

	public void CheckDestroyEnemy()
	{
		if (destroyOnEnemy)
			DoDestroy();
	}

	public void CheckDestroyWorld()
	{
		if (destroyOnWorld)
			DoDestroy();
	}

	public virtual void DoDestroy()
	{
		if (destroyShake)
			CameraShake.Instance.Shake(shakeDuration, shakeIntensity);

		if (destroyEffect != "")
			SpriteEffect.Instance.SpawnEffect(destroyEffect, transform.position, LevelManager.Instance.foregroundLayer);

		ExplodeEffect.Instance.Explode(transform, velocity, Sprite);
		Destroy(gameObject);
	}
	#endregion
}
