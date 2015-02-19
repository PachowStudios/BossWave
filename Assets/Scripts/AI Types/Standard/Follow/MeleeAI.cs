using UnityEngine;
using System.Collections;

public sealed class MeleeAI : FollowAI
{
	#region Internal Update Methods
	protected override void Attack()
	{
		base.Attack();

		PlayerControl.Instance.TakeDamage(gameObject);
	}
	#endregion
}
