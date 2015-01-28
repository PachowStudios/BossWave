using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(GhostTrailEffect))]
public sealed class DashAI : StandardEnemy
{
	public float swipeDamage = 5f;
	public float stabDamage = 10f;
	public Vector2 swipeKnockback = new Vector2(3f, 5f);
	public Vector2 stabKnockback = new Vector2(10f, 1f);
	public float swipeRange = 2f;
	public float minDashRange = 5f;
	public float maxDashRange = 10f;
	public float swipeCooldownTime = 0.5f;
	public float dashCooldownTime = 5f;
	public float dashSpeed = 25f;
	public float stabLegnth = 1f;

	private bool dashing = false;
	private float swipeCooldownTimer = 0f;
	private float dashCooldownTimer = 0f;
	private float defaultMoveSpeed;
	private float dashTarget;

	private GhostTrailEffect ghostTrail;

	protected override void Awake()
	{
		base.Awake();

		defaultMoveSpeed = moveSpeed;
		frontCheck = transform.FindChild("frontCheck");

		ghostTrail = GetComponent<GhostTrailEffect>();
	}

	protected override void ApplyAnimation()
	{
		anim.SetBool("Walking", right || left);
		anim.SetBool("Running", dashing);
	}

	protected override void Walk()
	{
		if (!dashing)
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
				if (PlayerControl.instance.transform.position.x > transform.position.x + swipeRange)
				{
					right = true;
					left = !right;
				}
				else if (PlayerControl.instance.transform.position.x < transform.position.x - swipeRange)
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
	}

	protected override void CheckAttack()
	{
		if (!dashing)
		{
			swipeCooldownTimer += Time.deltaTime;

			if (swipeCooldownTimer >= swipeCooldownTime && IsPlayerInRange(0f, swipeRange))
			{
				anim.SetTrigger("Swipe");
				PlayerControl.instance.TakeDamage(gameObject, swipeDamage, swipeKnockback);
				swipeCooldownTimer = 0f;
			}

			dashCooldownTimer += Time.deltaTime;

			if (dashCooldownTimer >= dashCooldownTime && IsPlayerInRange(minDashRange, maxDashRange))
			{
				dashing = true;
				ghostTrail.trailActive = true;
				moveSpeed = dashSpeed;
				dashTarget = transform.position.x + (maxDashRange * (FacingRight ? 1 : -1));
				dashCooldownTimer = 0f;
			}
		}
		else if (!disableMovement)
		{
			if (CheckLedgeCollision(false) || CheckFrontCollision(false) ||
				(left && transform.position.x < dashTarget) ||
				(right && transform.position.x > dashTarget))
			{
				Stab(false);
			}
			else if (IsPlayerInRange(0f, swipeRange))
			{
				Stab(true);
			}
		}
	}

	private void Stab(bool damagePlayer)
	{
		disableMovement = true;
		moveSpeed = defaultMoveSpeed;
		anim.SetTrigger("Stab");

		if (damagePlayer)
		{
			PlayerControl.instance.TakeDamage(gameObject, stabDamage, stabKnockback);
		}

		Sequence sequence = DOTween.Sequence();

		sequence
			.AppendInterval(stabLegnth)
			.AppendCallback(() => disableMovement = dashing = ghostTrail.trailActive = false);
	}
}
