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
		anim.SetBool("Walking", horizontalMovement != 0f);
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
					horizontalMovement = Extensions.RandomSign();

				CheckAtWall(true);
				CheckAtLedge(true);
			}
			else if (!CheckAtLedge())
			{
				if (RelativePlayerHeight < 0.5f)
				{
					FollowPlayer(followRange);
				}
			}
			else if (PlayerControl.Instance.IsGrounded)
			{
				CheckAtLedge(true);
			}
			else
			{
				horizontalMovement = 0f;
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
