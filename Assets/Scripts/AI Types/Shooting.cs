using UnityEngine;
using System.Collections;

public class Shooting : Enemy
{
	public Projectile projectile;
	public float followBuffer = 1.5f;
	public float detectionRange = 20f;
	public float cooldownTime = 2f;

	private float cooldownTimer = 0f;

	private Transform player;
	private Transform gun;

	new void Awake()
	{
		base.Awake();

		player = GameObject.Find("Player").transform;
		gun = transform.FindChild("Gun");
	}

	void FixedUpdate()
	{
		InitialUpdate();

		anim.SetBool("Walking", right || left);

		if (player.position.x > transform.position.x + followBuffer)
		{
			right = true;
			left = !right;
		}
		else if (player.position.x > transform.position.x &&
				 body.localScale.x < 0f)
		{
			Flip();
		}
		else if (player.position.x < transform.position.x - followBuffer)
		{
			left = true;
			right = !left;
		}
		else if (player.position.x < transform.position.x &&
				 body.localScale.x > 0f)
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
