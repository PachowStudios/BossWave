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
		//Legendary,
		//Godly,
		//Boss,
		NUM_TYPES
	};

	public string gunName;
	public Projectile projectile;
	public bool aimAtMouse = true;
	public float shootCooldown = 0.5f;
	public RarityLevel rarity = RarityLevel.Common;

	[HideInInspector]
	public bool disableInput = false;

	private bool shoot;
	private float shootTimer = 0f;

	#if !MOBILE_INPUT
	private bool useMouse = true;
	#endif

	private Transform firePoint;

	public bool FacingRight
	{
		get
		{
			return transform.rotation.eulerAngles.y == 0f;
		}
	}

	void Awake()
	{
		firePoint = transform.FindChild("FirePoint");
	}

	void Update()
	{
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
	}

	void FixedUpdate()
	{
		if (!disableInput)
		{
			Vector3 shotDirection = RotateTowardsMouse();

			shootTimer += Time.deltaTime;

			if (shoot && shootTimer >= shootCooldown)
			{
				Projectile projectileInstance = Instantiate(projectile, firePoint.position, Quaternion.identity) as Projectile;
				projectileInstance.direction = shotDirection;

				shootTimer = 0f;
			}
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
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = 10f;
			Vector3 gunLookPosition = Camera.main.ScreenToWorldPoint(mousePosition);
			gunLookPosition -= transform.position;
			float gunAngle = Mathf.Atan2(gunLookPosition.y, gunLookPosition.x) * Mathf.Rad2Deg;
			newEuler = Quaternion.AngleAxis(gunAngle, Vector3.forward).eulerAngles;
		}
		else
		{
			newEuler = Quaternion.Euler(0, 0, Mathf.Atan2(CrossPlatformInputManager.GetAxis("XboxGunY"), CrossPlatformInputManager.GetAxis("XboxGunX")) * Mathf.Rad2Deg).eulerAngles;
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
