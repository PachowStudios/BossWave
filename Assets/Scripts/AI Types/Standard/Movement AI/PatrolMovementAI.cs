using UnityEngine;
using System.Collections;

public sealed class PatrolMovementAI : StandardEnemy
{
	#region Fields
	public float followRange = 5f;
	#endregion

	#region Internal Update Methods
	protected override void ApplyAnimation()
	{
		anim.SetBool("Walking", horizontalMovement != 0f);
	}

	protected override void Walk()
	{
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
	#endregion
}
