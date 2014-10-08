using UnityEngine;
using System.Collections;


public class PlayerControl : MonoBehaviour
{
	public float maxHealth = 100f;
	public float invincibilityPeriod = 2f;
	public float gravity = -35f;
	public float turningSpeed = 1f;
	public float walkSpeed = 10f;
	public float runSpeed = 17.5f;
	public float runFullSpeed = 20f;
	public float runFullTime = 1.5f;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;
	public float jumpHeight = 5f;
	public float comboStartKills = 3f;
	public float comboDecreaseTime = 1f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;
	[HideInInspector]
	public float speedMultiplier = 1f;

	private CharacterController2D controller;
	private Animator anim;
	private BoxCollider2D boxCollider;
	private RaycastHit2D lastControllerColliderHit;
	private Vector3 velocity;
	private SpriteRenderer spriteRenderer;
	private ExplodeEffect explodeEffect;
	[HideInInspector]
	public Gun gun;

	[HideInInspector]
	public float health;
	[HideInInspector]
	public float score = 0f;
	[HideInInspector]
	public float combo = 1f;

	private bool right;
	private bool left;
	private bool jump;
	private bool run;
	private bool crouch;

	private bool runFull = false;
	private float runFullTimer = 0f;

	private float originalColliderHeight;
	private float originalColliderOffset;
	private float crouchingColliderHeight;
	private float crouchingColliderOffset;

	private float lastHitTime;
	private bool canTakeDamage = true;
	private float flashTimer = 0f;
	private float flashTime = 0.25f;
	private float smoothFlashTime;

	private float currentMaxCombo = 1f;
	private float comboTimer = 0f;
	private float killChain = 0f;
	private float nextCombo = 0f;

	void Awake()
	{
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		explodeEffect = GetComponent<ExplodeEffect>();
		gun = GetComponentInChildren<Gun>();

		originalColliderHeight = boxCollider.size.y;
		crouchingColliderHeight = originalColliderHeight / 2;
		originalColliderOffset = boxCollider.center.y;
		crouchingColliderOffset = originalColliderOffset - (crouchingColliderHeight / 2);

		health = maxHealth;

		lastHitTime = Time.time - invincibilityPeriod;
	}

	void Update()
	{
		right = Input.GetButton("Right");
		left = Input.GetButton("Left");
		run = Input.GetButton("Run");
		jump = jump || Input.GetButtonDown("Jump");
		crouch = Input.GetButton("Crouch");

		run = run && (right || left);

		anim.SetBool("Walking", right || left);
		anim.SetBool("Running", run);
		anim.SetBool("Crouching", crouch);
	}

	void FixedUpdate()
	{
		velocity = controller.velocity;

		if (controller.isGrounded)
		{
			velocity.y = 0f;
		}

		anim.SetBool("Grounded", controller.isGrounded);
		anim.SetBool("Falling", velocity.y < 0f);

		if (combo > 1)
		{
			comboTimer += Time.deltaTime;

			if (comboTimer >= Mathf.Clamp(comboDecreaseTime - (0.25f * (currentMaxCombo - combo)), comboDecreaseTime * 0.25f, comboDecreaseTime))
			{
				killChain = nextCombo - (combo * 2f) - 1f;
				combo--;
				comboTimer = 0f;
			}
		}

		if (health > 0f)
		{
			canTakeDamage = Time.time > lastHitTime + invincibilityPeriod;

			if (!canTakeDamage)
			{
				flashTimer += Time.deltaTime;

				smoothFlashTime = Mathf.Lerp(smoothFlashTime, 0.05f, 0.025f);

				if (flashTimer > smoothFlashTime)
				{
					spriteRenderer.enabled = !spriteRenderer.enabled;

					flashTimer = 0f;
				}
			}
			else
			{
				spriteRenderer.enabled = true;
				smoothFlashTime = flashTime;
			}
		}

		if (crouch)
		{
			normalizedHorizontalSpeed = 0f;
			boxCollider.size = new Vector2(boxCollider.size.x, crouchingColliderHeight);
			boxCollider.center = new Vector2(boxCollider.center.x, crouchingColliderOffset);
		}
		else
		{
			boxCollider.size = new Vector2(boxCollider.size.x, originalColliderHeight);
			boxCollider.center = new Vector2(boxCollider.center.x, originalColliderOffset);
		}

		if (!crouch || (crouch && !controller.isGrounded))
		{
			if (right)
			{
				normalizedHorizontalSpeed = 1f;

				if (transform.localScale.x < 0f)
				{
					Flip();
				}
			}
			else if (left)
			{
				normalizedHorizontalSpeed = -1f;

				if (transform.localScale.x > 0f)
				{
					Flip();
				}
			}
			else
			{
				normalizedHorizontalSpeed = 0f;
			}
		}

		if (jump && controller.isGrounded)
		{
			velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
			anim.SetTrigger("Jump");
		}

		if (run)
		{
			runFullTimer += Time.deltaTime;

			if (runFullTimer >= runFullTime)
			{
				runFull = true;
				anim.SetBool("Running_Full", runFull);
			}
		}
		else if (runFullTimer > 0f)
		{
			runFullTimer = 0f;
			runFull = false;
			anim.SetBool("Running_Full", runFull);
		}

		float smoothedMovementFactor = controller.isGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * (run ? (runFull ? runFullSpeed : runSpeed) : walkSpeed) * speedMultiplier, Time.fixedDeltaTime * smoothedMovementFactor);
		velocity.y += gravity * Time.fixedDeltaTime;

		controller.move(velocity * Time.fixedDeltaTime);

		jump = false;
	}

	void OnTriggerEnter2D(Collider2D enemy)
	{
		if (enemy.tag == "Enemy" || enemy.tag == "Projectile")
		{
			if (canTakeDamage)
			{
				if (health > 0f)
				{
					TakeDamage(enemy.gameObject);
				}
			}
		}
	}

	void OnTriggerStay2D(Collider2D enemy)
	{
		OnTriggerEnter2D(enemy);
	}

	void TakeDamage(GameObject enemy)
	{
		float damage = 0f;
		float knockback = 0f;

		if (enemy.tag == "Enemy")
		{
			damage = enemy.GetComponent<Enemy>().damage;
			knockback = enemy.GetComponent<Enemy>().knockback;
		}
		else if (enemy.tag == "Projectile")
		{
			damage = enemy.GetComponent<Projectile>().damage;
			knockback = enemy.GetComponent<Projectile>().knockback;
			enemy.GetComponent<Projectile>().CheckDestroy();
		}

		health -= damage;

		if (health <= 0f)
		{
			spriteRenderer.enabled = false;
			collider2D.enabled = false;
			explodeEffect.Explode(velocity, spriteRenderer.sprite);
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
			lastHitTime = Time.time;
		}
	}

	void Flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

		if (runFullTimer > 0f)
		{
			runFullTimer = 0f;
			runFull = false;
			anim.SetBool("Running_Full", runFull);
		}

		if (Mathf.Abs(velocity.x) > turningSpeed && controller.isGrounded)
		{
			anim.SetTrigger("Turn");
		}
	}

	public void AddHealth(float amount)
	{
		health = Mathf.Clamp(health + amount, health, maxHealth);
	}

	public void AddPoints(float points)
	{
		score += points * combo;
	}

	public void AddPointsFromEnemy(float enemyHealth, float enemyDamage)
	{
		killChain++;
		comboTimer = 0f;
		nextCombo = comboStartKills - 1f;

		for (int i = 1; i <= combo; i++)
		{
			nextCombo += i;
		}

		if (killChain >= nextCombo)
		{
			combo++;
			currentMaxCombo = combo;
		}

		score += Mathf.RoundToInt(enemyHealth * enemyDamage + (enemyHealth / maxHealth * 100)) * combo;
	}

	public void SwapGun(Gun newGun)
	{
		Transform oldTransform = gun.transform;
		Destroy(gun.gameObject);
		Gun gunInstance = Instantiate(newGun, oldTransform.position, oldTransform.rotation) as Gun;
		gunInstance.transform.parent = transform;
		gun = gunInstance;
	}

	public void ResetSpeed(float delay)
	{
		StartCoroutine(ResetSpeedCoroutine(delay));
	}

	private IEnumerator ResetSpeedCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		speedMultiplier = 1f;
	}
}


