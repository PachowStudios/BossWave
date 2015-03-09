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
		anim.SetBool("Walking", right || left);
	}

	protected override void Walk()
	{
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
	#endregion
}
