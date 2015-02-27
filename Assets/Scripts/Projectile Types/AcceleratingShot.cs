using UnityEngine;
using System.Collections;

public class AcceleratingShot : Projectile
{
	#region Fields
	public float accelTime = 1f;
	public AnimationCurve accelCurve;
	public bool homingDuringAccel = false;
	[Range(0f, 1f)]
	public float homingThreshold = 0.5f;
	[Range(0f, 1f)]
	public float homingSpeed = 0.5f;
	public bool hasTrail = false;
	[Range(0f, 1f)]
	public float trailThreshold = 0f;

	private float originalShotSpeed;
	private float accelTimer = 0f;
	#endregion

	#region Internal Properties
	private float accelPercentage
	{
		get
		{
			return accelCurve.Evaluate(Mathf.Clamp(accelTimer / accelTime, 0f, 1f));
		}
	}
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		originalShotSpeed = shotSpeed;
	}

	private void Update()
	{
		if (accelPercentage < 1f)
		{
			accelTimer += Time.deltaTime;

			shotSpeed = originalShotSpeed * accelPercentage;

			if (homingDuringAccel && accelPercentage >= homingThreshold)
			{
				Vector2 playerDirection = transform.position.LookAt2D(PlayerControl.Instance.collider2D.bounds.center) * Vector3.right;
				direction = Vector3.Lerp(direction, playerDirection, homingSpeed * 60f * Time.deltaTime).normalized;
			}

			if (hasTrail && accelPercentage >= trailThreshold)
			{
				anim.SetBool("Trail", true);
			}
		}
	}

	private void LateUpdate()
	{
		DoMovement();
	}
	#endregion
}
