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
			RotateTowardsMouse();

			shootTimer += Time.deltaTime;

			if (shoot && shootTimer >= shootCooldown)
			{
				Projectile projectileInstance = Instantiate(projectile, firePoint.position, Quaternion.identity) as Projectile;
				projectileInstance.direction = transform.right;

				shootTimer = 0f;
			}
		}
	}

	void RotateTowardsMouse()
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

		newEuler.z = newEuler.z < 180f ? Mathf.Clamp(newEuler.z, 0f, 90f) : Mathf.Clamp(newEuler.z, 270f, 360f);
		transform.rotation = Quaternion.Euler(newEuler);

		anim.SetFloat("Gun Angle", transform.rotation.eulerAngles.z);
	}
}
