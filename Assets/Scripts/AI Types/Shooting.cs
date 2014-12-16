using UnityEngine;
using System.Collections;

public class Shooting : Enemy
{
	public Projectile projectile;
	public float minDetectionRange = 10f;
	public float maxDetectionRange = 15f;
	public float cooldownTime = 2f;

	private float cooldownTimer = 0f;
	private float detectionRange;

	private Transform gun;

	new void Awake()
	{
		base.Awake();

		gun = transform.FindChild("Gun");

		detectionRange = Random.Range(minDetectionRange, maxDetectionRange);
	}

	void FixedUpdate()
	{
		InitialUpdate();

		anim.SetBool("Walking", right || left);

		if (PlayerControl.instance.transform.position.x > transform.position.x + detectionRange)
		{
			right = true;
			left = !right;
		}
		else if (PlayerControl.instance.transform.position.x > transform.position.x &&
				 transform.localScale.x < 0f)
		{
			Flip();
		}
		else if (PlayerControl.instance.transform.position.x < transform.position.x - detectionRange)
		{
			left = true;
			right = !left;
		}
		else if (PlayerControl.instance.transform.position.x < transform.position.x &&
				 transform.localScale.x > 0f)
		{
			Flip();
		}
		else
		{
			right = left = false;
		}

		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime && Mathf.Abs(PlayerControl.instance.transform.position.x - transform.position.x) <= detectionRange)
		{
			anim.SetTrigger("Shoot");

			Vector3 gunLookPosition = PlayerControl.instance.transform.collider2D.bounds.center;
			gunLookPosition -= gun.transform.position;
			gunLookPosition.y += (Mathf.Abs(projectile.gravity) / 2f) * (Mathf.Abs(PlayerControl.instance.transform.position.x - transform.position.x) / projectile.shotSpeed);
			float gunAngle = Mathf.Atan2(gunLookPosition.y, gunLookPosition.x) * Mathf.Rad2Deg;
			gun.transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);

			float bulletRotation = transform.localScale.x > 0 ? 0f : 180f;

			Projectile projectileInstance = Instantiate(projectile, gun.position, Quaternion.Euler(new Vector3(0, 0, bulletRotation))) as Projectile;
			projectileInstance.Initialize(gun.transform.right);
			
			cooldownTimer = 0f;
		}

		GetMovement();
		ApplyMovement();
	}
}
