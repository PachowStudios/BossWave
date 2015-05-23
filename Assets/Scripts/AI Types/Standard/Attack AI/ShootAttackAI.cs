using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class ShootAttackAI : AttackAI
{
	#region Fields
	public List<EnemyGun> guns;
	public float range = 5f;
	public float maxRangeOffset = 1f;
	public bool useLineOfSight = false;
	public bool useRandomGun = false;
	public bool burstShot = false;
	public int shotsPerBurst = 1;
	public float delayBetweenShots = 0.1f;
	public float attackDelay = 0f;
	public float cooldownTime = 1f;

	private int currentGun = 0;
	private float cooldownTimer = 0f;
	#endregion

	#region Initialization Methods
	public override void Initialize(StandardEnemy thisEnemy, Animator anim)
	{
		base.Initialize(thisEnemy, anim);

		range += Random.Range(-maxRangeOffset, maxRangeOffset);
	}
	#endregion

	#region Internal Update Methods
	public override void CheckAttack()
	{
		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime &&
			((!useLineOfSight && IsPlayerInRange(0f, range)) ||
			(useLineOfSight && IsPlayerVisible(range))))
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
			yield return new WaitForSeconds(attackDelay);

		if (useRandomGun)
			guns.Shuffle();

		if (burstShot)
		{
			for (int i = 0; i < shotsPerBurst; i++)
				StartCoroutine(Fire(delayBetweenShots * i));

			if (attackDelay > 0f)
			{
				yield return new WaitForSeconds(delayBetweenShots * shotsPerBurst);

				anim.SetTrigger("End Attack");
			}
		}
		else
			StartCoroutine(Fire(attackDelay));

		yield return null;
	}

	private IEnumerator Fire(float delay = 0f)
	{
		if (delay > 0)
			yield return new WaitForSeconds(delay);

		guns[currentGun].Fire();
		currentGun = (currentGun + 1 >= guns.Count) ? 0 : currentGun + 1;
		cooldownTimer = 0f;

		yield return null;
	}
	#endregion
}
