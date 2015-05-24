using UnityEngine;
using System.Collections;

public sealed class AcceleratingMod : ProjectileMod
{
	#region Fields
	public float accelTime = 0f;
	public float accelLength = 1f;
	public AnimationCurve accelCurve;
	public bool hasTrail = false;
	public float trailTime = 0f;

	private float originalSpeed;
	private float accelTimer = 0f;

	private Animator anim;
	#endregion

	#region Internal Properties
	private float AccelPercentage
	{ get { return accelCurve.Evaluate(Mathf.Clamp((accelTimer - accelTime) / accelLength, 0f, 1f)); } }
	#endregion

	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		anim = GetComponent<Animator>();
		originalSpeed = thisProjectile.shotSpeed;
		thisProjectile.shotSpeed = originalSpeed * accelCurve.Evaluate(0f);
	}
	#endregion

	#region PublicMethods
	public override void ApplyModifier()
	{
		accelTimer += Time.deltaTime;

		if (accelTimer >= accelTime && AccelPercentage < 1f)
			thisProjectile.shotSpeed = originalSpeed * AccelPercentage;

		if (hasTrail && accelTimer >= trailTime)
			anim.SetBool("Trail", true);
	}
	#endregion
}
