using UnityEngine;
using System.Collections;

public class BladeMasterAI : FollowAI
{
	public float minDashRange = 5f;
	public float maxDashRange = 10f;
	public float dashCooldownTime = 5f;
	public float dashSpeed = 25f;

	private bool dashing = false;
	private float dashCooldownTimer = 0f;
	private float defaultMoveSpeed;

	protected override void Awake()
	{
		base.Awake();

		defaultMoveSpeed = moveSpeed;
	}

	protected override void ApplyAnimation()
	{
		base.ApplyAnimation();

		anim.SetBool("Running", dashing);
	}

	protected override void Walk()
	{
		base.Walk();

		dashCooldownTimer += Time.deltaTime;

		if (dashCooldownTimer >= dashCooldownTime &&
			Mathf.Abs(PlayerControl.instance.transform.position.x - transform.position.x) >= minDashRange &&
			Mathf.Abs(PlayerControl.instance.transform.position.x - transform.position.x) <= maxDashRange)
		{
			dashing = true;
			dashCooldownTimer = 0f;
		}

		moveSpeed = dashing ? dashSpeed : defaultMoveSpeed;
		
		if (InAttackRange)
		{
			right = left = false;
		}
	}

	protected override void CheckAttack()
	{
		attackCooldownTimer += Time.deltaTime;

		if (InAttackRange)
		{
			if (dashing)
			{
				Attack("Stab");
				dashing = false;
				attackCooldownTimer = -1f;
			}
			else if (attackCooldownTimer >= attackCooldownTime)
			{
				Attack("Swipe");
				attackCooldownTimer = 0f;
			}
		}
	}

	protected override void Attack(string triggerName)
	{
		base.Attack(triggerName);

		PlayerControl.instance.TakeDamage(gameObject);
	}
}
