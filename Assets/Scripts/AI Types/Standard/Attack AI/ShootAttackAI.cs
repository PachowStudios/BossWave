using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class ShootAttackAI : AttackAI
{
	#region Fields
	public Projectile projectile;
	public float range = 5f;
	public float maxRangeOffset = 1f;
	public bool horizontalShot = true;
	public bool useRandomGun = false;
	public bool burstShot = false;
	public int shotsPerBurst = 1;
	public float delayBetweenShots = 0.1f;
	public float attackDelay = 0f;
	public float cooldownTime = 1f;

	private List<Transform> guns;
	private int currentGun = 0;
	private float cooldownTimer = 0f;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		guns = transform.FindChild("FirePoints").FindChildTransforms();

		range += Random.Range(-maxRangeOffset, maxRangeOffset);
	}
	#endregion

	#region Internal Update Methods
	public override void CheckAttack()
	{
		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime &&
			((horizontalShot && IsPlayerInRange(0f, range)) ||
			(!horizontalShot && IsPlayerVisible(range))))
		{
			StartCoroutine(Attack());
			cooldownTimer = 0f;
		}
	}
	#endregion

	#region Internal Helper Methods
	private IEnumerator Attack()
	{
		anim.SetTrigger("Attack");

		if (attackDelay > 0f)
		{
			yield return new WaitForSeconds(attackDelay);
		}

		if (useRandomGun)
		{
			guns.Shuffle();
		}

		if (burstShot)
		{
			for (int i = 0; i < shotsPerBurst; i++)
			{
				StartCoroutine(Fire(delayBetweenShots * i));
			}

			if (attackDelay > 0f)
			{
				yield return new WaitForSeconds(delayBetweenShots * shotsPerBurst);

				anim.SetTrigger("End Attack");
			}
		}
		else
		{
			StartCoroutine(Fire(attackDelay));
		}

		yield return null;
	}

	private IEnumerator Fire(float delay = 0f)
	{
		if (delay > 0)
		{
			yield return new WaitForSeconds(delay);
		}

		Vector3 shotDirection = Vector3.zero;

		if (horizontalShot)
		{
			shotDirection = thisEnemy.FacingRight ? new Vector3(1f, 0f) 
												  : new Vector3(-1f, 0f);
		}
		else
		{
			guns[currentGun].LookAt2D(PlayerControl.Instance.collider2D.bounds.center, true);
			shotDirection = guns[currentGun].localRotation * Vector3.right;
		}

		Projectile projectileInstance = Instantiate(projectile, guns[currentGun].position, Quaternion.identity) as Projectile;
		projectileInstance.Initialize(shotDirection);

		currentGun = (currentGun + 1 >= guns.Count) ? 0 : currentGun + 1;
		cooldownTimer = 0f;

		yield return null;
	}
	#endregion
}
