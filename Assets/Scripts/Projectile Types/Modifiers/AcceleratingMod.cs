using UnityEngine;
using System.Collections;

public sealed class AcceleratingMod : ProjectileMod
{
	#region Fields
	public float accelTime = 1f;
	public AnimationCurve accelCurve;
	public bool hasTrail = false;
	[Range(0f, 1f)]
	public float trailThreshold = 0f;

	private float originalSpeed;
	private float accelTimer = 0f;

	private Animator anim;
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

		anim = GetComponent<Animator>();
		originalSpeed = thisProjectile.shotSpeed;
	}
	#endregion

	#region PublicMethods
	public override void ApplyModifier()
	{
		if (accelPercentage < 1f)
		{
			accelTimer += Time.deltaTime;
			thisProjectile.shotSpeed = originalSpeed * accelPercentage;

			if (hasTrail && accelPercentage >= trailThreshold)
			{
				anim.SetBool("Trail", true);
			}
		}
	}
	#endregion
}
