using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour 
{
	public enum RarityLevel
	{
		Common,
		Uncommon,
		Rare,
		//VeryRare,
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

	private Transform firePoint;
	private Animator anim;

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
		anim = GetComponentInParent<Animator>();
	}

	void Update()
	{
		#if MOBILE_INPUT
		shoot = CrossPlatformInputManager.GetAxis("GunRotation") != 0f;
		#else
		shoot = CrossPlatformInputManager.GetButton("Shoot");
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
		#if MOBILE_INPUT
		Vector3 newEuler = Quaternion.Euler(0, 0, CrossPlatformInputManager.GetAxis("GunRotation")).eulerAngles;
		#else
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 10f;
		Vector3 gunLookPosition = Camera.main.ScreenToWorldPoint(mousePosition);
		gunLookPosition -= transform.position;
		float gunAngle = Mathf.Atan2(gunLookPosition.y, gunLookPosition.x) * Mathf.Rad2Deg;
		Vector3 newEuler = Quaternion.AngleAxis(gunAngle, Vector3.forward).eulerAngles;
		#endif

		Transform originalTransform = transform;
		originalTransform.rotation = Quaternion.Euler(newEuler);
		Vector3 shotDirection = originalTransform.right;

		newEuler.y = newEuler.z > 90f && newEuler.z < 270f ? 180f : 0f;

		Vector3 newScale = transform.localScale;
		newScale.y = newEuler.y == 180f ? -1f : 1f;

		transform.localScale = newScale;
		transform.rotation = Quaternion.Euler(newEuler);

		anim.SetFloat("Gun Angle", transform.rotation.eulerAngles.z);

		return shotDirection;
	}
}
