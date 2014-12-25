using UnityEngine;
using System.Collections;

public class AcceleratingShot : Projectile
{
	public float accelTime = 1f;
	public AnimationCurve accelCurve;

	private float originalShotSpeed;
	private float accelTimer = 0f;

	private float accelPercentage
	{
		get
		{
			return accelCurve.Evaluate(Mathf.Clamp(accelTimer / accelTime, 0f, 1f));
		}
	}

	protected override void Awake()
	{
		base.Awake();

		originalShotSpeed = shotSpeed;
	}

	void FixedUpdate()
	{
		InitialUpdate();

		if (accelPercentage < 1f)
		{
			accelTimer += Time.deltaTime;

			shotSpeed = originalShotSpeed * accelPercentage;
		}

		ApplyMovement();
	}
}
