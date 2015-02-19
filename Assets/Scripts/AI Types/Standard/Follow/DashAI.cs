using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(GhostTrailEffect))]
public sealed class DashAI : FollowAI
{
	#region Fields
	public float minDashRange = 5f;
	public float maxDashRange = 10f;
	public float swipeDamage = 5f;
	public float stabDamage = 10f;
	public Vector2 swipeKnockback = new Vector2(3f, 5f);
	public Vector2 stabKnockback = new Vector2(10f, 1f);
	public float dashCooldownTime = 5f;
	public float dashSpeed = 25f;
	public float stabLegnth = 1f;

	private bool dashing = false;
	private float dashCooldownTimer = 0f;
	private float defaultMoveSpeed;
	private float dashTarget;

	private GhostTrailEffect ghostTrail;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		defaultMoveSpeed = moveSpeed;
		frontCheck = transform.FindChild("frontCheck");

		ghostTrail = GetComponent<GhostTrailEffect>();
	}
	#endregion

	#region Internal Update Methods
	protected override void ApplyAnimation()
	{
		base.ApplyAnimation();

		anim.SetBool("Running", dashing);
	}

	protected override void Walk()
	{
		if (!dashing)
		{
			base.Walk();
		}
	}

	protected override void CheckAttack()
	{
		if (!dashing)
		{
			attackCooldownTimer += Time.deltaTime;

			if (attackCooldownTimer >= attackCooldownTime && 
				IsPlayerInRange(0f, attackRange))
			{
				Attack();
				attackCooldownTimer = 0f;
			}

			dashCooldownTimer += Time.deltaTime;

			if (dashCooldownTimer >= dashCooldownTime && 
				PlayerControl.Instance.IsGrounded && 
				IsPlayerInRange(minDashRange, maxDashRange))
			{
				StartDash();
				dashCooldownTimer = 0f;
			}
		}
		else if (!disableMovement)
		{
			if (IsPlayerInRange(0f, attackRange) ||
				!CheckLedgeCollision() || CheckFrontCollision() ||
				(left && transform.position.x < dashTarget) ||
				(right && transform.position.x > dashTarget))
			{
				EndDash();
			}
		}
	}
	#endregion

	#region Internal Helper Methods
	protected override void Attack()
	{
		anim.SetTrigger("Swipe");
		PlayerControl.Instance.TakeDamage(gameObject, swipeDamage, swipeKnockback);
	}

	private void StartDash()
	{
		dashing = true;
		ghostTrail.trailActive = true;
		moveSpeed = dashSpeed;
		dashTarget = transform.position.x + (maxDashRange * (FacingRight ? 1 : -1));
	}

	private void EndDash()
	{
		disableMovement = true;
		moveSpeed = defaultMoveSpeed;
		anim.SetTrigger("Stab");

		if (IsPlayerInRange(0f, attackRange))
		{
			PlayerControl.Instance.TakeDamage(gameObject, stabDamage, stabKnockback);
		}

		Sequence sequence = DOTween.Sequence();

		sequence
			.AppendInterval(stabLegnth)
			.AppendCallback(() => disableMovement = dashing = ghostTrail.trailActive = false);
	}
	#endregion
}
