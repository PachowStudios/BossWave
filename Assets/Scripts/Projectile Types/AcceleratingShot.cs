using UnityEngine;
using System.Collections;

public class AcceleratingShot : Projectile
{
	#region Fields
	public float accelTime = 1f;
	public AnimationCurve accelCurve;
	public bool hasTrail = false;
	[Range(0f, 1f)]
	public float trailPercentage = 0f;

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

			if (accelPercentage >= trailPercentage)
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
