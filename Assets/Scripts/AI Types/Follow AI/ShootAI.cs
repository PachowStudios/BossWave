using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShootAI : FollowAI
{
	public float maxRangeOffset = 1f;
	public bool horizontalShot = true;
	public Projectile projectile;

	private List<Transform> guns;
	private int currentGun = 0;

	new void Awake()
	{
		base.Awake();

		guns = transform.FindChild("FirePoints").FindChildTransforms();

		float rangeOffset = Random.Range(-maxRangeOffset, maxRangeOffset);
		followRange += rangeOffset;
		attackRange += rangeOffset;
	}

	protected override void Attack()
	{
		base.Attack();

		guns[currentGun].LookAt2D(PlayerControl.instance.collider2D.bounds.center, true);

		Vector3 shotDirection = Vector3.zero;

		if (horizontalShot)
		{
			if (PlayerControl.instance.transform.position.x < transform.position.x)
			{
				shotDirection = new Vector3(-1f, 0f, 0f);
			}
			else
			{
				shotDirection = new Vector3(1f, 0f, 0f);
			}
		}
		else
		{
			shotDirection = guns[currentGun].localRotation * Vector3.right;
		}

		Projectile projectileInstance = Instantiate(projectile, guns[currentGun].position, Quaternion.identity) as Projectile;
		projectileInstance.Initialize(shotDirection);

		currentGun = (currentGun + 1 == guns.Count) ? 0 : currentGun + 1;
	}
}
