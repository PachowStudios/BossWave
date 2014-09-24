using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour 
{
	public Projectile projectile;
	public bool aimAtMouse = true;
	public float shootCooldown = 0.5f;

	private bool shoot;
	private float shootTimer = 0f;

	void Update()
	{
		shoot = Input.GetButton("Shoot");
	}

	void FixedUpdate()
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

	void RotateTowardsMouse()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 10f;
		Vector3 gunLookPosition = Camera.main.ScreenToWorldPoint(mousePosition);
		gunLookPosition -= transform.position;
		float gunAngle = Mathf.Atan2(gunLookPosition.y, gunLookPosition.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);
	}
}
