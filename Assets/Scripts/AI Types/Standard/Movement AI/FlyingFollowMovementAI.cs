using UnityEngine;
using System.Collections;
using DG.Tweening;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public sealed class FlyingFollowMovementAI : StandardEnemy
{
	#region Fields
	public float viewRange = 20f;
	public float flyHeight = 5f;
	public float maxFlyHeightOffset = 1f;
	public float maxHorizontalTargetOffset = 2f;
	public float hoverSpeed = 1f;
	public float nextWaypointDistance = 0.25f;
	public float repathTime = 0.25f;

	private float flyHeightOffset;
	private float horizontalTargetOffset;
	private float repathTimer;
	private int currentWaypoint = 0;

	private Seeker seeker;
	private Path path;
	#endregion

	#region Internal Properties
	private Vector3 NewPlayerTargetPosition
	{
		get
		{
			Vector3 target = new Vector3(PlayerControl.Instance.transform.position.x + horizontalTargetOffset,
										 PlayerControl.Instance.LastGroundedPosition.y);

			RaycastHit2D raycast = Physics2D.Raycast(target, Vector2.up, flyHeight + flyHeightOffset, controller.platformMask);

			if (raycast.collider != null)
			{
				target = raycast.point;
				target.y -= 1f;
			}
			else
			{
				target.y += flyHeight + flyHeightOffset;
			}

			return target;
		}
	}
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		horizontalTargetOffset = Random.Range(-maxHorizontalTargetOffset, maxHorizontalTargetOffset);
		repathTimer = repathTime;

		seeker = GetComponent<Seeker>();
		seeker.pathCallback += OnPathCalculated;
	}

	protected override void Start()
	{
		DOTween.To(x => flyHeightOffset = x, -maxFlyHeightOffset, maxFlyHeightOffset, hoverSpeed)
			.SetSpeedBased(true)
			.SetLoops(-1, LoopType.Yoyo)
			.SetEase(Ease.InOutSine);
	}

	protected override void LateUpdate()
	{
		if (path != null && currentWaypoint < path.vectorPath.Count)
		{
			Vector3 direction = (path.vectorPath[currentWaypoint] - transform.position).normalized;
			velocity = Vector3.Lerp(velocity, direction * moveSpeed, inAirDamping * Time.deltaTime);
			controller.move(velocity * Time.deltaTime);
			velocity = controller.velocity;

			if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
			{
				currentWaypoint++;
			}
		}
	}
	#endregion

	#region Internal Update Methods
	protected override void ApplyAnimation()
	{
		
	}

	protected override void Walk()
	{
		repathTimer += Time.deltaTime;

		if (repathTimer >= repathTime)
		{
			seeker.StartPath(transform.position, NewPlayerTargetPosition);
			repathTimer = 0f;
		}
	}
	#endregion

	#region Internal Helper Methods
	private void OnPathCalculated(Path newPath)
	{
		newPath.Claim(this);

		if (!newPath.error)
		{
			if (path != null)
			{
				path.Release(this);
			}

			path = newPath;
			currentWaypoint = 0;
		}
		else
		{
			newPath.Release(this);
		}
	}
	#endregion
}
