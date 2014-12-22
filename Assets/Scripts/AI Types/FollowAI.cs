using UnityEngine;
using System.Collections;

public class FollowAI : Enemy
{
	public float followRange = 5f;
	public float attackRange = 5f;
	public float attackCooldownTime = 1f;

	private float attackCooldownTimer = 0f;

	new void Awake()
	{
		base.Awake();
	}

	void FixedUpdate()
	{
		InitialUpdate();

		anim.SetBool("Walking", right || left);

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

		attackCooldownTimer += Time.deltaTime;

		if (attackCooldownTimer >= attackCooldownTime &&
			Mathf.Abs(PlayerControl.instance.transform.position.x - transform.position.x) <= attackRange)
		{
			Attack();
			attackCooldownTimer = 0f;
		}

		GetMovement();
		ApplyMovement();
	}

	protected virtual void Attack()
	{
		anim.SetTrigger("Attack");

		if ((PlayerControl.instance.transform.position.x > transform.position.x &&
				 transform.localScale.x > 0f) ||
				(PlayerControl.instance.transform.position.x < transform.position.x &&
				 transform.localScale.x < 0f))
		{
			Flip();
		}
	}
}
