using UnityEngine;
using System.Collections;

public abstract class FollowAI : StandardEnemy
{
	public float followRange = 5f;
	public float attackRange = 5f;
	public float attackCooldownTime = 1f;

	protected float attackCooldownTimer = 0f;

	protected override void ApplyAnimation()
	{
		anim.SetBool("Walking", right || left);
	}

	protected override void Walk()
	{
		if (IsGrounded)
		{
			if (PlayerControl.instance.Top < transform.position.y)
			{
				if (!right && !left)
				{
					right = (checkLedgeCollision) ? Random.value < 0.5f
												  : IsPlayerOnRightSide;
					left = !right;
				}
			}
			else
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
		}
		else
		{
			right = left = false;
			FacePlayer();
		}
	}

	protected override void CheckAttack()
	{
		attackCooldownTimer += Time.deltaTime;

		if (attackCooldownTimer >= attackCooldownTime && IsPlayerInRange(0f, attackRange))
		{
			Attack();
			attackCooldownTimer = 0f;
		}
	}

	protected virtual void Attack()
	{
		anim.SetTrigger("Attack");
		FacePlayer();
	}
}
