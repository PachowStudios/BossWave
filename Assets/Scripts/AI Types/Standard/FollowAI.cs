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
			if ((int)lastGroundedPosition.y > (int)PlayerControl.instance.LastGroundedPosition.y)
			{
				if (!right && !left)
				{
					right = Random.value < 0.5f;
					left = !right;
				}

				CheckFrontCollision(true);
				CheckLedgeCollision(true);
			}
			else
			{
				if (CheckLedgeCollision())
				{
					if (transform.position.x + followRange < PlayerControl.instance.transform.position.x)
					{
						right = true;
						left = !right;
					}
					else if (transform.position.x - followRange > PlayerControl.instance.transform.position.x)
					{
						left = true;
						right = !left;
					}
					else
					{
						right = left = false;
					}
				}
				else
				{
					if (PlayerControl.instance.IsGrounded)
					{
						CheckLedgeCollision(true);
					}
					else
					{
						right = left = false;
					}
				}
			}
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
