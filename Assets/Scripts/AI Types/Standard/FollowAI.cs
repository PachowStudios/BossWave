using UnityEngine;
using System.Collections;

public abstract class FollowAI : StandardEnemy
{
	#region Fields
	public float followRange = 5f;
	public float attackRange = 5f;
	public float attackCooldownTime = 1f;

	protected float attackCooldownTimer = 0f;
	#endregion

	#region MonoBehaviour
	protected override void OnTriggerEnter2D(Collider2D other)
	{
		base.OnTriggerEnter2D(other);

		if (other.tag == "JumpMarker")
		{
			if (IsGrounded)
			{
				JumpMarker jumpInfo = other.GetComponent<JumpMarker>();

				if (RelativePlayerLastGrounded != 0f)
				{
					right = (jumpInfo.Direction > 0);
					left = !right;

					Jump((RelativePlayerLastGrounded < 0f) ? jumpInfo.JumpHeight : jumpInfo.FallHeight);
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
		anim.SetBool("Walking", right || left);
	}

	protected override void Walk()
	{
		if (IsGrounded)
		{
			if (RelativePlayerLastGrounded != 0f)
			{
				if (!WasGroundedLastFrame && IsGrounded)
				{
					right = IsPlayerOnRightSide;
					left = !right;
				}

				CheckFrontCollision(true);
			}
			else
			{
				if (CheckLedgeCollision())
				{
					if (RelativePlayerHeight < 0.5f)
					{
						FollowPlayer();
					}
				}
				else
				{
					if (PlayerControl.Instance.IsGrounded)
					{
						CheckLedgeCollision(true);
					}
					else
					{
						right = left = false;
					}
				}
			}
		}
	}

	protected override void CheckAttack()
	{
		attackCooldownTimer += Time.deltaTime;

		if (attackCooldownTimer >= attackCooldownTime && IsPlayerInRange(0f, attackRange))
		{
			Attack();
			attackCooldownTimer = 0f;
		}
	}
	#endregion

	#region Internal Helper Methods
	protected virtual void FollowPlayer()
	{
		if (transform.position.x + followRange < PlayerControl.Instance.transform.position.x)
		{
			right = true;
			left = !right;
		}
		else if (transform.position.x - followRange > PlayerControl.Instance.transform.position.x)
		{
			left = true;
			right = !left;
		}
		else
		{
			right = left = false;
			FacePlayer();
		}
	}

	protected virtual void Attack()
	{
		anim.SetTrigger("Attack");
	}
	#endregion
}
