#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class CharacterController2D : MonoBehaviour
{
	#region Types
	private struct CharacterRaycastOrigins
	{
		public Vector3 topLeft;
		public Vector3 bottomRight;
		public Vector3 bottomLeft;
	}

	public class CharacterCollisionState2D
	{
		public bool right;
		public bool left;
		public bool above;
		public bool below;
		public bool becameGroundedThisFrame;
		public bool wasGroundedLastFrame;
		public bool movingDownSlope;
		public float slopeAngle;

		public bool HasCollision
		{ get { return below || right || left || above; } }

		public void Reset()
		{
			right = left = above = below = becameGroundedThisFrame = movingDownSlope = false;
			slopeAngle = 0f;
		}

		public override string ToString()
		{
			return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}",
								 right, left, above, below, movingDownSlope, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame);
		}
	}
	#endregion

	#region Events
	public event Action<RaycastHit2D> OnControllerCollidedEvent;
	#endregion

	#region Public Fields
	public LayerMask platformMask = 0;
	public LayerMask oneWayPlatformMask = 0;
	public float jumpingThreshold = 0.07f;
	[Range(2, 20)]
	public int totalHorizontalRays = 8;
	[Range(2, 20)]
	public int totalVerticalRays = 4;
	[Range(0.001f, 0.3f)]
	public float skinWidth = 0.02f;
	[Range(0, 90f)]
	public float slopeLimit = 30f;
	public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1.5f), new Keyframe(0, 1), new Keyframe(90, 0));
	#endregion

	#region Internal Fields
	private Vector3 velocity;
	private CharacterCollisionState2D collisionState = new CharacterCollisionState2D();

	private CharacterRaycastOrigins raycastOrigins;
	private RaycastHit2D raycastHit;
	private List<RaycastHit2D> raycastHitsThisFrame = new List<RaycastHit2D>(2);
	private float verticalDistanceBetweenRays;
	private float horizontalDistanceBetweenRays;
	private bool isGoingUpSlope = false;

	private new Transform transform;
	private BoxCollider2D boxCollider;

	private const float kSkinWidthFloatFudgeFactor = 0.001f;
	private const float SlopeLimitTangent = 3.732051f;
	#endregion

	#region Public Properties
	public Vector3 Velocity
	{ get { return velocity; } }

	public bool IsGrounded
	{ get { return collisionState.below; } }

	public bool WasGroundedLastFrame
	{ get { return collisionState.wasGroundedLastFrame; } }
	#endregion	

	#region Monobehaviour
	private void Awake()
	{
		// add our one-way platforms to our normal platform mask so that we can land on them from above
		platformMask |= oneWayPlatformMask;

		// cache some components
		transform = GetComponent<Transform>();
		boxCollider = GetComponent<BoxCollider2D>();

		// here, we trigger our properties that have setters with bodies
		RecalculateDistanceBetweenRays();
	}
	#endregion

	#region Public Methods
	public void Move(Vector3 deltaMovement)
	{
		// save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
		collisionState.wasGroundedLastFrame = collisionState.below;

		// clear our state
		collisionState.Reset();
		raycastHitsThisFrame.Clear();
		isGoingUpSlope = false;

		var desiredPosition = transform.position + deltaMovement;
		PrimeRaycastOrigins(desiredPosition, deltaMovement);

		// first, we check for a slope below us before moving
		// only check slopes if we are going down and grounded
		if (deltaMovement.y < 0 && collisionState.wasGroundedLastFrame)
			HandleVerticalSlope(ref deltaMovement);

		// now we check movement in the horizontal dir
		if (deltaMovement.x != 0)
			MoveHorizontally(ref deltaMovement);

		// next, check movement in the vertical dir
		if (deltaMovement.y != 0)
			MoveVertically(ref deltaMovement);

		transform.Translate(deltaMovement, Space.World);

		// only calculate velocity if we have a non-zero deltaTime
		if (Time.deltaTime > 0)
			velocity = deltaMovement / Time.deltaTime;

		// set our becameGrounded state based on the previous and current collision state
		if (!collisionState.wasGroundedLastFrame && collisionState.below)
			collisionState.becameGroundedThisFrame = true;

		// if we are going up a slope we artificially set a y velocity so we need to zero it out here
		if (isGoingUpSlope)
			velocity.y = 0;

		// send off the collision events if we have a listener
		if (OnControllerCollidedEvent != null)
			for (var i = 0; i < raycastHitsThisFrame.Count; i++)
				OnControllerCollidedEvent(raycastHitsThisFrame[i]);
	}

	public void WarpToGrounded()
	{
		do
		{
			Move(new Vector3(0, -1f, 0));
		} while (!IsGrounded);
	}

	public void RecalculateDistanceBetweenRays()
	{
		// figure out the distance between our rays in both directions
		// horizontal
		var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * skinWidth);
		verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

		// vertical
		var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * skinWidth);
		horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
	}
	#endregion

	#region Internal Movement Methods
	private void PrimeRaycastOrigins(Vector3 futurePosition, Vector3 deltaMovement)
	{
		// our raycasts need to be fired from the bounds inset by the skinWidth
		var modifiedBounds = boxCollider.bounds;
		modifiedBounds.Expand(-2f * skinWidth);

		raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
		raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
		raycastOrigins.bottomLeft = modifiedBounds.min;
	}

	private void MoveHorizontally(ref Vector3 deltaMovement)
	{
		var isGoingRight = deltaMovement.x > 0;
		var rayDistance = Mathf.Abs(deltaMovement.x) + skinWidth;
		var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
		var initialRayOrigin = isGoingRight ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

		for (var i = 0; i < totalHorizontalRays; i++)
		{
			var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * verticalDistanceBetweenRays);

			DrawRay(ray, rayDirection * rayDistance, Color.red);

			// if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
			// walk up sloped oneWayPlatforms
			if (i == 0 && collisionState.wasGroundedLastFrame)
				raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
			else
				raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask & ~oneWayPlatformMask);

			if (raycastHit)
			{
				// the bottom ray can hit slopes but no other ray can so we have special handling for those cases
				if (i == 0 && HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(raycastHit.normal, Vector2.up)))
				{
					raycastHitsThisFrame.Add(raycastHit);
					break;
				}

				// set our new deltaMovement and recalculate the rayDistance taking it into account
				deltaMovement.x = raycastHit.point.x - ray.x;
				rayDistance = Mathf.Abs(deltaMovement.x);

				// remember to remove the skinWidth from our deltaMovement
				if (isGoingRight)
				{
					deltaMovement.x -= skinWidth;
					collisionState.right = true;
				}
				else
				{
					deltaMovement.x += skinWidth;
					collisionState.left = true;
				}

				raycastHitsThisFrame.Add(raycastHit);

				// we add a small fudge factor for the float operations here. if our rayDistance is smaller
				// than the width + fudge bail out because we have a direct impact
				if (rayDistance < skinWidth + kSkinWidthFloatFudgeFactor)
					break;
			}
		}
	}

	private bool HandleHorizontalSlope(ref Vector3 deltaMovement, float angle)
	{
		// disregard 90 degree angles (walls)
		if (Mathf.RoundToInt(angle) == 90)
			return false;

		// if we can walk on slopes and our angle is small enough we need to move up
		if (angle < slopeLimit)
		{
			// we only need to adjust the deltaMovement if we are not jumping
			// TODO: this uses a magic number which isn't ideal!
			if (deltaMovement.y < jumpingThreshold)
			{
				// apply the slopeModifier to slow our movement up the slope
				var slopeModifier = slopeSpeedMultiplier.Evaluate(angle);
				deltaMovement.x *= slopeModifier;

				// we dont set collisions on the sides for this since a slope is not technically a side collision

				// smooth y movement when we climb. we make the y movement equivalent to the actual y location that corresponds
				// to our new x location using our good friend Pythagoras
				deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
				isGoingUpSlope = true;

				collisionState.below = true;
			}
		}
		else // too steep. get out of here
			deltaMovement.x = 0;

		return true;
	}

	private void MoveVertically(ref Vector3 deltaMovement)
	{
		var isGoingUp = deltaMovement.y > 0;
		var rayDistance = Mathf.Abs(deltaMovement.y) + skinWidth;
		var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
		var initialRayOrigin = isGoingUp ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;

		// apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
		initialRayOrigin.x += deltaMovement.x;

		// if we are moving up, we should ignore the layers in oneWayPlatformMask
		var mask = platformMask;
		if (isGoingUp && !collisionState.wasGroundedLastFrame)
			mask &= ~oneWayPlatformMask;

		for (var i = 0; i < totalVerticalRays; i++)
		{
			var ray = new Vector2(initialRayOrigin.x + i * horizontalDistanceBetweenRays, initialRayOrigin.y);

			DrawRay(ray, rayDirection * rayDistance, Color.red);
			raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);
			if (raycastHit)
			{
				// set our new deltaMovement and recalculate the rayDistance taking it into account
				deltaMovement.y = raycastHit.point.y - ray.y;
				rayDistance = Mathf.Abs(deltaMovement.y);

				// remember to remove the skinWidth from our deltaMovement
				if (isGoingUp)
				{
					deltaMovement.y -= skinWidth;
					collisionState.above = true;
				}
				else
				{
					deltaMovement.y += skinWidth;
					collisionState.below = true;
				}

				raycastHitsThisFrame.Add(raycastHit);

				// this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a situation
				// where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame due to residual velocity.
				if (!isGoingUp && deltaMovement.y > 0.00001f)
					isGoingUpSlope = true;

				// we add a small fudge factor for the float operations here. if our rayDistance is smaller
				// than the width + fudge bail out because we have a direct impact
				if (rayDistance < skinWidth + kSkinWidthFloatFudgeFactor)
					return;
			}
		}
	}

	private void HandleVerticalSlope(ref Vector3 deltaMovement)
	{
		// slope check from the center of our collider
		var centerOfCollider = (raycastOrigins.bottomLeft.x + raycastOrigins.bottomRight.x) * 0.5f;
		var rayDirection = -Vector2.up;

		// the ray distance is based on our slopeLimit
		var slopeCheckRayDistance = SlopeLimitTangent * (raycastOrigins.bottomRight.x - centerOfCollider);

		var slopeRay = new Vector2(centerOfCollider, raycastOrigins.bottomLeft.y);
		DrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);
		raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, platformMask);

		if (raycastHit)
		{
			// bail out if we have no slope
			var angle = Vector2.Angle(raycastHit.normal, Vector2.up);

			if (angle == 0)
				return;

			// we are moving down the slope if our normal and movement direction are in the same x direction
			var isMovingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);

			if (isMovingDownSlope)
			{
				// going down we want to speed up in most cases so the slopeSpeedMultiplier curve should be > 1 for negative angles
				var slopeModifier = slopeSpeedMultiplier.Evaluate(-angle);
				deltaMovement.y = raycastHit.point.y - slopeRay.y - skinWidth;
				deltaMovement.x *= slopeModifier;
				collisionState.movingDownSlope = true;
				collisionState.slopeAngle = angle;
			}
		}
	}
	#endregion

	#region Internal Debug Methods
	[System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
	private void DrawRay(Vector3 start, Vector3 dir, Color color)
	{
		Debug.DrawRay(start, dir, color);
	}
	#endregion
}