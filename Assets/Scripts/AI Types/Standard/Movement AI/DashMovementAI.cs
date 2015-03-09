using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DashAttackAI))]
public sealed class DashMovementAI : StandardEnemy
{
	#region Fields
	public float followRange = 5f;
	public float dashSpeed = 25f;

	private float defaultMoveSpeed;

	private DashAttackAI dashAttackAI;
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		defaultMoveSpeed = moveSpeed;

		dashAttackAI = GetComponent<DashAttackAI>();
	}
	#endregion

	#region Internal Update Methods
	protected override void ApplyAnimation()
	{
		anim.SetBool("Walking", right || left);
		anim.SetBool("Running", dashAttackAI.Dashing);
	}

	protected override void Walk()
	{
		if (!dashAttackAI.Dashing && !dashAttackAI.Stabbing)
		{
			moveSpeed = defaultMoveSpeed;

			if (RelativePlayerLastGrounded != 0f)
			{
				if (!WasGroundedLastFrame)
				{
					right = Random.value < 0.5f;
					left = !right;
				}

				CheckFrontCollision(true);
				CheckLedgeCollision(true);
			}
			else if (CheckLedgeCollision())
			{
				if (RelativePlayerHeight < 0.5f)
				{
					FollowPlayer(followRange);
				}
			}
			else if (PlayerControl.Instance.IsGrounded)
			{
				CheckLedgeCollision(true);
			}
			else
			{
				right = left = false;
			}
		}
		else if (dashAttackAI.Dashing)
		{
			moveSpeed = dashSpeed;
		}
		else
		{
			moveSpeed = 0f;
		}
	}
	#endregion
}
