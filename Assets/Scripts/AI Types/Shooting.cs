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
		gun = transform.Find("gun");
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
		else if (player.position.x < transform.position.x - followBuffer)
		{
			left = true;
			right = !left;
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
				if ((player.position.x < transform.position.x && transform.localScale.x > 0) ||
					(player.position.x > transform.position.x && transform.localScale.x < 0))
				{
					Flip();
				}

				anim.SetTrigger("Shoot");

				float bulletRotation = transform.localScale.x > 0 ? 0f : 180f;

				Projectile projectileInstance = Instantiate(projectile, gun.position, Quaternion.Euler(new Vector3(0, 0, bulletRotation))) as Projectile;
				projectileInstance.right = transform.localScale.x > 0;
				projectileInstance.left = !projectileInstance.right;

				cooldownTimer = 0f;
			}
		}

		GetMovement();
		ApplyMovement();
	}
}
