using UnityEngine;
using System.Collections;

public sealed class MeleeAttackAI : AttackAI
{
	#region Fields
	public float range = 5f;
	public float cooldownTime = 1f;

	private float cooldownTimer = 0f;
	#endregion

	#region Internal Update Methods
	public override void CheckAttack()
	{
		cooldownTimer += Time.deltaTime;

		if (cooldownTimer >= cooldownTime && IsPlayerInRange(0f, range))
		{
			Attack();
			cooldownTimer = 0f;
		}
	}
	#endregion

	#region Internal Helper Methods
	private void Attack()
	{
		anim.SetTrigger("Attack");
		PlayerControl.Instance.TakeDamage(gameObject);
	}
	#endregion
}
