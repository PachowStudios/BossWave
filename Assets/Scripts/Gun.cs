using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour 
{
	public enum RarityLevel
	{
		Common,
		Uncommon,
		Rare,
		VeryRare,
		Legendary,
		Godly,
		//Boss,
		NUM_ITEMS
	};

	public string gunName;
	public RarityLevel rarity = RarityLevel.Common;
	public Projectile projectile;
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

	private bool shoot;
	private bool previousShoot;
	private bool shootStart;
	private float shootTimer = 0f;
	private bool overheated = false;
	private float overheatTimer = 0f;

	private Projectile projectileInstance;

	#if !MOBILE_INPUT
	private bool useMouse = true;
	#endif

	private SpriteRenderer spriteRenderer;

	public bool FacingRight
	{
		get
		{
			return transform.rotation.eulerAngles.y == 0f;
		}
	}

	public bool NoInput
	{
		get
		{
			return !shoot;
		}
	}

	public float FireRate
	{
		get
		{
			return Mathf.Round((1f / shootCooldown) * 10f) / 10f;
		}
	}

	void Awake()
	{
		firePoint = transform.FindChild("FirePoint");
		spriteRenderer = GetComponent<SpriteRenderer>();

		shootTimer = shootCooldown;
	}

	void Update()
	{
		previousShoot = shoot;

		#if MOBILE_INPUT
		shoot = CrossPlatformInputManager.GetAxis("GunRotation") != 0f;
		#else
		bool xboxInput = CrossPlatformInputManager.GetAxis("XboxGunX") != 0f || CrossPlatformInputManager.GetAxis("XboxGunY") != 0f;
		bool mouseInput = CrossPlatformInputManager.GetButton("Shoot");

		if (useMouse)
		{
			if (xboxInput)
			{
				useMouse = false;
			}

			shoot = mouseInput;
		}
		else
		{
			if (mouseInput)
			{
				useMouse = true;
			}

			shoot = xboxInput;
		}
		#endif

		shoot = disableInput ? false : shoot;

		shootStart = shootStart || (shoot && !previousShoot);
	}

	void FixedUpdate()
	{
		Vector3 shotDirection = RotateTowardsMouse();

		if (!disableInput && !overheated)
		{
			if (continuousFire)
			{
				if (shootStart)
				{
					projectileInstance = Instantiate(projectile, firePoint.position, Quaternion.identity) as Projectile;
					projectileInstance.direction = shotDirection;
					shootStart = false;
				}

				if (projectileInstance != null && !shoot)
				{
					Destroy(projectileInstance.gameObject);
				}
			}
			else
			{
				shootTimer += Time.deltaTime;

				if (shoot && shootTimer >= shootCooldown)
				{
					projectileInstance = Instantiate(projectile, firePoint.position, Quaternion.identity) as Projectile;
					projectileInstance.direction = shotDirection;

					shootTimer = 0f;
				}
			}
		}
		else
		{
			if (continuousFire && projectileInstance != null)
			{
				Destroy(projectileInstance.gameObject);
			}
		}

		if (canOverheat)
		{
			if (shoot && !disableInput && !overheated)
			{
				overheatTimer = Mathf.Clamp(overheatTimer + Time.deltaTime, 0f, overheatTime);

				if (overheatTimer >= overheatTime)
				{
					overheated = true;
					PlayerControl.instance.AddHealth(-overheatDamage);
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

			spriteRenderer.color = overheatGradient.Evaluate(overheatTimer / overheatTime);
		}
	}

	private Vector3 RotateTowardsMouse()
	{
		Vector3 newEuler;

		#if MOBILE_INPUT
		newEuler = Quaternion.Euler(0, 0, CrossPlatformInputManager.GetAxis("GunRotation")).eulerAngles;
		#else
		if (useMouse)
		{
			if (shoot)
			{
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.z = 10f;
				newEuler = new Vector3(0f, 0f, transform.LookAt2D(Camera.main.ScreenToWorldPoint(mousePosition)));
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
		#endif

		Transform originalTransform = transform;
		originalTransform.rotation = Quaternion.Euler(newEuler);
		Vector3 shotDirection = originalTransform.right;

		newEuler.y = newEuler.z > 90f && newEuler.z < 270f ? 180f : 0f;

		Vector3 newScale = transform.localScale;
		newScale.y = newEuler.y == 180f ? -1f : 1f;

		transform.localScale = newScale;
		transform.rotation = Quaternion.Euler(newEuler);

		return shotDirection;
	}
}
