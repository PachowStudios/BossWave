using UnityEngine;
using System.Collections;

public sealed class MeleeAI : FollowAI
{
	protected override void Attack()
	{
		base.Attack();

		PlayerControl.instance.TakeDamage(gameObject);
	}
}
