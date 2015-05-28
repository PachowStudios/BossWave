using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DG.Tweening;

public sealed class PlayerControl : MonoBehaviour
{
	#region Fields
	private static PlayerControl instance;

	public float maxHealth = 100f;
	public float invincibilityPeriod = 2f;
	public float gravity = -35f;
	public float walkSpeed = 10f;
	public float runSpeed = 17.5f;
	public bool continuouslyRunning = false;
	public float continuousRunSpeed = 10f;
	public bool continuouslyFalling = false;
	public float groundDamping = 10f;
	public float inAirDamping = 5f;
	public float jumpHeight = 5f;
	public float comboStartKills = 3f;
	public float comboDecreaseTime = 1f;
	public int maxScore = 999999999;
	public int maxMicrochips = 99999;
	public Transform popupMessagePoint;
	public Transform gunPoint;
	public List<Gun> startingGuns;
	public float gunSwapCooldownTime = 0.5f;
	public float minAltIdleTime = 5f;
	public float maxAltIdleTime = 10f;
	public List<string> altIdleAnimations;
	public LayerMask bottomCollider;

	private float health;
	private bool dead = false;
	private bool jump;
	private bool run;
	private bool usingGun = false;
	private bool disableInput = false;
	private bool inPortal = false;

	private Vector3 velocity;
	private Vector3 lastGroundedPosition;
	private float horizontalMovement = 0f;
	private float verticalMovement = 0f;
	private float speedMultiplier = 1f;

	private List<Gun> guns = new List<Gun>();
	private int currentGunIndex = 0;
	private float gunSwapCooldownTimer = 0f;

	private bool canTakeDamage = true;
	private float lastHitTime;
	private float flashTimer = 0f;
	private float flashTime = 0.25f;
	private float smoothFlashTime;

	private int score = 0;
	private int microchips = 0;
	private int combo = 1;
	private float currentMaxCombo = 1f;
	private float comboTimer = 0f;
	private float killChain = 0f;

	private float altIdleTimer = 0f;
	private float altIdleTime = 0f;

	private bool cancelGoTo = false;
	private bool useTargetPoint = false;
	private bool goToFaceRight = false;
	private bool reEnableAfterMove = true;
	private bool inertiaAfterMove = false;
	private Vector3 targetPoint;

	private LayerMask defaultCollisionLayers;
	private string defaultFullSortingLayer;
	private int defaultFullSortingOrder;

	private CharacterController2D controller;
	private Animator anim;
	private GhostTrailEffect ghostTrail;
	private List<SpriteRenderer> spriteRenderers;
	#endregion

	#region Public Properties
	public static PlayerControl Instance
	{ get { return instance; } }

	public float Health
	{
		get { return health; }

		set
		{
			if (value < health)
				lastHitTime = Time.time;

			health = Mathf.Clamp(value, 0f, maxHealth);
			CheckDeath();
		}
	}

	public float HealthPercent
	{ get { return Mathf.Clamp01(Health / maxHealth); } }

	public bool IsDead
	{ get { return dead; } }

	public int Score
	{ get { return score; } }

	public int Microchips
	{ get { return microchips; } }

	public int Combo
	{ get { return combo; } }

	public bool Jumped
	{ get { return jump; } }

	public bool IsGrounded
	{ get { return controller.IsGrounded && !continuouslyFalling; } }

	public bool IsInputDisabled
	{ get { return disableInput; } }

	public Vector3 Velocity
	{ get { return velocity; } }

	public Vector3 LastGroundedPosition
	{ get { return lastGroundedPosition; } }

	public Vector3 PopupMessagePoint
	{ get { return popupMessagePoint.position; } }

	public ReadOnlyCollection<SpriteRenderer> SpriteRenderers
	{ get { return spriteRenderers.AsReadOnly(); } }

	public ReadOnlyCollection<Gun> Guns
	{ get { return guns.AsReadOnly(); } }

	public Gun Gun
	{ get { return guns.ElementAtOrDefault(currentGunIndex); } }

	public bool GunsFull
	{ get { return guns.Count >= startingGuns.Capacity; } }
	#endregion

	#region Internal Properties
	private bool Right
	{ get { return horizontalMovement > 0f; } }

	private bool Left
	{ get { return horizontalMovement < 0f; } }

	private bool FacingRight
	{ get { return transform.localScale.x > 0; } }

	private float NewAltIdleTime
	{ get { return Random.Range(minAltIdleTime, maxAltIdleTime); } }
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		controller = GetComponent<CharacterController2D>();
		anim = GetComponent<Animator>();
		ghostTrail = GetComponent<GhostTrailEffect>();
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList<SpriteRenderer>();

		health = maxHealth;

		lastHitTime = Time.time - invincibilityPeriod;
		altIdleTime = NewAltIdleTime;

		defaultCollisionLayers = controller.platformMask;
	}

	private void Start()
	{
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if (spriteRenderer.name != "Full")
			{
				spriteRenderer.color = Color.clear;
			}
			else
			{
				defaultFullSortingLayer = spriteRenderer.sortingLayerName;
				defaultFullSortingOrder = spriteRenderer.sortingOrder;
			}
		}

		foreach (Gun startingGun in startingGuns)
		{
			if (startingGun != null)
			{
				AddGun(startingGun);
			}
		}
	}

	private void Update()
	{
		if (!dead)
		{
			GetInput();
			ApplyAnimation();

			if (useTargetPoint && disableInput)
				UpdateGoTo();

			if (combo > 1)
				UpdateCombo();

			if (Health > 0f)
				UpdateInvincibilityFlash();
		}
	}

	private void LateUpdate()
	{
		GetMovement();
		ApplyMovement();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Enemy" || other.tag == "Projectile")
		{
			if (canTakeDamage && Health > 0f)
				TakeDamage(other.gameObject);
		}
		else if (other.tag == "Portal")
		{
			inPortal = true;
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		OnTriggerEnter2D(other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "Portal")
		{
			inPortal = false;
		}
	}
	#endregion

	#region Internal Update Methods
	private void GetInput()
	{
		#if UNITY_EDITOR
		if (CrossPlatformInputManager.GetButtonDown("Speedup"))
		{
			TimeWarpEffect.Instance.StartWarp(5f, 0.5f, GameObject.FindObjectsOfType<AudioSource>());
		}
		else if (CrossPlatformInputManager.GetButtonDown("Slowdown"))
		{
			TimeWarpEffect.Instance.EndWarp(0.5f, GameObject.FindObjectsOfType<AudioSource>());
		}
		#endif

		if (!disableInput)
		{
			horizontalMovement = CrossPlatformInputManager.GetAxis("Horizontal");
			verticalMovement = CrossPlatformInputManager.GetAxis("Vertical");
			run = CrossPlatformInputManager.GetButton("Run") && Gun.NoInput;
			jump = jump || (CrossPlatformInputManager.GetButtonDown("Jump") && IsGrounded);

			gunSwapCooldownTimer += Time.deltaTime;

			if (gunSwapCooldownTimer >= gunSwapCooldownTime)
			{
				for (int i = 0; i < guns.Count; i++)
				{
					if (CrossPlatformInputManager.GetButtonDown("Gun" + (i + 1).ToString()))
					{
						SelectGun(i, true);
						break;
					}
				}

				int scrollWheelInput = (int)CrossPlatformInputManager.GetAxisRaw("Mouse ScrollWheel");

				if (scrollWheelInput != 0)
					SelectGun(currentGunIndex + scrollWheelInput, true);
			}
		}

		run = ((run && horizontalMovement != 0f) || continuouslyRunning) && !continuouslyFalling;
	}

	private void ApplyAnimation()
	{
		anim.SetBool("Walking", (horizontalMovement != 0f || continuouslyRunning) && IsGrounded);
		anim.SetBool("Running", run);
		anim.SetBool("Grounded", IsGrounded);
		anim.SetBool("Falling", velocity.y < 0f || continuouslyFalling);
		anim.SetFloat("Gun Angle", Gun.transform.rotation.eulerAngles.z);
		ghostTrail.trailActive = speedMultiplier > 1f;
	}

	private void UpdateGoTo()
	{
		if (transform.position.x < targetPoint.x && !Left)
			horizontalMovement = 1f;
		else if (transform.position.x > targetPoint.x && !Right)
			horizontalMovement = -1f;
		else
			cancelGoTo = true;

		if (cancelGoTo)
		{
			if (goToFaceRight && !FacingRight)
				transform.Flip();
			if (!goToFaceRight && FacingRight)
				transform.Flip();

			ResetInput();

			useTargetPoint = false;

			if (!inertiaAfterMove)
				velocity.x = 0f;

			if (reEnableAfterMove)
				EnableInput();
		}
	}

	private void UpdateCombo()
	{
		comboTimer += Time.deltaTime;

		if (comboTimer >= Mathf.Clamp(comboDecreaseTime - (0.25f * (currentMaxCombo - combo)), comboDecreaseTime * 0.25f, comboDecreaseTime))
		{
			combo--;
			killChain = combo == 1 ? 0f : GetNextCombo() - combo;
			comboTimer = 0f;
		}
	}

	private void UpdateInvincibilityFlash()
	{
		canTakeDamage = Time.time > lastHitTime + invincibilityPeriod;

		if (!canTakeDamage)
		{
			flashTimer += Time.deltaTime;
			smoothFlashTime = Mathf.Lerp(smoothFlashTime, 0.05f, 0.025f);

			if (flashTimer > smoothFlashTime)
			{
				SetRenderersEnabled(alternate: true);
				flashTimer = 0f;
			}
		}
		else
		{
			SetRenderersEnabled(true);
			smoothFlashTime = flashTime;
		}
	}

	private void GetMovement()
	{
		if (horizontalMovement == 0f && IsGrounded && Gun.NoInput)
		{
			altIdleTimer += Time.deltaTime;

			if (altIdleTimer >= altIdleTime)
			{
				anim.SetTrigger(altIdleAnimations[Random.Range(0, altIdleAnimations.Count)]);
				altIdleTimer = 0f;
				altIdleTime = NewAltIdleTime;
			}
		}

		if (Gun.NoInput)
		{
			if (usingGun)
			{
				usingGun = false;
				SetRenderersVisible(alternate: true);
			}

			if (continuouslyRunning && !FacingRight)
				transform.Flip();

			if (Right && !FacingRight)
				transform.Flip();
			else if (Left && FacingRight && !continuouslyRunning)
				transform.Flip();
		}
		else
		{
			if (!usingGun)
			{
				usingGun = true;
				SetRenderersVisible(alternate: true);
			}

			if (Gun.FacingRight && !FacingRight)
				transform.Flip();
			else if (!Gun.FacingRight && FacingRight)
				transform.Flip();
		}

		if (jump && IsGrounded)
		{
			if (!inPortal)
				Jump(jumpHeight);

			jump = false;
		}
	}

	private void ApplyMovement()
	{
		float smoothedMovementFactor = IsGrounded ? groundDamping : inAirDamping;

		velocity.x = Mathf.Lerp(velocity.x,
								horizontalMovement * (run ? (continuouslyRunning && !useTargetPoint ? continuousRunSpeed
																									: runSpeed)
														  : walkSpeed) * speedMultiplier,
								smoothedMovementFactor * Time.deltaTime);

		if (continuouslyFalling)
			velocity.y = Mathf.Lerp(velocity.y,
									verticalMovement * walkSpeed,
									smoothedMovementFactor * Time.deltaTime);
		else
			velocity.y += gravity * Time.deltaTime;

		controller.Move(velocity * Time.deltaTime);
		velocity = controller.Velocity;

		if (IsGrounded)
		{
			velocity.y = 0f;
			lastGroundedPosition = transform.position;
		}
	}
	#endregion

	#region Internal Helper Methods
	private void Jump(float height, bool playAnimation = true)
	{
		if (height > 0f)
		{
			velocity.y = Mathf.Sqrt(2f * height * -gravity);

			if (playAnimation)
				anim.SetTrigger("Jump");
		}
	}

	private void SelectGun(int gunIndex, bool showPopup = false)
	{
		gunIndex = Extensions.ClampWrap(gunIndex, 0, guns.Count - 1);

		if (showPopup)
		{
			if (!PopupSwapGun.Instance.ShowingPopup)
				PopupMessage.Instance.CreatePopup(PopupMessagePoint, "", guns[gunIndex].SpriteRenderer.sprite, true);

			CurrentGunName.Instance.Show(guns[gunIndex].gunName, guns[gunIndex].Color, gunSwapCooldownTime);
		}

		if (gunIndex == currentGunIndex)
		{
			guns[currentGunIndex].disableInput = false;
		}
		else
		{
			guns[currentGunIndex].disableInput = true;
			guns[gunIndex].disableInput = false;
			currentGunIndex = gunIndex;
		}

		gunSwapCooldownTimer = 0f;
	}

	private void ResetInput()
	{
		horizontalMovement = 0f;
		run = jump = false;
	}

	private bool CheckDeath()
	{
		if (Health <= 0f && !dead)
		{
			dead = true;

			StartCoroutine(GameMenu.Instance.GameOver());
			DisableInput();
			SetRenderersEnabled(false);
			collider2D.enabled = false;
			PopupSwapGun.Instance.ClearPopup();

			foreach (SpriteRenderer sprite in spriteRenderers)
			{
				sprite.transform.localScale = transform.localScale;
				ExplodeEffect.Instance.Explode(sprite.transform, velocity, sprite.sprite);
			}
		}

		return dead;
	}

	private void SetRenderersEnabled(bool enabled = true, bool alternate = false)
	{
		foreach (SpriteRenderer sprite in spriteRenderers)
		{
			if (alternate)
				sprite.enabled = !sprite.enabled;
			else
				sprite.enabled = enabled;
		}
	}

	private void SetRenderersVisible(bool enabled = true, bool alternate = false)
	{
		foreach (SpriteRenderer sprite in spriteRenderers)
		{
			if (alternate)
				sprite.color = sprite.color == Color.white ? Color.clear : Color.white;
			else
				sprite.color = enabled ? Color.white : Color.clear;
		}
	}

	private float GetNextCombo()
	{
		float nextCombo = comboStartKills - 1f;

		for (int i = 1; i <= combo; i++)
			nextCombo += i;

		return nextCombo;
	}
	#endregion

	#region Public Methods
	public void TakeDamage(GameObject enemy, float damage = 0f, Vector2 knockback = default(Vector2))
	{
		if (!canTakeDamage)
			return;

		float knockbackDirection = 1f;

		if (enemy.tag == "Enemy")
		{
			Enemy currentEnemy = enemy.GetComponent<Enemy>();

			if (!currentEnemy.spawned)
				return;

			damage = (damage == 0f) ? currentEnemy.damage : damage;
			knockback = (knockback == default(Vector2)) ? currentEnemy.knockback : knockback;
			knockbackDirection = Mathf.Sign(transform.position.x - enemy.transform.position.x);
		}
		else if (enemy.tag == "Projectile")
		{
			Projectile currentProjectile = enemy.GetComponent<Projectile>();
			damage = currentProjectile.damage;
			knockback = currentProjectile.knockback;
			knockbackDirection = Mathf.Sign(currentProjectile.direction.x);
			currentProjectile.CheckDestroyEnemy();
		}

		if (damage != 0f)
		{
			Health -= damage;

			if (Health > 0f && knockback != default(Vector2))
			{
				Sequence knockbackSequence = DOTween.Sequence();

				knockbackSequence
					.AppendInterval(0.1f)
					.AppendCallback(() =>
					{
						velocity.x = Mathf.Sqrt(Mathf.Pow(knockback.x, 2) * -gravity) * knockbackDirection;

						if (IsGrounded)
							velocity.y = Mathf.Sqrt(knockback.y * -gravity);

						controller.Move(velocity * Time.deltaTime);
						velocity = controller.Velocity;
						lastHitTime = Time.time;
					});
			}
		}
	}

	public void Move(Vector3 velocity)
	{
		controller.Move(velocity * Time.deltaTime);
		this.velocity = controller.Velocity;
	}

	public int AddPoints(int points, bool ignoreCombo = false)
	{
		int newPoints = points * (ignoreCombo ? 1 : combo);
		score = Mathf.Clamp(score + newPoints, 0, maxScore);

		return newPoints;
	}

	public int AddPointsFromEnemy(float enemyHealth, float enemyDamage)
	{
		killChain++;
		comboTimer = 0f;

		if (killChain >= GetNextCombo())
		{
			combo++;
			currentMaxCombo = combo;
		}

		int newPoints = Mathf.RoundToInt(enemyHealth * enemyDamage + (enemyHealth / maxHealth * 100)) * combo;
		score = Mathf.Clamp(score + newPoints, 0, maxScore);

		return newPoints;
	}

	public void AddMicrochips(int newMicrochips)
	{
		microchips = Mathf.Clamp(microchips + newMicrochips, 0, maxMicrochips);
	}

	public void AddGun(Gun newGun)
	{
		int gunIndex = GunsFull ? currentGunIndex : guns.Count;

		if (guns.ElementAtOrDefault(gunIndex) != null)
		{
			Destroy(guns[gunIndex].gameObject);
			guns.RemoveAt(gunIndex);
		}

		Gun gunInstance = Instantiate(newGun, gunPoint.position, gunPoint.rotation) as Gun;
		gunInstance.name = newGun.name;
		gunInstance.transform.parent = transform;
		gunInstance.transform.localScale = gunPoint.localScale;
		gunInstance.SpriteRenderer.color = Color.clear;
		gunInstance.disableInput = true;
		guns.Insert(gunIndex, gunInstance);

		if (gunIndex == currentGunIndex)
			SelectGun(gunIndex);
	}

	public void SpeedBoost(float multiplier, float length)
	{
		speedMultiplier = multiplier;

		DOTween.Sequence()
			.AppendInterval(length)
			.AppendCallback(() => speedMultiplier = 1f);
	}

	public IEnumerator JumpToFloor()
	{
		LayerMask originalCollider = controller.platformMask;

		if (IsGrounded)
		{
			Jump(1f, false);
			Move(velocity);
		}

		controller.platformMask = bottomCollider;

		while (!IsGrounded)
			yield return new WaitForSeconds(0.1f);

		controller.platformMask = originalCollider;
		DisableInput();
	}

	public void GoToPoint(Vector3 point, bool faceRight, bool autoEnableInput = true, bool inertia = false)
	{
		targetPoint = point;
		useTargetPoint = true;
		goToFaceRight = faceRight;
		cancelGoTo = false;
		reEnableAfterMove = autoEnableInput;
		inertiaAfterMove = inertia;
		DisableInput();
	}

	public void CancelGoTo()
	{
		cancelGoTo = true;
	}

	public void DisableInput()
	{
		disableInput = true;
		Gun.disableInput = true;
		ResetInput();
	}

	public void EnableInput()
	{
		ResetInput();
		disableInput = false;
		Gun.disableInput = false;
	}

	public void SetFullSorting(string sortingLayer, int sortingOrder)
	{
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if (spriteRenderer.name == "Full")
			{
				spriteRenderer.sortingLayerName = sortingLayer;
				spriteRenderer.sortingOrder = sortingOrder;
			}
		}
	}

	public void RestoreDefaultFullSorting()
	{
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if (spriteRenderer.name == "Full")
			{
				spriteRenderer.sortingLayerName = defaultFullSortingLayer;
				spriteRenderer.sortingOrder = defaultFullSortingOrder;
			}
		}
	}

	public void SetCollisionLayers(LayerMask collisionLayers)
	{
		controller.platformMask = collisionLayers;
	}

	public void RestoreDefaultCollisionLayers()
	{
		controller.platformMask = defaultCollisionLayers;
	}
	#endregion
}


