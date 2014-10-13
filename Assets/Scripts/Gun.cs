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
				Projectile projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity) as Projectile;
				projectileInstance.direction = transform.right;

				shootTimer = 0f;
			}
		}
	}

	void RotateTowardsMouse()
	{
		#if MOBILE_INPUT
		transform.rotation = Quaternion.Euler(0, 0, CrossPlatformInputManager.GetAxis("GunRotation"));
		#else
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 10f;
		Vector3 gunLookPosition = Camera.main.ScreenToWorldPoint(mousePosition);
		gunLookPosition -= transform.position;
		float gunAngle = Mathf.Atan2(gunLookPosition.y, gunLookPosition.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);
		#endif
	}
}
