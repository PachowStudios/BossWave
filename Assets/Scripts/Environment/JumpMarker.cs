using UnityEngine;
using System.Collections;

public class JumpMarker : MonoBehaviour
{
	#region Fields
	[SerializeField]
	private JumpMarker jumpEndPoint;
	[SerializeField]
	[Range(0f, 10f)]
	private float fallJumpHeight;
	#endregion

	#region Public Properties
	public float JumpHeight
	{
		get
		{
			return (jumpEndPoint == null) ? 0f : Mathf.Max(0f, jumpEndPoint.transform.position.y - transform.position.y + 2f);
		}
	}

	public float FallHeight
	{
		get { return fallJumpHeight; }
	}

	public int Direction
	{
		get
		{
			return (jumpEndPoint == null) ? -1
										  : (transform.position.x > jumpEndPoint.transform.position.x) ? -1 : 1;
		}
	}
	#endregion

	#region MonoBehaviour
	private void OnDrawGizmosSelected()
	{
		if (jumpEndPoint != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(transform.position, jumpEndPoint.transform.position);
		}
	}
	#endregion
}
