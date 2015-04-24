using UnityEngine;
using System.Collections;

public class FollowMovementAI : StandardEnemy
{
	#region Fields
	public float followRange = 5f;
	#endregion

	#region MonoBehaviour
	protected override void OnTriggerEnter2D(Collider2D other)
	{
		base.OnTriggerEnter2D(other);

		if (other.tag == "JumpMarker")
		{
			if (IsGrounded)
			{
				JumpMarker jumpMarker = other.GetComponent<JumpMarker>();
				
				if (horizontalMovement == jumpMarker.Direction)
				{
					float jumpHeight = 0f;

					if (RelativePlayerHeight < 0f)
					{
						jumpHeight = jumpMarker.CalculateJumpJumpHeight(moveSpeed, gravity);
					}
					else if (RelativePlayerHeight > 0f)
					{
						if (jumpMarker.HasFallPoint)
						{
							jumpHeight = jumpMarker.CalculateFallJumpHeight(moveSpeed, gravity);
						}
						else
						{
							horizontalMovement *= -1f;

							if (horizontalMovement == 0f)
								horizontalMovement = 1f;
						}
					}
					else
					{
						jumpHeight = jumpMarker.CalculateGapJumpHeight(moveSpeed, gravity);
					}

					Jump(jumpHeight);
				}
			}
		}
	}

	protected virtual void OnTriggerStay2D(Collider2D other)
	{
		if (other.tag == "JumpMarker")
		{
			OnTriggerEnter2D(other);
		}
	}
	#endregion

	#region Internal Update Methods
	protected override void ApplyAnimation()
	{
		anim.SetBool("Walking", horizontalMovement != 0f);
	}

	protected override void Walk()
	{
		if (IsGrounded)
		{
			if (RelativePlayerLastGrounded != 0f)
			{
				if (!WasGroundedLastFrame)
					horizontalMovement = Extensions.RandomSign();

				CheckAtWall(true);
			}
			else if (RelativePlayerHeight < 0.5f)
			{
				FollowPlayer(followRange);
			}
		}
	}
	#endregion
}
