using UnityEngine;
using System.Collections;

public class JumpMarker : MonoBehaviour
{
	#region Fields
	[SerializeField]
	private JumpMarker jumpEndPoint;
	[SerializeField]
	private JumpMarker gapEndPoint;
	[SerializeField]
	private JumpMarker fallEndPoint;
	#endregion

	#region Public Properties
	public int Direction
	{
		get
		{
			if (jumpEndPoint != null)
			{
				return jumpEndPoint.transform.position.x > transform.position.x ? 1 : -1;
			}
			else if (gapEndPoint != null)
			{
				return gapEndPoint.transform.position.x > transform.position.x ? 1 : -1;
			}
			else if (fallEndPoint != null)
			{
				return fallEndPoint.transform.position.x > transform.position.x ? 1 : -1;
			}
			else
			{
				return 1;
			}
		}
	}

	public bool HasFallPoint
	{
		get { return fallEndPoint != null; }
	}
	#endregion

	#region MonoBehaviour
	private void OnDrawGizmosSelected()
	{
		if (jumpEndPoint != null)
		{
			Gizmos.color = (jumpEndPoint.fallEndPoint == this) ? Color.green : Color.blue;
			Gizmos.DrawLine(transform.position, jumpEndPoint.transform.position);
		}

		if (gapEndPoint != null)
		{
			Gizmos.color = (gapEndPoint.gapEndPoint == this) ? Color.green : Color.yellow;
			Gizmos.DrawLine(transform.position, gapEndPoint.transform.position);
		}

		if (fallEndPoint != null)
		{
			Gizmos.color = (fallEndPoint.jumpEndPoint == this) ? Color.green : Color.red;
			Gizmos.DrawLine(transform.position, fallEndPoint.transform.position);
		}
	}
	#endregion

	#region Private Helper Methods
	private float CalculateJumpHeight(Transform target, float moveSpeed, float gravity)
	{
		gravity = Mathf.Abs(gravity);

		if (moveSpeed <= 0f || gravity <= 0f)
		{
			return 0f;
		}

		float platformHeight = target.position.y - transform.position.y;
		float distanceTime = Mathf.Abs(target.position.x - transform.position.x) / moveSpeed;
		float apexTime = Mathf.Sqrt(2f * Mathf.Max(0f, platformHeight) * gravity) / gravity;
		float fallGravity = (gravity * Mathf.Max(0f, distanceTime - apexTime));

		return Mathf.Max(0f, (Mathf.Pow(fallGravity, 2f) / gravity / 4f) + platformHeight + 1f);
	}
	#endregion

	#region Public Methods
	public float CalculateJumpJumpHeight(float moveSpeed, float gravity)
	{
		return (jumpEndPoint == null) ? CalculateFallJumpHeight(moveSpeed, gravity) 
									  : CalculateJumpHeight(jumpEndPoint.transform, moveSpeed, gravity);
	}

	public float CalculateGapJumpHeight(float moveSpeed, float gravity)
	{
		return (gapEndPoint == null) ? 0f : CalculateJumpHeight(gapEndPoint.transform, moveSpeed, gravity);
	}

	public float CalculateFallJumpHeight(float moveSpeed, float gravity)
	{
		return (fallEndPoint == null) ? 0f : CalculateJumpHeight(fallEndPoint.transform, moveSpeed, gravity);
	}
	#endregion
}
