using UnityEngine;
using System.Collections;

public class TargetPractice : Enemy
{
	protected override void FixedUpdate()
	{
		InitialUpdate();
	}

	protected override void ApplyAnimation()
	{ }

	protected override void Walk()
	{ }

	protected override void Jump(float height)
	{ }

	protected override void CheckAttack()
	{ }
}
