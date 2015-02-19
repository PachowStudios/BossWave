using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
	#region Fields
	public enum RarityLevel
	{
		Common,
		Uncommon,
		Rare,
		VeryRare,
		Legendary,
		Godly,
		Boss
	};

	public string gunName;
	public RarityLevel rarity = RarityLevel.Common;
	public Projectile projectile;

	public bool secondaryShot = false;
	public Projectile secondaryProjectile;
	public float secondaryCooldown = 5f;
	public bool showSecondaryGUI = false;
	public Sprite secondaryIcon;

	public bool continuousFire = false;
	public float shootCooldown = 0.5f;

	public bool canOverheat = false;
	public float overheatTime = 5f;
	public float overheatDamage = 0f;
	[Range(0f, 1f)]
	public float overheatThreshold = 0.5f;
	public Gradient overheatGradient;

	[HideInInspector]
	public bool disableInput = false;
	[HideInInspector]
	public Transform firePoint;
	[HideInInspector]
	public float secondaryTimer;

	private bool shoot;
	private bool previousShoot;
	private bool shootStart;
	private bool secondaryShoot;

	private float shootTimer;

	private bool overheated = false;
	private float overheatTimer = 0f;

	private Projectile projectileInstance;
	private Projectile secondaryProjectileInstance;

	private bool useMouse = true;

	private SpriteRenderer spriteRenderer;
	#endregion

	#region Public Properties
	public Color Color
	{
		get
		{
			switch (rarity)
			{
				case RarityLevel.Common:
					return new Color(0.533f, 0.549f, 0.471f);
				case RarityLevel.Uncommon:
					return new Color(0.424f, 0.533f, 1f);
				case RarityLevel.Rare:
					return new Color(0.655f, 0.929f, 0f);
				case RarityLevel.VeryRare:
					return new Color(1f, 0.565f, 0f);
				case RarityLevel.Legendary:
					return new Color(1f, 0.133f, 0.271f);
				case RarityLevel.Godly:
					return new Color(1f, 0.133f, 0.662f);
				case RarityLevel.Boss:
					return new Color(0.38f, 0.075f, 0.506f);
				default:
					return Color.white;
			}
		}
	}

	public SpriteRenderer SpriteRenderer
	{
		get 
		{
			if (spriteRenderer != null)
			{
				return spriteRenderer;
			}
			else
			{
				return GetComponent<SpriteRenderer>();
			}
		}
	}

	public bool FacingRight
	{
		get { return transform.rotation.eulerAngles.y == 0f; }
	}

	public bool NoInput
	{
		get
		{
			RotateTowardsMouse();
			return !shoot && !secondaryShoot;
		}
	}

	public float FireRate
	{
		get { return Mathf.Round((1f / shootCooldown) * 10f) / 10f; }
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		firePoint = transform.FindChild("FirePoint");
		spriteRenderer = GetComponent<SpriteRenderer>();

		shootTimer = shootCooldown;
		secondaryTimer = secondaryCooldown;
	}

	private void Update()
	{
		GetInput();
	}

	private void LateUpdate()
	{
		CheckShoot();
	}

	private void OnDisable()
	{
		if (continuousFire && projectileInstance != null)
		{
			Destroy(projectileInstance.gameObject);
		}
	}
	#endregion

	#region Private Update Methods
	private void GetInput()
	{
		previousShoot = shoot;

		bool xboxInput = CrossPlatformInputManager.GetAxis("XboxGunX") != 0f || CrossPlatformInputManager.GetAxis("XboxGunY") != 0f;
		bool mouseInput = CrossPlatformInputManager.GetButton("Shoot");
		bool secondaryMouseInput = CrossPlatformInputManager.GetButton("SecondaryShoot");

		if (useMouse)
		{
			if (xboxInput)
			{
				useMouse = false;
			}

			shoot = mouseInput;
			secondaryShoot = secondaryMouseInput;
		}
		else
		{
			if (mouseInput || secondaryMouseInput)
			{
				useMouse = true;
			}

			shoot = xboxInput;
		}

		shoot = disableInput ? false : shoot;
		secondaryShoot = disableInput ? false
									  : secondaryShot ? secondaryShoot
													  : false;

		shootStart = shootStart || (shoot && !previousShoot);
	}

	private void CheckShoot()
	{
		Vector3 shotDirection = RotateTowardsMouse();

		if (!disableInput && !overheated)
		{
			if (continuousFire)
			{
				if (shootStart && projectileInstance == null)
				{
					projectileInstance = Instantiate(projectile, firePoint.position, Quaternion.identity) as Projectile;
					projectileInstance.Initialize(shotDirection);
					shootStart = false;
				}
			}
			else
			{
				shootTimer += Time.deltaTime;

				if (shoot && shootTimer >= shootCooldown)
				{
					projectileInstance = Instantiate(projectile, firePoint.position, Quaternion.identity) as Projectile;
					projectileInstance.Initialize(shotDirection);

					shootTimer = 0f;
				}

				if (secondaryShot)
				{
					secondaryTimer += Time.deltaTime;

					if (secondaryShoot && secondaryTimer >= secondaryCooldown)
					{
						secondaryProjectileInstance = Instantiate(secondaryProjectile, firePoint.position, Quaternion.identity) as Projectile;
						secondaryProjectileInstance.Initialize(shotDirection);

						secondaryTimer = 0f;
					}
				}
			}
		}

		if (continuousFire && projectileInstance != null &&
			(disableInput || !shoot))
		{
			Destroy(projectileInstance.gameObject);
		}

		if (canOverheat)
		{
			if ((shoot || secondaryShoot) && !disableInput && !overheated)
			{
				overheatTimer = Mathf.Clamp(overheatTimer + Time.deltaTime, 0f, overheatTime);

				if (overheatTimer >= overheatTime)
				{
					overheated = true;
					PlayerControl.Instance.Health -= overheatDamage;
				}
			}
			else
			{
				overheatTimer = Mathf.Clamp(overheatTimer - Time.deltaTime, 0f, overheatTime);

				if (overheatTimer <= overheatTime * overheatThreshold)
				{
					overheated = false;
				}
			}

			if (!NoInput)
			{
				spriteRenderer.color = overheatGradient.Evaluate(overheatTimer / overheatTime);
			}
		}

		if (disableInput || NoInput)
		{
			spriteRenderer.color = Color.clear;
		}
		else if (!canOverheat)
		{
			spriteRenderer.color = Color.white;
		}
	}
	#endregion

	#region Private Helper Methods
	private Vector3 RotateTowardsMouse()
	{
		Vector3 newEuler;

		if (useMouse)
		{
			if (shoot || secondaryShoot)
			{
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.z = 10f;
				newEuler = transform.position.LookAt2D(Camera.main.ScreenToWorldPoint(mousePosition)).eulerAngles;
			}
			else
			{
				newEuler = Vector3.zero;
			}
		}
		else
		{
			newEuler = Quaternion.Euler(0f, 0f, Mathf.Atan2(CrossPlatformInputManager.GetAxis("XboxGunY"), CrossPlatformInputManager.GetAxis("XboxGunX")) * Mathf.Rad2Deg).eulerAngles;
		}

		Vector3 shotDirection = Quaternion.Euler(newEuler) * Vector3.right;
		transform.CorrectScaleForRotation(newEuler, true);

		return shotDirection;
	}
	#endregion
}
