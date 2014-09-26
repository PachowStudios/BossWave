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

	private Transform player;
	private Transform gun;

	new void Awake()
	{
		base.Awake();

		player = GameObject.Find("Player").transform;
		gun = transform.FindChild("Gun");

		detectionRange = Random.Range(minDetectionRange, maxDetectionRange);
	}

	void FixedUpdate()
	{
		InitialUpdate();

		anim.SetBool("Walking", right || left);

		if (player.position.x > transform.position.x + detectionRange)
		{
			right = true;
			left = !right;
		}
		else if (player.position.x > transform.position.x &&
				 transform.localScale.x < 0f)
		{
			Flip();
		}
		else if (player.position.x < transform.position.x - detectionRange)
		{
			left = true;
			right = !left;
		}
		else if (player.position.x < transform.position.x &&
				 transform.localScale.x > 0f)
		{
			Flip();
		}
		else
		{
			right = left = false;
		}

		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime)
		{
			if (Mathf.Abs(player.position.x - transform.position.x) <= detectionRange)
			{
				anim.SetTrigger("Shoot");

				Vector3 gunLookPosition = player.collider2D.bounds.center;
				gunLookPosition -= gun.transform.position;
				gunLookPosition.y += (Mathf.Abs(projectile.gravity) / 2f) * (Mathf.Abs(player.position.x - transform.position.x) / projectile.shotSpeed);
				float gunAngle = Mathf.Atan2(gunLookPosition.y, gunLookPosition.x) * Mathf.Rad2Deg;
				gun.transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);

				float bulletRotation = transform.localScale.x > 0 ? 0f : 180f;

				Projectile projectileInstance = Instantiate(projectile, gun.position, Quaternion.Euler(new Vector3(0, 0, bulletRotation))) as Projectile;
				projectileInstance.direction = gun.transform.right;

				cooldownTimer = 0f;
			}
		}

		GetMovement();
		ApplyMovement();
	}
}
