using UnityEngine;
using System.Collections;

public abstract class FollowAI : StandardEnemy
{
	public float followRange = 5f;
	public float attackRange = 5f;
	public float attackCooldownTime = 1f;

	protected float attackCooldownTimer = 0f;

	protected bool InAttackRange
	{
		get
		{
			return Mathf.Abs(PlayerControl.instance.transform.position.x - transform.position.x) <= attackRange;
		}
	}

	protected override void ApplyAnimation()
	{
		anim.SetBool("Walking", right || left);
	}

	protected override void Walk()
	{
		if (PlayerControl.instance.transform.position.x > transform.position.x + followRange)
		{
			right = true;
			left = !right;
		}
		else if (PlayerControl.instance.transform.position.x < transform.position.x - followRange)
		{
			left = true;
			right = !left;
		}
		else
		{
			right = left = false;
		}
	}

	protected override void Jump(float height)
	{
		velocity.y = Mathf.Sqrt(2f * height * -gravity);
	}

	protected override void CheckAttack()
	{
		attackCooldownTimer += Time.deltaTime;

		if (attackCooldownTimer >= attackCooldownTime && InAttackRange)
		{
			Attack("Attack");
			attackCooldownTimer = 0f;
		}
	}

	protected virtual void Attack(string triggerName)
	{
		anim.SetTrigger(triggerName);

		if ((PlayerControl.instance.transform.position.x > transform.position.x &&
				 transform.localScale.x > 0f) ||
				(PlayerControl.instance.transform.position.x < transform.position.x &&
				 transform.localScale.x < 0f))
		{
			Flip();
		}
	}
}
