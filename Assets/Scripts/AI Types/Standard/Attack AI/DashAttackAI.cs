using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DashMovementAI), typeof(GhostTrailEffect))]
public sealed class DashAttackAI : AttackAI
{
	#region Fields
	public float swipeRange = 5f;
	public float minDashRange = 5f;
	public float maxDashRange = 10f;
	public float swipeDamage = 5f;
	public float dashDamage = 10f;
	public Vector2 swipeKnockback = new Vector2(3f, 5f);
	public Vector2 dashKnockback = new Vector2(10f, 1f);
	public float swipeCooldownTime = 1f;
	public float dashCooldownTime = 5f;
	public float stabDuration = 1f;

	private float swipeCooldownTimer = 0f;
	private float dashCooldownTimer = 0f;
	private float dashTarget;

	private DashMovementAI dashMovementAI;
	private GhostTrailEffect ghostTrail;
	#endregion

	#region Public Properties
	public bool Dashing { get; private set; }
	public bool Stabbing { get; private set; }
	#endregion

	#region Initialization Methods
	public override void Initialize(StandardEnemy thisEnemy, Animator anim)
	{
		base.Initialize(thisEnemy, anim);

		dashMovementAI = GetComponent<DashMovementAI>();
		ghostTrail = GetComponent<GhostTrailEffect>();
	}
	#endregion

	#region Internal Update Methods
	public override void CheckAttack()
	{
		if (!Dashing && !Stabbing)
		{
			swipeCooldownTimer += Time.deltaTime;

			if (swipeCooldownTimer >= swipeCooldownTime &&
				IsPlayerInRange(0f, swipeRange))
			{
				Swipe();
				swipeCooldownTimer = 0f;
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
		else if (!Stabbing)
		{
			if (IsPlayerInRange(0f, swipeRange) ||
				!dashMovementAI.CheckLedgeCollision() || dashMovementAI.CheckFrontCollision() ||
				(!dashMovementAI.FacingRight && transform.position.x < dashTarget) ||
				(dashMovementAI.FacingRight && transform.position.x > dashTarget))
			{
				StartCoroutine(EndDash());
			}
		}
	}
	#endregion

	#region Internal Helper Methods
	private void Swipe()
	{
		anim.SetTrigger("Swipe");
		PlayerControl.Instance.TakeDamage(gameObject, swipeDamage, swipeKnockback);
	}

	private void StartDash()
	{
		Dashing = true;
		ghostTrail.trailActive = true;
		dashTarget = transform.position.x + (maxDashRange * (dashMovementAI.FacingRight ? 1 : -1));
	}

	private IEnumerator EndDash()
	{
		Dashing = false;
		Stabbing = true;
		anim.SetTrigger("Stab");

		if (IsPlayerInRange(0f, swipeRange))
		{
			PlayerControl.Instance.TakeDamage(gameObject, dashDamage, dashKnockback);
		}

		yield return new WaitForSeconds(stabDuration);

		Stabbing = ghostTrail.trailActive = false;
	}
	#endregion
}
